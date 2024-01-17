/*
 * Form에 있는 Component들을 쓰기 위해 파일만 분리
 */

#define _NOT_CONNECT_PM
#define _NOT_CONNECT_GS
#undef _CONNECT_PM

using FluentFTP;
using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DabinPACT {
	public partial class frmMain : Form {

        private string _mqttFtpIp = "";
        private string _id = "root";
        private string _password = "root";
        private string _remotePath = "/run/media/mmcblk0p1/ad936x/SA/calibration/";
        private string _md5Suffix = ".md5";

        public Queue _ftpCmdQueue = new Queue();

        public async Task<bool> ftpClientUploadAsync(string fileDir, string fileName)
        {
            try
            {
                _ftpClient = new FtpClient(_mqttFtpIp);
                _ftpClient.Credentials = new NetworkCredential(_id, _password);
                _ftpClient.RetryAttempts = 3;

                await _ftpClient.ConnectAsync();
                await _ftpClient.UploadFileAsync(fileDir + fileName, _remotePath + fileName);
                await _ftpClient.UploadFileAsync(fileDir + fileName + _md5Suffix, _remotePath + fileName + _md5Suffix);
                await _ftpClient.DisconnectAsync();
            }
            catch (Exception ex)
            {
                OnFtpReceivedMessage(ex.ToString());
                return false;
            }
            return true;
        }

        private void OnFtpReceivedMessage(string message)
        {
            CallLogMessage(message);
        }

        private void EncodingFtpCmd(string fileName)
        {
            // STEP 1. CAL READY 메시지.
            _ftpCmdQueue.Enqueue("0x" + ((byte)FTP_CODE.MODE).ToString("X2") + " " + "0x" + ((byte)FTP_CODE.TYPE).ToString("X2")
                + " " + "0x" + ((byte)FTP_CODE.CAL_READY).ToString("X2"));
            // STEP 3. CAL File 'N' TRansfer Start 메시지
            _ftpCmdQueue.Enqueue("0x" + ((byte)FTP_CODE.MODE).ToString("X2") + " " + "0x" + ((byte)FTP_CODE.TYPE).ToString("X2")
                + " " + "0x" + ((byte)FTP_CODE.FILE_TRANS_START).ToString("X2") + " " + fileName);
            // STEP 4. CAL File 'N' Transfer Done 메시지
            _ftpCmdQueue.Enqueue("0x" + ((byte)FTP_CODE.MODE).ToString("X2") + " " + "0x" + ((byte)FTP_CODE.TYPE).ToString("X2")
                + " " + "0x" + ((byte)FTP_CODE.FILE_TRANS_DONE).ToString("X2") + " " + fileName);
        }

        private async Task SendFtpScenario(string fileName)
        {
            // File Name 설정
            string fileDir = Path.GetDirectoryName(Directory.GetCurrentDirectory()) + "/output/";

            // STEP 1. check CAL READY 메시지를 보낸다.
            string calReady = _ftpCmdQueue.Dequeue().ToString();
            _sendMsgMqttQueue.Enqueue(calReady);
            _receiveMsgMqttQueue.Enqueue(calReady);

            bool resultStep1 = await AsyncSendMessageMqtt();
            // STEP 2. CAL READY RESPONSE MESSAGE 확인.

            string resultMD5out = await _FileUtil.GetMD5CheckSum(fileDir, fileName);

            // STEP 3. CAL File 'N' TRansfer Start 메시지
            string fileTransStart = _ftpCmdQueue.Dequeue().ToString();
            _sendMsgMqttQueue.Enqueue(fileTransStart);
            _receiveMsgMqttQueue.Enqueue(fileTransStart);

            bool resultStep3 = await AsyncSendMessageMqtt();

            bool resultFtpFileUpload = await ftpClientUploadAsync(fileDir, fileName);

            // STEP 4. CAL File 'N' Transfer Done
            string fileTransDone = _ftpCmdQueue.Dequeue().ToString();
            _sendMsgMqttQueue.Enqueue(fileTransDone);
            _receiveMsgMqttQueue.Enqueue(fileTransDone);

            bool resultStep4 = await AsyncSendMessageMqtt();
        }
    }
}
/*
 * Form에 있는 Component들을 쓰기 위해 파일만 분리
 */

#define _NOT_CONNECT_PM
#define _NOT_CONNECT_GS
#undef _CONNECT_PM

using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DabinPACT {
	public partial class frmMain : Form {

        public string _hostNameSA = ""; // "192.168.15.4";
        public string _portSA = "";

        public TelnetConnection _tcSA;

        private Queue _sendMsgSAQueue;
        private Queue _receiveMsgSAQueue;
        private string _oneCycleMessageSA = "";

        private async Task initTcpScpiSA()
        {
            void initSAClient()
            {
                _sendMsgSAQueue = new Queue();
                _receiveMsgSAQueue = new Queue();

                _tcSA = new TelnetConnection("Signal Analyzer");
                _tcSA.Opened += OnConnectDevice;
                _tcSA.Closed += OnDisConnectDevice;

                _tcSA.ReadTimeout = CF._READ_TIMEOUT; // 15 sec

                _tcSA.Open(_hostNameSA, _portSA);
                _tcSA.messageHandler += OnSCPIMessageSA;
            }
            await Task.Run(() =>
            {
                try
                {
                    initSAClient();
                }
                catch (Exception ex)
                {
                    CallLogMessage("[SA] " + ex.ToString());
                }
            });
        }

        public void WriteSA(string s)
        {
            _tcSA.WriteLine(s);
            if (s.IndexOf('?') >= 0)
            {
                try
                {
                    _tcSA.Read();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //_tcSA.Dispose();
                }
            }
        }

        private void OnSCPIMessageSA(byte[] reponse)
        {
            string result = ASCIIEncoding.ASCII.GetString(reponse);
            if (_receiveMsgSAQueue.Count == 1)
            {
                _oneCycleMessageSA += CF.CombineMessage((string)_receiveMsgSAQueue.Dequeue(), ASCIIEncoding.ASCII.GetString(reponse));
                String message = String.Copy(_oneCycleMessageSA);
                CallLogMessageWithDt(message);
                _oneCycleMessageSA = "";
            }
            else
            {
                _oneCycleMessageSA += CF.CombineMessage((string)_receiveMsgSAQueue.Dequeue(), ASCIIEncoding.ASCII.GetString(reponse)) + "|";
                if (_sendMsgSAQueue.Count > 0) WriteSA((string)_sendMsgSAQueue.Dequeue());
            }
        }       
    }
}
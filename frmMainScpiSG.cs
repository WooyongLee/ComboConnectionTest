/*
 * Form에 있는 Component들을 쓰기 위해 파일만 분리
 */


using System;
using System.Collections;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;



namespace DabinPACT {
	public partial class frmMain : Form {

        public string _hostNameSG = ""; // SG 192.168.0.51 port 5025
        public string _portSG = "";

        public TelnetConnection _tcSG;

        private Queue _sendMsgSGQueue;
        private Queue _receiveMsgSGQueue;

        private string _oneCycleMessageSG = "";

        private async Task initTcpScpiSG()
        {
            void initSGClient()
            {
                _sendMsgSGQueue = new Queue();
                _receiveMsgSGQueue = new Queue();

                _tcSG = new TelnetConnection("Signal Generator");
                _tcSG.Opened += OnConnectDevice;
                _tcSG.Closed += OnDisConnectDevice;

                _tcSG.ReadTimeout = CF._READ_TIMEOUT; //  15 sec

                _tcSG.Open(_hostNameSG, "5025");
                _tcSG.messageHandler += OnSCPIMessageSG;
            }

            await Task.Run(() =>
            {
                try
                {
                    initSGClient();
                }
                catch (Exception ex)
                {
                    CallLogMessage("[SG] "+ ex.ToString());
                }
            });
        }

        public void WriteSG(string s)
        {
            _tcSG.WriteLine(s);
            if (s.IndexOf('?') >= 0)
            {
                try
                {
                    _tcSG.Read();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                    //_tcSG.Dispose();
                }
            }
        }

        private void OnSCPIMessageSG(byte[] reponse)
        {
            string result = ASCIIEncoding.ASCII.GetString(reponse);
            if (_receiveMsgSGQueue.Count == 1)
            {
                _oneCycleMessageSG += CF.CombineMessage((string)_receiveMsgSGQueue.Dequeue(), ASCIIEncoding.ASCII.GetString(reponse));
                String message = String.Copy(_oneCycleMessageSG);
                CallLogMessageWithDt(message);
                _oneCycleMessageSG = "";
            }
            else
            {
                _oneCycleMessageSG += CF.CombineMessage((string)_receiveMsgSGQueue.Dequeue(), ASCIIEncoding.ASCII.GetString(reponse)) + "|";
                if (_sendMsgSGQueue.Count > 0) WriteSG((string)_sendMsgSGQueue.Dequeue());
            }
        }

        private async Task SendSGFrequencyMessage(float frequency, string freqUnit)
        {
            string freqCmdSG = ":FREQ:CW " + frequency + " " + freqUnit;
            _sendMsgSGQueue.Enqueue(freqCmdSG);
            //_receiveMsgSGQueue.Enqueue(freqCmdSG);
            if (_sendMsgSGQueue.Count > 0) WriteSG((string)_sendMsgSGQueue.Dequeue());
            await Task.Delay(_waitMessageTime);
        }

        private async Task SendSGAmplitudeMessage(int amplitude, string amplitudeUnit)
        {
            string powerAttenu = ":POWer:ATTenuation:AUTO ON";
            _sendMsgSGQueue.Enqueue(powerAttenu);
            if (_sendMsgSGQueue.Count > 0) WriteSG((string)_sendMsgSGQueue.Dequeue());
            await Task.Delay(_waitMessageTime);

            string levelSet = ":POWer:LEVEL " + amplitude + amplitudeUnit; // parsing
            _sendMsgSGQueue.Enqueue(levelSet);
            if (_sendMsgSGQueue.Count > 0) WriteSG((string)_sendMsgSGQueue.Dequeue());
            await Task.Delay(_waitMessageTime);

            string rfOn = ":OUTPut ON"; // parsing
            _sendMsgSGQueue.Enqueue(rfOn);
            if (_sendMsgSGQueue.Count > 0) WriteSG((string)_sendMsgSGQueue.Dequeue());
            await Task.Delay(_waitMessageTime);
        }
    }
}
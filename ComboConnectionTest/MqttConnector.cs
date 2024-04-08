
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComboConnectionTest
{
    // MQTT 통신을 위해 연결을 초기화하는 클래스
    public class MqttConnector : IDisposable
    {
        /// <summary>
        /// Spectrum Data를 수신할 때 마다 호출하는 이벤트
        /// </summary>
        /// <param name="obj">Type : List<int>, 스펙트럼 데이터(1001개)</param>
        /// <param name="e">null</param>
        public delegate void ReceiveSpectrumData(object obj, EventArgs e);
        public event ReceiveSpectrumData ReceiveSpectrumDataEvent;

        // MainWIndow에 작성할 로그 처리용
        public event WriteLog WriteLogEvent;

        // Binary 데이터(Spectrum, Byte Array) 수신 시 True로 설정되는 플래그 
        private bool bReceivedBin = false;

        public MqttClass _mqSubscriber;
        public MqttClass _mqBinSubscriber;
        public MqttClass _mqPublisher;

        private string _mqttServerIp = "";
        private int _mqttPort = 0;

        // Topic String
        private string _mqttSendTopic = "SCAN/CMD";  // APP에서 FW로 명령어를 전송할 때
        // private readonly string _mqttSendTopic = "pact/command";  // APP에서 FW로 명령어를 전송할 때
        // private readonly string _mqttSendTopic = "vor/command";  // APP에서 FW로 명령어를 전송할 때

        private string _mqttReceiveTopic = "SCAN/TXT"; // command topic에서 받은 명령어에 대한
        // private readonly string _mqttReceiveTopic = "pact/data1"; // command topic에서 받은 명령어에 대한
                                                                  // 응답 또는 status를 보내줄 때

        private string _mqttReceiveBinTopic = "SCAN/DAT"; // 0x11(or 0x23)” 명령어가 pact/command로
        // private readonly string _mqttReceiveBinTopic = "pact/data2"; // 0x11(or 0x23)” 명령어가 pact/command로
                                                                  // 보내졌을 때 그래프에 출력될 데이터를
                                                                  // 바이너리형태로 보내줌.


        private readonly int _waitPACTTime = 50; // 50 ms PACT 메시지 전송 후, wait time

        public delegate void ConnectDone(object obj, EventArgs args);
        public event ConnectDone ConnectDoneEvent;

        public MqttConnector(string strIP, int port)
        {
            this._mqttServerIp = strIP;
            this._mqttPort = port;
        }


        public async Task InitMqttClient(string topic = "")
        {
            if (topic.ToLower().Contains("scan"))
            {
                _mqttSendTopic = "SCAN/CMD";
                _mqttReceiveTopic = "SCAN/TXT";
                _mqttReceiveBinTopic = "SCAN/DAT";
            }

            else if (topic.ToLower().Contains("pact"))
            {
                _mqttSendTopic = "pact/command";
                _mqttReceiveTopic = "pact/data1";
                _mqttReceiveBinTopic = "pact/data2";
            }

            // (pact/command)
            void initPublisher()
            {
                _mqPublisher = new MqttClass();
                _mqPublisher.messageHandler += OnCheckConnectionMessage;
                _mqPublisher.receivedMessageHandler += OnPublisherReceivedMessage;
                _mqPublisher.BuildOptions(_mqttServerIp, _mqttPort);
                _mqPublisher.Connect(_mqttSendTopic);
            }

            void initSubscriber()
            {
                _mqSubscriber = new MqttClass();
                _mqSubscriber.messageHandler += OnCheckConnectionMessage;
                _mqSubscriber.receivedMessageHandler += OnSubscriberReceivedMessage;
                _mqSubscriber.BuildOptions(_mqttServerIp, _mqttPort);
                _mqSubscriber.Connect(_mqttReceiveTopic);
                _mqSubscriber.ReceiveMessage();
            }

            void initSubscriberBin()
            {
                _mqBinSubscriber = new MqttClass();
                _mqBinSubscriber.messageHandler += OnCheckConnectionMessage;
                _mqBinSubscriber.receivedMessageHandler += OnSubscriberBinReceivedMessage;
                _mqBinSubscriber.BuildOptions (_mqttServerIp, _mqttPort);
                _mqBinSubscriber.Connect(_mqttReceiveBinTopic);
                _mqBinSubscriber.ReceiveMessage();
            }


            // 각각의 MQTT Client 초기화 비동기처리
            await Task.Run(() =>
            {
                initPublisher();
                initSubscriber();
                initSubscriberBin();

                bool done = false;
                var watch = Stopwatch.StartNew();

                while (!done)
                {
                    done = _mqPublisher.MqttClient.IsConnected | _mqSubscriber.MqttClient.IsConnected; // To DO :: Check Subcriber Connected 
                    Task.Delay(_waitPACTTime);

                    // 연결 시도 후 일정 시간 지날 시 시도 종료
                    // To Do :: 시도 중에 Connect 버튼 비활성화, 시도 후 실패 시 Connect 버튼 활성화 이벤트 추가할 것
                    if ( watch.ElapsedMilliseconds > (long)CommonFunctions.READ_TIMEOUT * 2)
                    {
                        watch.Stop();
                        done = true;
                        WriteLogEvent("Connected Fail, Time Out");
                    }
                }

                // Connect 시도 끝났을 때 MainWindow로 전달
                if (ConnectDoneEvent != null)
                {
                    ConnectDoneEvent(this, null);
                }
            });
        }

        // 구독자 관점 바이너리 데이터 수신 처리 이벤트 함수
        int cnt = 0;
        private void OnSubscriberBinReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            List<float> recvSpectrumList = new List<float>();

            const int CntOfHeader = 2;
            const int FourByte = 4;

            int MAX_SPECTRUM_NUM = 1000;
            byte[] receivedSpectrum = e.ApplicationMessage.Payload;
            Int32[] _spectrumData = new Int32[MAX_SPECTRUM_NUM];
            StringBuilder sb = new StringBuilder();

           //  WriteLogEvent(e.ApplicationMessage.Payload.ToString());

            if (e.ApplicationMessage.Payload.Length < 1999)
            {
                if (_mqttReceiveBinTopic.Contains("SCAN"))
                {
                    var scanMode = receivedSpectrum[0];
                    var scanType = receivedSpectrum[1];
                    var numOfCell= receivedSpectrum[2];
                    var strScannerLog = "scanMode = " + scanMode + ", scanType = " + scanType + ", numOfCell = " + numOfCell;
                    // WriteLogEvent(strScannerLog);
                }

                WriteLogEvent("Recv 5gnr Binary");

                return;
            }

            var mode = BitConverter.ToInt32(receivedSpectrum, 0 * FourByte);
            var type = BitConverter.ToInt32(receivedSpectrum, 1 * FourByte);

            for ( int i = 0; i < MAX_SPECTRUM_NUM; i++ )
            {
                if ((i + 2) * 4 >= receivedSpectrum.Length)
                {
                    break;
                }

                _spectrumData[i] = BitConverter.ToInt32(receivedSpectrum, (i + CntOfHeader) * FourByte);
                if ( i < 5)
                {
                    sb.Append(_spectrumData[i]);
                    sb.Append(" ");
                }

                // 스펙트럼 데이터가 100을 곱해서 들어옴
                float spectrumData = (float)BitConverter.ToInt32(receivedSpectrum, (i + CntOfHeader) * FourByte) / 100;
                recvSpectrumList.Add(spectrumData);
            }
            int maxSpectrum = _spectrumData.Max(); ;

            if (ReceiveSpectrumDataEvent != null)
            {
                ReceiveSpectrumDataEvent(recvSpectrumList, null);
            }

            // 맨 뒤에서 부터 
            int LengthOfPayload = e.ApplicationMessage.Payload.Length;
            var timestamp = BitConverter.ToInt32(receivedSpectrum, LengthOfPayload - 1 * 4);
            var currentPartialDataIndex = BitConverter.ToInt32(receivedSpectrum, LengthOfPayload - 2 * 4);
            var totalPartialCnt = BitConverter.ToInt32(receivedSpectrum, LengthOfPayload - 3 * 4);

            string strSpectrumMessage = sb.ToString();

            string strLog = string.Format("Cnt Of Payload {0} ", receivedSpectrum.Length);
            // WriteLogEvent(strLog);

            // Spectrum에 0.01 곱해준 값
            double doubledMaxSpectrum = (double)maxSpectrum / 100.0;

            // 임시 로그 출력
            // strLog = string.Format("Binary Subscriber Receive Max = {0} ... , timestamp : {1}", doubledMaxSpectrum.ToString(), timestamp);
            strLog = string.Format("Binary Subscriber Receive Max = {0} ", doubledMaxSpectrum.ToString());
            WriteLogEvent(strLog);
            bReceivedBin = true;
        }

        // 구독자 관점 메시지 수신 처리 이벤트 함수
        private void OnSubscriberReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string strLog = string.Empty;

            // 추가적으로 Receive Queue를 점검하고 여러 조건에 따라 설정할 것들이 있지만 (ex. Temperature, IQImb 등)
            // 일단 수신하는 지 확인하기 위해 로그만 찍어줌

            string recvMsgPayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            // Subscriber Receive Message: 0x06 0x02 20 3.537598
            // [2022 - 02 - 21 17:08:25] Subscriber Receive Message: 0x06 0x01
            // [2022 - 02 - 21 17:08:25] Subscriber Receive Message: 0x51 0
            // [2022 - 02 - 21 17:08:26] Subscriber Receive Message: 0x51 0

            // Ignore Message
            if ( recvMsgPayload.Contains("0x06 0x01") || recvMsgPayload.Contains("0x51 0") || recvMsgPayload.Contains("0x25 0") || recvMsgPayload.Contains("0x30"))
            {
                return;
            }

            // Ignore Auto Atten Message
            if (recvMsgPayload.Contains("0x26") || recvMsgPayload.Contains("0x24"))
            {
                return;
            }

            // 배터리 메시지
            string batMsg = "0x06 0x02";
            if (recvMsgPayload.Contains(batMsg))
            {
                // strLog  =  recvMsgPayload.Replace(batMsg , "");
                return;
            }

            else
            {
                strLog = string.Format("Subscriber Receive Message : {0}", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
            }
            
            WriteLogEvent(strLog);
        }

        private void OnPublisherReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            string strLog = string.Format("Publisher Receive Message : {0}", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

            WriteLogEvent(strLog);
        }

        private void OnCheckConnectionMessage(string error)
        {
            WriteLogEvent(error);
        }

        public async Task SendRequestSpectrumCmd(Queue queue, bool bSendAck = false)
        {
            string execStr = "";
            while (queue.Count != 0)
            {
                execStr += queue.Dequeue().ToString() + CommonFunctions.BLANK;
            }
            var payload = _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(execStr));
            _mqPublisher.SendMessage();
            string strLog = string.Format("Send Message : {0}", Encoding.UTF8.GetString(payload));
            WriteLogEvent(strLog);

            await Task.Delay(_waitPACTTime); // 잠시 대기 후

            if (bSendAck)
            {
                _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes("0x11"));
                _mqPublisher.SendMessage();
                strLog = string.Format("Send Message : {0}", "0x11");
                WriteLogEvent(strLog);
            }

            await Task.Delay(_waitPACTTime); // 잠시 대기 후

            // TimeStamp 송신 (0x12로)
            string timestampCmd = "0x12" + " " + DateTime.Now.ToString("HHmmssffff");
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(timestampCmd));
            _mqPublisher.SendMessage();
            strLog = string.Format("Send Message : {0}", timestampCmd);
            WriteLogEvent(strLog);

            await Task.Run(async () =>
            {
                while (!bReceivedBin)
                {
                    await Task.Delay(10);
                }
                bReceivedBin = false;
            });
        }

        public async Task SendNormalCmd(Queue queue, bool bSendAck = false)
        {
            string execStr = "";
            while( queue.Count != 0)
            {
                execStr += queue.Dequeue().ToString() + CommonFunctions.BLANK;
            }
            var payload = _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes(execStr));
            _mqPublisher.SendMessage();
            string strLog = string.Format("Send Message : {0}", Encoding.UTF8.GetString(payload));
            WriteLogEvent(strLog);

            await Task.Delay(_waitPACTTime); // 잠시 대기 후

            if (bSendAck)
            {
                _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes("0x11"));
                _mqPublisher.SendMessage();
                strLog = string.Format("Send Message : {0}", "0x11");
                WriteLogEvent(strLog);
            }

            await Task.Delay(_waitPACTTime); // 잠시 대기 후
        }

        public void Dispose()
        {
            _mqSubscriber.MqttClient.DisconnectAsync();
            _mqBinSubscriber.MqttClient.DisconnectAsync();
            _mqPublisher.MqttClient.DisconnectAsync();

            _mqSubscriber.MqttClient.Dispose();
            _mqBinSubscriber.MqttClient.Dispose();
            _mqPublisher.MqttClient.Dispose();
        }
    }
}

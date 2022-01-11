
using MQTTnet;
using System;
using System.Collections;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ComboConnectionTest
{
    // MQTT 통신을 위해 연결을 초기화하는 클래스
    public class MqttConnector
    {
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
        private readonly string _mqttSendTopic = "pact/command";  // APP에서 FW로 명령어를 전송할 때

        private readonly string _mqttReceiveTopic = "pact/data1"; // command topic에서 받은 명령어에 대한
                                                                  // 응답 또는 status를 보내줄 때

        private readonly string _mqttReceiveBinTopic = "pact/data2"; // 0x11(or 0x23)” 명령어가 pact/command로
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


        public async Task InitMqttClient()
        {
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
        private void OnSubscriberBinReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            byte[] receivedSpectrum = e.ApplicationMessage.Payload;
            Int32[] _spectrumData = new Int32[1001];
            StringBuilder sb = new StringBuilder();
            for ( int i = 0; i < _spectrumData.Length; i++ )
            {
                _spectrumData[i] = BitConverter.ToInt32(receivedSpectrum, (i + 2) * 4);
                if ( i < 5)
                {
                    sb.Append(_spectrumData[i]);
                    sb.Append(" ");
                }
            }
            string strSpectrumMessage = sb.ToString();

            // 임시 로그 출력
            string strLog = string.Format("Binary Subscriber Receive Message {0} ... , Length : {1}", strSpectrumMessage, receivedSpectrum.Length);
            WriteLogEvent(strLog);
            bReceivedBin = true;
        }

        // 구독자 관점 메시지 수신 처리 이벤트 함수
        private void OnSubscriberReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            // 추가적으로 Receive Queue를 점검하고 여러 조건에 따라 설정할 것들이 있지만 (ex. Temperature, IQImb 등)
            // 일단 수신하는 지 확인하기 위해 로그만 찍어줌
            string strLog = string.Format("Subscriber Receive Message : {0}", Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
          
            string recvMsgPayload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            if (recvMsgPayload.Contains("0x30"))
            {
                int a = 0;
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

        public async Task SendExecGlobalOffsetMessageToPACT(Queue queue)
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
            _mqPublisher.BuildMessage(_mqttSendTopic, Encoding.ASCII.GetBytes("0x11"));
            _mqPublisher.SendMessage();
            strLog = string.Format("Send Message : {0}", Encoding.UTF8.GetString(payload));
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
    }
}

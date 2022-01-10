
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

        public MqttClass _mqSubscriber;
        public MqttClass _mqBinSubscriber;
        public MqttClass _mqPublisher;

        // Send&Receive Queue
        private Queue _sendMsgMqttQueue;
        private Queue _receiveMsgMqttQueue;
        private Queue _send5GMsgMqttQueue;

        private string _mqttServerIp = "";
        private int _mqttPort = 0;

        // Topic String
        private readonly string _mqttSendTopic = "pact/command";
        private readonly string _mqttReceiveTopic = "pact/data1";
        
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
            _sendMsgMqttQueue = new Queue();
            _receiveMsgMqttQueue = new Queue();
            _send5GMsgMqttQueue = new Queue();

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

            // To Do :: Subscriber 부분 초기화 구현할 것


            // 각각의 MQTT Client 초기화 비동기처리
            await Task.Run(() =>
            {
                initPublisher();
                initSubscriber();

                bool done = false;
                var watch = Stopwatch.StartNew();

                while (!done)
                {
                    done = _mqPublisher.MqttClient.IsConnected | _mqSubscriber.MqttClient.IsConnected; // To DO :: Check Subcriber Connected 
                    Task.Delay(_waitPACTTime);

                    // 연결 시도 후 일정 시간 지날 시 시도 종료
                    // To Do :: 시도 중에 Connect 버튼 비활성화, 시도 후 실패 시 Connect 버튼 활성화 이벤트 추가할 것
                    if ( watch.ElapsedMilliseconds > (long)CommonFunctions._READ_TIMEOUT * 2)
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

        // 구독자 관점 메시지 수신 처리 이벤트 함수
        private void OnSubscriberReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            
        }

        private void OnPublisherReceivedMessage(MqttApplicationMessageReceivedEventArgs e)
        {
            WriteLogEvent(Encoding.UTF8.GetString(e.ApplicationMessage.Payload));
        }

        private void OnCheckConnectionMessage(string error)
        {
            WriteLogEvent(error);
        }
    }
}

using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Extensions.Rpc.Options;
using MQTTnet.LowLevelClient;
using MQTTnet.Protocol;
using MQTTnet.Server;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace DabinPACT {
    public class MQTTClass {
        MqttFactory _factory;
        IMqttClientOptions _options;
        public IMqttClient _mqttClient;
        //ILowLevelMqttClient _llmqttClient;
        MqttApplicationMessage _message;
        CancellationToken _cancellationToken;
        public HandleMessage messageHandler;
        public HandleMessageReceived receivedMessageHandler;
        int tryCount = 0;

        // RPC용
        MqttRpcClient _rpcClient;
        IMqttRpcClientOptions _rpcOptions;
        TimeSpan        _timeout;
        MqttQualityOfServiceLevel    _qos;   // enum MqttQualityOfServiceLevel
        Task<byte[]>    _response;

        //public MQTTClass(frmMain fm) {
        public MQTTClass() {
            _factory = new MqttFactory();
            _mqttClient = _factory.CreateMqttClient();
            //_llmqttClient = _factory.CreateLowLevelMqttClient();
            _cancellationToken = new CancellationToken();
        }

        // RPC용. BuildRPCOptions 다음에 불러야한다.
        public void Subscribe() {
            _rpcClient = new MqttRpcClient(_mqttClient, _rpcOptions);
            _timeout = TimeSpan.FromSeconds(20);
            _qos = MqttQualityOfServiceLevel.AtMostOnce;
            _response = _rpcClient.ExecuteAsync(_timeout, "myMethod", "payload", _qos); // 이 자체가 대기 후 오는 것 같다. 확인 필요 ^^^
        }

        public void BuildRPCOptions() {
            _rpcOptions = new MqttRpcClientOptionsBuilder().Build();
        }

        // Protocol 형식에 따라 Over load 해도 됨
        public void BuildOptions(string ip, int port, string id, string name, string pwd, string websocket) {
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .WithTls()
                .WithCleanSession()
                .WithClientId(id)
                .WithCredentials(name, pwd)
                .WithWebSocketServer(websocket)
                .Build();
        }

        public void BuildOptions(string ip, int port)
        {
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .WithCleanSession()
                .Build();
        }

        // Protocol 형식에 따라 Over load 해도 됨
        public void BuildMessage(string topic, byte[] payload) {
            _message= new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();
        }

        public async void Connect(string topic) {
            _mqttClient.UseDisconnectedHandler(async e => {
                messageHandler("===== DisConnected MQTT Broker " + topic + " =====");
                await Task.Delay(TimeSpan.FromSeconds(5));
                tryCount++;
                if (tryCount < 3)
                {
                    try
                    {
                        await _mqttClient.ConnectAsync(_options, _cancellationToken);
                    }
                    catch
                    {
                        Console.WriteLine("MQTT Connection Error: Even Retring After 5 secs");
                    }
                }
            });

            _mqttClient.UseConnectedHandler(async e => {
                messageHandler("===== Connected MQTT Broker " + topic + " =====");
                await _mqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
            });

            tryCount = 0;
            
            try
            {
                await _mqttClient.ConnectAsync(_options, _cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public async void SendMessage() {
            await _mqttClient.PublishAsync(_message, _cancellationToken);
        }

        public async void Disconnect()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }
        }

        public void ReceiveMessage() {
            _mqttClient.UseApplicationMessageReceivedHandler(e => {
                if (e.ApplicationMessage.Payload != null)
                {
                    receivedMessageHandler(e);
                }
            });
        }
    }
}

﻿using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Internal;
using MQTTnet.Packets;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ComboConnectionTest
{
    // Mqtt Client 관점
    public class MqttClass
    {
        private MqttFactory _factory; // MQTT Client를 생성하기 위한 Factory Class
        private MqttClientOptions _options; // 비동기 연결에 필요한 옵션 파라미터에 들어감
        private MqttApplicationMessage _message; // Client가 구독시도하는 메시지
        private CancellationToken _cancellationToken; // 비동기 호출을 취소

        public IMqttClient MqttClient; // MQTT Client
        public HandleMessage messageHandler;
        public HandleMessageReceived receivedMessageHandler;

        public MqttClass()
        {
            _factory = new MqttFactory();
            MqttClient = _factory.CreateMqttClient(); 
            _cancellationToken = new CancellationToken();

            // MqttClient.DisconnectedHandler

        }

        // 옵션 빌더를 통해 MQTT 연결 옵션을 지정한다.
        public void BuildOptions(string ip, int port)
        {
            _options = new MqttClientOptionsBuilder()
                .WithTcpServer(ip, port)
                .WithCleanSession()
                .Build();
        }

        // 메시지 빌더를 통해 MQTT 메시지를 구성한다.
        public byte[] BuildMessage(string topic, byte[] payload)
        {
            _message = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithRetainFlag()
                .Build();

            return _message.Payload;
        }

        int tryCount = 0;
        public async void Connect(string topic)
        {
            #region old
            // tryCount = 0;
            //MqttClient.UseDisconnectedHandler(async e =>
            //{
            //    messageHandler("Disconnected MQTT Broker " + topic);
            //    await Task.Delay(TimeSpan.FromSeconds(5));
            //    tryCount++;
            //    if ( tryCount < 3 )
            //    {
            //        try
            //        {
            //            // Try to Re Connect
            //            await MqttClient.ConnectAsync(_options, _cancellationToken);
            //        }
            //        catch (Exception ex)
            //        {
            //            Console.WriteLine(ex.ToString());
            //            // To Do :: Write Some Message at Console
            //        }
            //    }
            //});

            //MqttClient.UseConnectedHandler(async e =>
            //{
            //    messageHandler("Connected MQTT Broker " + topic);
            //    await MqttClient.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build());
            //});
            #endregion

            MqttClient.ConnectedAsync += async e =>
            {
                await MqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithExactlyOnceQoS()
                    .WithTopic(topic).Build());

                // or
                //await MqttClient.SubscribeAsync(
                //    new MqttClientSubscribeOptions
                //    {
                //        TopicFilters = new List<MqttTopicFilter> { new MqttTopicFilter { Topic = topic } }
                //    },
                //    CancellationToken.None);

                messageHandler("Connected MQTT Broker " + topic);
            };

            MqttClient.DisconnectedAsync += e =>
            {
                var ex = e.Exception;
                messageHandler("Disconnected MQTT Broker " + topic);

                return CompletedTask.Instance;
            };

            try
            {
                await MqttClient.ConnectAsync(_options, _cancellationToken).ConfigureAwait(false);

            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                messageHandler("Connect Exception " + ex.ToString());
            }
        }

        public async void SendMessage()
        {
            if (MqttClient.IsConnected)
            {
                // Send ~~ Publish
                await MqttClient.PublishAsync(_message, _cancellationToken);
            }
        }


        // 구독자 관점에서, 메시지 핸들러를 통해 수신
        public void ReceiveMessage()
        {
            MqttClient.ApplicationMessageReceivedAsync += e =>
            {
                if (e.ApplicationMessage.Payload != null)
                {
                    receivedMessageHandler(e);
                }

                return Task.CompletedTask;
            };

        }
    }
}

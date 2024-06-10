using MQTTnet.Diagnostics;
using MQTTnet.Server;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MQTTnet.Internal;
using MQTTnet.Protocol;
using System.IO;
using Newtonsoft.Json;
using Path = System.IO.Path;

namespace ComboBrokerTest
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }



        #region legacy, mqttlib samples
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // Task.Run(Publish_Message_From_Broker);
            Task.Run(Run_Minimal_Server);
        }

        private void Send_Message_Button_Click(object sender, RoutedEventArgs e)
        {
            Task.Run(Publish_Message_From_Broker);
        }

        public static async Task Force_Disconnecting_Client()
        {
            /*
             * This sample will disconnect a client.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Let the client connect.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Now disconnect the client (if connected).
                var affectedClient = (await mqttServer.GetClientsAsync()).FirstOrDefault(c => c.Id == "MyClient");
                if (affectedClient != null)
                {
                    await affectedClient.DisconnectAsync();
                }
            }
        }


        public static async Task Publish_Message_From_Broker()
        {
            /*
             * This sample will publish a message directly at the broker.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Create a new message using the builder as usual.
                var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();

                // Now inject the new message at the broker.
                await mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = "SenderClientId"
                    });
            }
        }

        public static async Task Run_Minimal_Server()
        {
            /*
             * This sample starts a simple MQTT server which will accept any TCP connection.
             */

            var mqttFactory = new MqttFactory();

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            // The port can be changed using the following API (not used in this example).
            // new MqttServerOptionsBuilder()
            //     .WithDefaultEndpoint()
            //     .WithDefaultEndpointPort(1234)
            //     .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();

                mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        private static Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            Console.WriteLine(string.Format("{0} {1} {2}", arg.ClientId, arg.UserName, arg.Endpoint));
            return Task.CompletedTask;
        }

        public static async Task Run_Server_With_Logging()
        {
            /*
             * This sample starts a simple MQTT server and prints the logs to the output.
             *
             * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
             *
             * See sample "Run_Minimal_Server" for more details.
             */

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        public static async Task Validating_Connections()
        {
            /*
             * This sample starts a simple MQTT server which will check for valid credentials and client ID.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Setup connection validation before starting the server so that there is 
                // no change to connect without valid credentials.
                mqttServer.ValidatingConnectionAsync += e =>
                {
                    if (e.ClientId != "ValidClientId")
                    {
                        e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    }

                    if (e.UserName != "ValidUser")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    if (e.Password != "SecretPassword")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                };

                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }

        static async Task<MqttServer> StartMqttServer()
        {
            var mqttFactory = new MqttFactory();

            // Due to security reasons the "default" endpoint (which is unencrypted) is not enabled by default!
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder().WithDefaultEndpoint().Build();
            var server = mqttFactory.CreateMqttServer(mqttServerOptions);
            await server.StartAsync();
            return server;
        }
        #endregion
    }
}

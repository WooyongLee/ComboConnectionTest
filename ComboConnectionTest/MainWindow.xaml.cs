
using MQTTnet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace ComboConnectionTest
{
    // Message Handle을 위한 델리게이트 정의
    public delegate void HandleMessage(string error);
    public delegate void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e);

    public delegate void WriteLog(string log);

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private string LogMsg = "";
        
        MqttConnector Connector;
        JsonManager JsonMng;
        public MainWindow()
        {
            InitializeComponent();

            JsonMng = new JsonManager();
        }

        private async void ConButton_Click(object sender, RoutedEventArgs e)
        {
            string strIp = InputIPTextBox.Text;
            int port = int.Parse(InputPortTextBox.Text);
            Connector = new MqttConnector(strIp, port);
            Connector.WriteLogEvent += Connetor_WriteLogEvent;
            Connector.ConnectDoneEvent += Connector_ConnectDoneEvent;

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                ConButton.Content = "Wait to Connect";
                ConButton.IsEnabled = false;
            }));

            await Connector.InitMqttClient();
        }

        private void Connector_ConnectDoneEvent(object obj, EventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConButton.Content = "Connect";
                ConButton.IsEnabled = true;
            }));
        }

        // MQTT Connector에서 올라온 로그를 처리하는 이벤트 함수
        private void Connetor_WriteLogEvent(string log)
        {
            string strCurDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss");

            LogMsg += String.Format("[{0}] {1}\n", strCurDateTime, log);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LogTextBox.Text = LogMsg;
            }));
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // To Do :: Connect 완료 시에만 Send 버튼 활성화하기

            // 임시로 Global Offset 노드를 고정하여 테스트함
            string tempNodeName = "nodGlobalOffset";

            List<ParamSetJson> paramSets = JsonMng.GetParams(tempNodeName);
            Hashtable hashtable = new Hashtable();
            if (paramSets != null)
            {
                // ParamSets로 받은 (Deserialized 된)데이터들 HashTable에 모두 추가
                foreach (ParamSetJson paramSet in paramSets)
                {
                    hashtable.Add(paramSet.Name, paramSet);
                }
            }

            MqttMsgSender MsgSender = new MqttMsgSender();
            await MsgSender.SendMessage(hashtable);
        }
    }
}

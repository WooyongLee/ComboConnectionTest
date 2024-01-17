using LiveCharts.Wpf;
using MQTTnet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using static ComboConnectionTest.TextFileManager;

namespace ComboConnectionTest
{
    // Message Handle을 위한 델리게이트 정의
    public delegate void HandleMessage(string error);
    public delegate void HandleMessageReceived(MqttApplicationMessageReceivedEventArgs e);

    public delegate void WriteLog(string log);

    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window, IDisposable
    {
        private string LogMsg = "";

        ChartViewerWindow ChartViewWin;

        MqttConnector Connector;

        string spCsvFilePath = "spectrum_data_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv"; // 저장할 CSV 파일 경로 및 이름

        public MainWindow()
        {
            InitializeComponent();

            double freqMHz = 4000.000001; // mhz
            int THOUSAND = 1000;
            // ulong result = (ulong)(freqMHz * THOUSAND * THOUSAND);
            ulong result =  (ulong)Math.Round(freqMHz * (double)THOUSAND * (double)THOUSAND);
            Console.WriteLine("result = " + result);



            ChartViewWin = new ChartViewerWindow();

            TextFileWriter.MakeLogTextFile();

            // string strMac = GetMacAddress("192.168.10.6");
            // Console.WriteLine(strMac); // 00:0A:35:00:1E:62

            // SearchIP();

            // Do();
        }

        public void Do()
        {
            IPHostEntry GetIP = Dns.GetHostEntry(Dns.GetHostName());
            string ipAddr = string.Empty;
            string strMac = string.Empty;
            for (int i = 0; i < GetIP.AddressList.Length; i++)
            {
                if (GetIP.AddressList[i].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ipAddr = GetIP.AddressList[i].ToString();
                    strMac = GetMac(ipAddr);
                    Console.WriteLine(ipAddr + " => " + strMac);
                }
            }
            strMac = GetMac("192.168.10.6");
            Console.WriteLine("192.168.10.6" + " => " + strMac);
        }

        public List<string> SearchIP()
        {
            List<string> ipList = new List<string>();

            for (int i = 2; i < 255; i++)
            {
                string currentIP = "192.168.10." + i.ToString();
                IPAddress currentIPAddress = IPAddress.Parse(currentIP);

                Ping ping = new Ping();
                PingOptions pintOptions = new PingOptions();

                pintOptions.DontFragment = true;
                string message = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                byte[] messageByteArray = Encoding.ASCII.GetBytes(message);

                int timeout = 150;

                PingReply pintReply = ping.Send(currentIPAddress, timeout, messageByteArray, pintOptions);

                string macAddress = null;

                if (pintReply.Status == IPStatus.Success)
                {
                    ipList.Add(currentIP); //  Available IP
                    macAddress = GetMacAddress(currentIPAddress.ToString());

                    if ( macAddress == "00:0A:35:00:1E:62")
                    {
                        Console.WriteLine(currentIP + " =>" + macAddress);
                        break;
                    }
                }
            }

            return ipList;
        }

        public string GetMacAddress(string ipAddress)
        {
            string macAddress = string.Empty;
            System.Diagnostics.Process pProcess = new System.Diagnostics.Process();
            pProcess.StartInfo.FileName = "arp";
            pProcess.StartInfo.Arguments = "-a " + ipAddress;
            pProcess.StartInfo.UseShellExecute = false;
            pProcess.StartInfo.RedirectStandardOutput = true;
            pProcess.StartInfo.CreateNoWindow = true;
            pProcess.Start();
            string strOutput = pProcess.StandardOutput.ReadToEnd();
            string[] substrings = strOutput.ToUpper().Split('-');
            if (substrings.Length >= 8)
            {
                macAddress = substrings[3].Substring(Math.Max(0, substrings[3].Length - 2))
                         + ":" + substrings[4] + ":" + substrings[5] + ":" + substrings[6]
                         + ":" + substrings[7] + ":"
                         + substrings[8].Substring(0, 2);
                return macAddress;
            }

            else
            {
                return "not found";
            }
        }

        public string GetMac(string ip)
        {
            string ret = string.Empty;
            ObjectQuery oq = new System.Management.ObjectQuery("SELECT * FROM Win32_NetworkAdapterConfiguration WHERE IPEnabled='TRUE'");
            var query1 = new ManagementObjectSearcher(oq);
            foreach(var mo in query1.Get())
            {
                string[] address = (string[])mo["IPAddress"];
                if (address[0] == ip && mo["MACAddress"] != null)
                {
                    ret = mo["MACAddress"].ToString();
                    break;
                }
            }
            return ret;
        }

        int cnt = 0;
        private void SaveSpectrumDataToCSV(List<float> spectrumDataList)
        {
            // CSV 파일에 데이터를 저장하기 위해 StreamWriter를 사용합니다.
            using (StreamWriter writer = new StreamWriter(spCsvFilePath, true))
            {
                writer.Write(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff"));
                writer.Write(",");

                writer.Write(cnt++);
                writer.Write(",");
                // spectrumDataList의 각 요소를 CSV 파일에 쓰기합니다.
                foreach (float data in spectrumDataList)
                {
                    writer.Write(data.ToString());
                    writer.Write(",");
                }

                // 한 줄의 데이터를 모두 썼으면 줄바꿈 문자를 추가합니다.
                writer.WriteLine();
            }
        }

        private async void ConButton_Click(object sender, RoutedEventArgs e)
        {
            string strIp = InputIPTextBox.Text;
            int port = int.Parse(InputPortTextBox.Text);
            Connector = new MqttConnector(strIp, port);
            Connector.WriteLogEvent += Connetor_WriteLogEvent;
            Connector.ConnectDoneEvent += Connector_ConnectDoneEvent;
            Connector.ReceiveSpectrumDataEvent += Connector_ReceiveSpectrumDataEvent;

            await Dispatcher.BeginInvoke(new Action(() =>
            {
                ConButton.Content = " Wait to Connect ";
                ConButton.IsEnabled = false;
            }));

            var selectedTopicItem = TopicCombobox.SelectedItem.ToString();

            await Connector.InitMqttClient(selectedTopicItem);
        }

        private void Connector_ReceiveSpectrumDataEvent(object obj, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                List<float> spectrumData = (List<float>)obj;
                ChartViewWin.SetChartData(spectrumData);
                
                SaveSpectrumDataToCSV(spectrumData);
            }));

        }

        private void Connector_ConnectDoneEvent(object obj, EventArgs args)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                ConButton.Content = " Connected Done ";
                ConButton.IsEnabled = false;
            }));
        }

        // MQTT Connector에서 올라온 로그를 처리하는 이벤트 함수
        private void Connetor_WriteLogEvent(string log)
        {
            string strCurDateTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss:fff");

            string dateTimeLog = String.Format("[{0}] {1}", strCurDateTime, log);
            TextFileWriter.WriteLog(dateTimeLog);

            LogMsg += (dateTimeLog  + "\n");
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LogTextBox.Text = LogMsg;
                var length = LogMsg.Length;
                if (length> 1)
                {
                    LogTextBox.CaretIndex = length - 1;
                }
                LogTextBox.ScrollToEnd();
            }));

            // 버퍼 30000 넘어가면 삭제
            if (LogMsg.Length >= 30000)
            {
                LogMsg = string.Empty;
            }
        }

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            // To Do :: Connect 완료 시에만 Send 버튼 활성화하기

            // 임시로 Global Offset 노드를 고정하여 테스트함
            Queue sendQueue = MqttMsgMaker.SendMessage(SendText.Text);
            if (sendQueue != null)
            {
                await Connector.SendNormalCmd(sendQueue);
            }
        }

        // 텍스트 로그 삭제 버튼 클릭 이벤트 함수
        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                LogTextBox.Text = "";
                LogMsg = "";
            }));
        }

        private async void SendCmdButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;
            const string FixedValuesPadding = "0 0 0 1 0 2 0 0 2 0 0 2 0 1 47000 0 0 10000 1 0 10000 0 800 0 1 100 0 0 0";

            Queue sendQueue = new Queue();
            if (button.Name.Contains("Spectrum"))
            {
                sendQueue.Enqueue("0x04 0x00");
                sendQueue.Enqueue(double.Parse(FreqTextBox.Text));
                sendQueue.Enqueue(int.Parse(SpanTextBox.Text));
                sendQueue.Enqueue("10");
                sendQueue.Enqueue("10");
                sendQueue.Enqueue("0");
                sendQueue.Enqueue(int.Parse(AttenTextBox.Text));
                sendQueue.Enqueue("0");
                sendQueue.Enqueue(FixedValuesPadding);

                ChartViewWin.SetYaxisMinMax(double.Parse(RefLvTextBox.Text));

                await Connector.SendRequestSpectrumCmd(sendQueue, true);
            }

            else if (button.Name.Contains("IQ"))
            {
                sendQueue.Enqueue("0x43");
                sendQueue.Enqueue("3650010000");
                sendQueue.Enqueue("0");
                sendQueue.Enqueue("0"); // atten = 0
                sendQueue.Enqueue("0"); // preamp off = 0
                sendQueue.Enqueue("3"); // SG Freq Offset = 3
                sendQueue.Enqueue("1000"); // Carrier Threshold = 1 x 1000
                sendQueue.Enqueue("1000"); // Image Threshold = 1 x 1000

                await Connector.SendNormalCmd(sendQueue);
            }

            else if (button.Name.Contains("CalMode"))
            {
                sendQueue.Enqueue("0x65 7");
                await Connector.SendNormalCmd(sendQueue, true);
            }

            else if (button.Name.Contains("FTP"))
            {

            }
        }

        private void SpectrumViewButton_Click(object sender, RoutedEventArgs e)
        {
            ChartViewWin.Show();
        }

        public void Dispose()
        {
            ChartViewWin.Dispose();
            TextFileWriter.CloseFile();
            if (Connector != null)
            {
                Connector.Dispose();

            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.Dispose();

            Application.Current.Shutdown();
        }
    }
}

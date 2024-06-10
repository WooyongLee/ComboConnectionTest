using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MQTTnet;
using MQTTnet.Server;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace ComboBrokerTest.ViewModel
{
    public partial class MainViewModel : ObservableObject
    {
        private MqttServer _mqttServer;
        private bool _isRunning;

        #region Properties

        private bool _isMenuVisible;

        public bool IsMenuVisible
        {
            get => _isMenuVisible;
            set => SetProperty(ref _isMenuVisible, value);
        }


        private string _serverStatus;

        public string ServerStatus
        {
            get => _serverStatus;
            set => SetProperty(ref _serverStatus, value);
        }

        private string _logMessage;

        public string LogMessage
        {
            get => _logMessage;
            set => SetProperty(ref _logMessage, value);
        }

        #endregion

        #region Commands

        public IRelayCommand ToggleServerCommand { get; }

        public IRelayCommand ToggleMenuVisibilityCommand { get; }

        public IRelayCommand SettingMenuClickCommand { get; }

        public IRelayCommand PowerSwitchCommand { get; }


        #endregion

        public MainViewModel()
        {
            _isRunning = false;
            ServerStatus = "Stopped";
            ToggleServerCommand = new RelayCommand(async () => await ToggleServer());
            ToggleMenuVisibilityCommand = new RelayCommand(() => ToggleMenuVisibility());
            SettingMenuClickCommand = new RelayCommand<object>(SettingMenuClicked) ;
            PowerSwitchCommand = new RelayCommand<object>(PowerSwitchClicked);
            LogMessage = "";
        }

        private void PowerSwitchClicked(object obj)
        {
            var psVm = obj as PowerSwitchViewModel;
            string status = psVm.IsOn ? "on" : "off";
            MessageBox.Show("Power Switch [" + status + "] Clicked");
        }

        private void SettingMenuClicked(object o)
        {
            var strMenuButtonName = o as string;
            MessageBox.Show("Setting Menu " + strMenuButtonName + " Clicked");
        }

        [RelayCommand]
        private void ToggleMenuVisibility()
        {
            IsMenuVisible = !IsMenuVisible; // 메뉴 가시성 토글
            TriggerMenuAnimation();
        }

        private async Task ToggleServer()
        {
            if (_isRunning)
            {
                await StopServer();
            }
            else
            {
                await StartServer();
            }
        }

        private async Task StartServer()
        {
            var mqttFactory = new MqttFactory();
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            if (_mqttServer == null)
            {
                _mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions);
            }

            await _mqttServer.StartAsync();

            _mqttServer.ClientConnectedAsync += MqttServer_ClientConnectedAsync;

            LogMessage = GenerateLogMessage("Server Started!");

            _isRunning = true;
            ServerStatus = "Running";
        }

        private async Task StopServer()
        {
            if (_mqttServer != null)
            {
                _mqttServer.ClientConnectedAsync -= MqttServer_ClientConnectedAsync;
                await _mqttServer.StopAsync();
            }

            LogMessage = GenerateLogMessage("Server Stopped!");

            _isRunning = false;
            ServerStatus = "Stopped";
        }

        private Task MqttServer_ClientConnectedAsync(ClientConnectedEventArgs arg)
        {
            LogMessage = GenerateLogMessage(string.Format("{0} {1} {2}", arg.ClientId, arg.UserName, arg.Endpoint));
            return Task.CompletedTask;
        }

        private void TriggerMenuAnimation()
        {
            var window = Application.Current.MainWindow;
            var border = (System.Windows.Controls.Border)window.FindName("ExpandMenuBorder");
            var storyboardKey = IsMenuVisible ? "ShowMenuStoryboard" : "HideMenuStoryboard";
            var storyboard = (Storyboard)window.Resources[storyboardKey];

            storyboard.Completed += (s, e) =>
            {
                border.Visibility = IsMenuVisible ? Visibility.Visible : Visibility.Collapsed;
            };

            border.Visibility = Visibility.Visible; // 애니메이션 시작 전 가시성 설정
            storyboard.Begin(border);
        }

        private static string GenerateLogMessage(string message)
        {
            // 현재 날짜와 시간을 가져옵니다.
            DateTime now = DateTime.Now;

            // 날짜와 시간을 "yy-MM-dd HH:mm:ss" 형식으로 포맷합니다.
            string timestamp = now.ToString("yy-MM-dd HH:mm:ss");

            // 포맷된 날짜와 시간과 로그 메시지를 결합하여 반환합니다.
            string strLogFormat = $"[{timestamp}] {message}";

            Console.WriteLine(strLogFormat);
            return strLogFormat;
        }
    }
}

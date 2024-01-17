using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace ComboConnectionTest
{
    /// <summary>
    /// ChartViewerWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class ChartViewerWindow : Window
    {
        public ChartViewerVM VM;
        private List<float> curList;
        private Timer ChartUpdateTimer; // Work Thread Timer

        public ChartViewerWindow()
        {
            InitializeComponent();

            ViewerChart.DisableAnimations = true;
            ChartLineSeries.PointGeometrySize = 0.1; // Point Size -> 꼭지점 크기 거의 없게
            ChartLineSeries.LineSmoothness = 0.4; // Line Smoothing -> 부드럽게
            ChartLineSeries.StrokeThickness = 0.3; // Line Thickness -> 최대한 얇게

            // Mouse Over Effect Disabled
            ViewerChart.DataTooltip = null;
            ViewerChart.Hoverable = false;

            VM = new ChartViewerVM();
            this.DataContext = VM;

            ChartUpdateTimer = new Timer(ChartUpdateFunc);
            ChartUpdateTimer.Change(0, 100); // 100 ms 주기
            #region 해당 축에 대한 Separator 설정
            LiveCharts.Wpf.Separator sepX = new LiveCharts.Wpf.Separator();
            sepX.Step = ChartViewerVM.MAX_SPECTRUM_NUM / 10; // x축은 10칸 만
            sepX.StrokeThickness = 1.5;
            sepX.IsEnabled = true;
            XaXis.Separator = sepX;

            LiveCharts.Wpf.Separator sepY = new LiveCharts.Wpf.Separator();
            sepY.Step = 10;
            sepY.StrokeThickness = 1.5;
            sepY.IsEnabled = true;
            YaXis.Separator = sepY;
            #endregion

            // 기본 ref level 0 으로 설정
            this.SetYaxisMinMax(0);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            e.Cancel = true;
            this.Hide();
        }

        private void ChartUpdateFunc(object state)
        {
            if (curList == null)
            {
                return;
            }

            if (VM.ChartValues == null)
            {
                return;
            }

            try
            {
                float maxValue = 0;

                // for (int i = 0; i < ChartViewerVM.MAX_SPECTRUM_NUM; i++)
                for (int i = 0; i < curList.Count; i++)
                {
                    float value = curList[i];

                    // 메모리 릭 방지를 위한 변수 직접할당
                    if ((int)VM.ChartValues[i].Value != this.curList[i])
                    {
                        // 해당 부분에서 간헐적으로 오류 발생, 
                        VM.ChartValues[i].Value = value;
                    }

                    // Get MaxValue
                    if (maxValue < value)
                    {
                        maxValue = value;
                    }
                    VM.MaxValue = (int)maxValue;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void SetChartData(List<float> list)
        {
            this.curList = list;
        }

        public void SetYaxisMinMax(double ViewerRefLv)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                YaXis.MaxValue = ViewerRefLv;
                YaXis.MinValue = ViewerRefLv - 100;
            }));
        }

        public void Dispose()
        {
            if (ChartUpdateTimer != null)
            {
                ChartUpdateTimer.Dispose();
            }
        }
    }
}

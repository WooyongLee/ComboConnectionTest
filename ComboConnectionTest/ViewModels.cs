using LiveCharts;
using LiveCharts.Defaults;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace ComboConnectionTest
{
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        // 각 Property 이름으로 지정해 놓고 UI 쪽으로 변경에 대한 이벤트 구현
        protected void NotifyPropertyChanged(string propertyName = "")
        {
            this.VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            // Verify that the property name matches a real,  
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(this)[propertyName] == null)
            {
                string msg = "Invalid property name: " + propertyName;

                throw new Exception(msg);
            }
        }
    }

    public class ChartViewerVM : ViewModelBase
    {
        public static readonly int MAX_SPECTRUM_NUM = 1000;

        // 스펙트럼 개수
        private int maxNum;

        private int maxValue;

        private ChartValues<ObservableValue> chartValues;

        public ChartValues<ObservableValue> ChartValues
        {
            get { return chartValues; }
            set
            {
                chartValues = value;
                NotifyPropertyChanged("ChartValues");
            }
        }

        public int MaxValue
        {
            get { return maxValue; }
            set
            {
                maxValue = value;
                NotifyPropertyChanged("MaxValue");
            }
        }

        public int MaxNum
        {
            get { return maxNum; }
            set
            {
                if (ChartValues != null)
                {
                    maxNum = ChartValues.Count;
                }

                else
                {
                    maxNum = MAX_SPECTRUM_NUM;
                }

                NotifyPropertyChanged("MaxNum");
            }
        }

        public ChartViewerVM()
        {
            this.ChartValues = new ChartValues<ObservableValue>();

            // 1001개의 스펙트럼 데이터 초기화
            for (int i = 0; i < MAX_SPECTRUM_NUM; i++)
            {
                this.ChartValues.Add(new ObservableValue(0));
            }
        }
    }
}

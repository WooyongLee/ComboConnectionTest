using CommunityToolkit.Mvvm.ComponentModel;

namespace ComboBrokerTest.ViewModel
{
    public class PowerSwitchViewModel : ObservableRecipient
    {

        #region Properties
        private bool _isOn;

        public bool IsOn
        {
            get => _isOn;
            set => SetProperty(ref _isOn, value);
        }
        #endregion

        public PowerSwitchViewModel()
        {
            IsOn = false;
        }
    }
}

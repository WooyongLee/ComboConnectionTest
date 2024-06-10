using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;

namespace ComboBrokerTest.ViewModelConverter
{
    public class InverseBooleanToVisibilityConverter : IValueConverter
    {
        //
        // 요약:
        //     부울 값으로 변환 된 System.Windows.Visibility 열거형 값입니다.
        //
        // 매개 변수:
        //   value:
        //     변환할 부울 값입니다. 이 값은 표준 부울 값 또는 null 허용 부울 값을 수 있습니다.
        //
        //   targetType:
        //     이 매개 변수는 사용되지 않습니다.
        //
        //   parameter:
        //     이 매개 변수는 사용되지 않습니다.
        //
        //   culture:
        //     이 매개 변수는 사용되지 않습니다.
        //
        // 반환 값:
        //     System.Windows.Visibility.Visible 경우 value 은 true고, 그렇지 않으면 System.Windows.Visibility.Collapsed합니다.
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool flag = false;
            if (value is bool)
            {
                flag = !(bool)value;
            }

            return (!flag) ? Visibility.Collapsed : Visibility.Visible;
        }

        //
        // 요약:
        //     변환 된 System.Windows.Visibility 열거형 값을 부울 값입니다.
        //
        // 매개 변수:
        //   value:
        //     System.Windows.Visibility 열거형 값입니다.
        //
        //   targetType:
        //     이 매개 변수는 사용되지 않습니다.
        //
        //   parameter:
        //     이 매개 변수는 사용되지 않습니다.
        //
        //   culture:
        //     이 매개 변수는 사용되지 않습니다.
        //
        // 반환 값:
        //     true 경우 value 은 System.Windows.Visibility.Visible고, 그렇지 않으면 false합니다.
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                return (Visibility)value != Visibility.Visible;
            }

            return false;
        }
    }

    public class OnViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Hidden;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }

    public class OffViewConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
            {
                return Visibility.Hidden;
            }
            else
            {
                return Visibility.Visible;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ComboBrokerTest.UIComponent
{
    /// <summary>
    /// TextBlockWithBorder.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class TextBlockWithBorder : UserControl
    {
        public TextBlockWithBorder()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TextBlockWithBorder), new PropertyMetadata(null));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        public static readonly DependencyProperty CommandParameterProperty =
           DependencyProperty.Register("CommandParameter", typeof(object), typeof(TextBlockWithBorder), new PropertyMetadata(null));

        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Command != null && Command.CanExecute(CommandParameter))
            {
                Command.Execute(CommandParameter);
            }
        }

        public string Text
        {
            get { return textBlock.Text; }
            set { textBlock.Text = value; }
        }
    }
}

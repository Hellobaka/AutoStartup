using System.Windows;

namespace AutoStartup.Windows
{
    /// <summary>
    /// PythonProgram.xaml 的交互逻辑
    /// </summary>
    public partial class PowerShell : Window
    {
        public PowerShell()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(PowerShellCommand.Text))
            {
                MainWindow.ShowError("脚本内容不得为空");
                return;
            }
            DialogResult = true;
            Tag = new Services.Service("PowerShell command",
                                       "powershell.exe",
                                       true,
                                       $"-Command {PowerShellCommand.Text}");
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
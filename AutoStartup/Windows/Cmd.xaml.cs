using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace AutoStartup.Windows
{
    /// <summary>
    /// PythonProgram.xaml 的交互逻辑
    /// </summary>
    public partial class CMD : Window
    {
        public CMD()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CMDCommand.Text))
            {
                MainWindow.ShowError("脚本内容不得为空");
                return;
            }

            DialogResult = true;
            Tag = new Services.Service("cmd command",
                                       "cmd.exe",
                                       true,
                                       $"/c {CMDCommand.Text}");
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

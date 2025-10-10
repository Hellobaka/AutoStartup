using System.IO;
using System.Windows;

namespace AutoStartup.Windows
{
    /// <summary>
    /// PythonProgram.xaml 的交互逻辑
    /// </summary>
    public partial class PythonProgram : Window
    {
        public PythonProgram()
        {
            InitializeComponent();
        }

        private void BrowsePythonProgram_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Python 脚本|*.py|所有文件|*.*",
                Title = "选择需要运行的 Python 脚本文件"
            };
            if (dialog.ShowDialog() ?? false)
            {
                PythonProgramPath.Text = dialog.FileName;
            }
        }

        private void BrowsePythonInterpreter_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog()
            {
                Multiselect = false,
                Filter = "Python 解释器|*.exe|所有文件|*.*",
                Title = "选择一个 Python 解释器"
            };
            if (dialog.ShowDialog() ?? false)
            {
                PythonInterpreters.Text = dialog.FileName;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var paths = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)?.Split(';')
                .Concat(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)?.Split(';'));

            foreach (var path in paths ?? [])
            {
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                var pythonPath = Path.Combine(path, "python.exe");
                if (File.Exists(pythonPath) && !PythonInterpreters.Items.Contains(pythonPath))
                {
                    PythonInterpreters.Items.Add(pythonPath);
                }
            }
            if (PythonInterpreters.Items.Count > 0)
            {
                PythonInterpreters.SelectedIndex = 0;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!File.Exists(PythonInterpreters.Text))
            {
                MainWindow.ShowError($"选择的 Python 解释器文件不存在: {PythonInterpreters.Text}");
                return;
            }
            if (!File.Exists(PythonProgramPath.Text))
            {
                MainWindow.ShowError($"选择的 Python 脚本路径不存在: {PythonProgramPath.Text}");
                return;
            }
            DialogResult = true;
            Tag = new Services.Service(Path.GetFileNameWithoutExtension(PythonProgramPath.Text), PythonInterpreters.Text, true, $"\"{PythonProgramPath.Text}\"", Path.GetDirectoryName(PythonProgramPath.Text));
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
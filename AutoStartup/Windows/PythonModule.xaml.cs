using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace AutoStartup.Windows
{
    /// <summary>
    /// PythonProgram.xaml 的交互逻辑
    /// </summary>
    public partial class PythonModule : Window
    {
        public PythonModule()
        {
            InitializeComponent();
        }

        private void BrowsePythonProgram_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog()
            {
                Multiselect = false,
                Title = "选择运行脚本的基础路径"
            };
            if (dialog.ShowDialog() ?? false)
            {
                PythonProgramPath.Text = dialog.FolderName;
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
            if (!Directory.Exists(PythonProgramPath.Text))
            {
                MainWindow.ShowError($"选择的 Python 模块目录不存在: {PythonProgramPath.Text}");
                return;
            }
            DialogResult = true;
            Tag = new Services.Service(PythonModulePath.Text,
                                       PythonInterpreters.Text,
                                       true,
                                       $"-m {PythonModulePath.Text}",
                                       PythonProgramPath.Text);
            Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}

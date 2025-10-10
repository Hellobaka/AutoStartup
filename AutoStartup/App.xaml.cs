using AutoStartup.Services;
using Microsoft.Win32;
using System.Security.Principal;
using System.Windows;

namespace AutoStartup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        [STAThread]
        public static async Task Main(string[] args)
        {
            Shared.AppMutex = new Mutex(true, Shared.AppMutexName, out bool success);
            if (!success)
            {
                IPCNotice.SendActivationSignal();
                return;
            }

            try
            {
                Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
                Shared.IsElevated = IsElevated();
                Shared.Maintenance = args.Length == 0;

                ServiceManager serviceManager = new();

                TaskbarHelper.OnTaskbarDoubleClicked += TaskbarHelper_OnTaskbarDoubleClicked;
                IPCNotice.OnActivateRequested += IPCNotice_OnActivateRequested;
                TaskbarHelper.BuildTaskBar();
                IPCNotice.StartPipeServer();
                bool loadService = serviceManager.LoadFromFile();
                if (!CheckHasStartupRegistry())
                {
                    AddStartupProgram("AutoStartup", Environment.ProcessPath + " -o");
                }
                if (Shared.Maintenance)
                {
                    RunApp();
                }
                else
                {
                    if (loadService)
                    {
                        await serviceManager.StartAllAsync();
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("加载服务列表失败，请手动启动以修复问题。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                        return;
                    }
                }

                Shared.QuitSignal.WaitOne();
            }
            catch { }
            finally
            {
                Shared.AppMutex.ReleaseMutex();
                Shared.AppMutex.Dispose();
            }
        }

        private static void IPCNotice_OnActivateRequested()
        {
            if (Shared.WPFInstance == null)
            {
                RunApp();
                return;
            }
            Shared.WPFInstance.Dispatcher.BeginInvoke(new Action(() =>
            {
                var window = Shared.WPFInstance.MainWindow;
                if (window == null)
                {
                    return;
                }
                window.Show();
            }));
        }

        private static void TaskbarHelper_OnTaskbarDoubleClicked()
        {
            if (Shared.WPFInstance == null)
            {
                RunApp();
                return;
            }
            Shared.WPFInstance.Dispatcher.BeginInvoke(new Action(() =>
            {
                var window = Shared.WPFInstance.MainWindow;
                if (window == null)
                {
                    return;
                }
                window.Show();
            }));
        }

        private static void RunApp()
        {
            if (Shared.IsElevated)
            {
                System.Windows.MessageBox.Show("当前应用由管理员启动，在当前权限下，无法支持拖拽添加应用。", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            Thread staThread = new(() =>
            {
                try
                {
                    var app = new App
                    {
                        StartupUri = new("pack://application:,,,/MainWindow.xaml"),
                    };
                    app.InitializeComponent();

                    Shared.WPFInstance = app;
                    Shared.WPFInstance.Run();
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"WPF 事件循环过程发生异常：{ex}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
        }

        private static bool CheckHasStartupRegistry() => Registry.CurrentUser?.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true)?.GetValueNames().Any(x => x == "AutoStartup") ?? false;

        private static void AddStartupProgram(string name, string path)
        {
            RegistryKey? rk = Registry.CurrentUser?.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk == null)
            {
                AutoStartup.MainWindow.ShowError("自启动项添加失败，请检查运行权限。");
                return;
            }
            rk.SetValue(name, path);
        }

        private static bool IsElevated()
        {
            try
            {
                using WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }
    }
}
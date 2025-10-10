using AutoStartup.Services;
using Microsoft.Win32;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Threading.Tasks;
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

                TaskbarHelper.OnTaskbarDoubleClicked += TaskbarHelper_OnTaskbarDoubleClicked;
                IPCNotice.OnActivateRequested += IPCNotice_OnActivateRequested;
                Shared.Maintenance = args.Length == 0;
                TaskbarHelper.BuildTaskBar();
                IPCNotice.StartPipeServer();
                ServiceManager serviceManager = new();
                bool loadService = serviceManager.LoadFromFile();
                if (Shared.Maintenance)
                {
                    RunApp();
                }
                else
                {
                    if (!CheckHasStartupRegistry())
                    {
                        AddStartupProgram("AutoStartup", System.Reflection.Assembly.GetExecutingAssembly().Location + " -o");
                    }
                    if (loadService)
                    {
                        await serviceManager.StartAllAsync();
                    }
                    else
                    {
                        AutoStartup.MainWindow.ShowError("加载服务列表失败，请手动启动以修复问题。");
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
            Thread staThread = new(() =>
            {
                var app = new App
                {
                    StartupUri = new("pack://application:,,,/MainWindow.xaml"),
                };
                app.InitializeComponent();

                Shared.WPFInstance = app;
                Shared.WPFInstance.Run();
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
    }
}

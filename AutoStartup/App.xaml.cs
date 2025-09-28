using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace AutoStartup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        [STAThread]
        public static void Main(string[] args)
        {
            Shared.AppMutex = new Mutex(true, Shared.AppMutexName, out bool success);
            if (!success)
            {
                IPCNotice.SendActivationSignal();
                return;
            }

            try
            {
                TaskbarHelper.OnTaskbarDoubleClicked += TaskbarHelper_OnTaskbarDoubleClicked;
                IPCNotice.OnActivateRequested += IPCNotice_OnActivateRequested;
                Shared.Maintenance = args.Length != 0;
                TaskbarHelper.BuildTaskBar();
                IPCNotice.StartPipeServer();
                if (!Shared.Maintenance)
                {
                    RunApp();
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
                else if (window.IsVisible)
                {
                    window.Hide();
                }
                else
                {
                    window.Show();
                }
            }));
        }

        private static void RunApp()
        {
            Thread staThread = new(() =>
            {
                var app = new App
                {
                    StartupUri = new("pack://application:,,,/MainWindow.xaml"),
                    ShutdownMode = ShutdownMode.OnExplicitShutdown
                };
                app.InitializeComponent();

                Shared.WPFInstance = app;
                Shared.WPFInstance.Run();
            });
            staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
        }
    }
}

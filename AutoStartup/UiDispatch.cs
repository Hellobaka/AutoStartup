using NLog;
using System.Windows.Threading;

namespace AutoStartup
{
    internal static class UiDispatch
    {
        private static readonly Logger Logger = LogManager.GetLogger("Diagnostics");

        // Do not wait for the UI from process output callbacks. Shutdown can abort
        // queued operations between the check below and BeginInvoke.
        public static void Post(Dispatcher dispatcher, Action action, string context)
        {
            if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            {
                return;
            }

            try
            {
                dispatcher.BeginInvoke(new Action(() =>
                {
                    if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
                    {
                        return;
                    }

                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "界面更新失败：{0}", context);
                    }
                }));
            }
            catch (OperationCanceledException)
            {
                // The dispatcher was shut down while posting this update.
            }
            catch (InvalidOperationException ex) when (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
            {
                Logger.Debug(ex, "界面已关闭，跳过更新：{0}", context);
            }
        }
    }
}

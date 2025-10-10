using AutoStartup.Services;
using System.IO;
using System.Threading.Tasks;

namespace AutoStartup
{
    public static class TaskbarHelper
    {
        public static event Action OnTaskbarDoubleClicked;

        private static Thread UIThread { get; set; }

        private static NotifyIcon NotifyIcon { get; set; }

        private static ToolStripMenuItem TotalServiceDisplay { get; set; }

        private static ToolStripMenuItem RunningServiceDisplay { get; set; }

        private static ToolStripMenuItem TaskBarMenuParent { get; set; }

        public static void BuildTaskBar()
        {
            if (UIThread == null)
            {
                UIThread = new Thread(() =>
                {
                    try
                    {
                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);
                        Application.SetDefaultFont(new("Segoe UI", 9));
                        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

                        NotifyIcon = new NotifyIcon();
                        NotifyIcon.Icon = new Icon(new MemoryStream(Convert.FromBase64String(Shared.IconBase64)));
                        var menu = new ContextMenuStrip();
                        NotifyIcon.ContextMenuStrip = menu;

                        TotalServiceDisplay = new ToolStripMenuItem { Text = $"共 {0} 个服务" };
                        RunningServiceDisplay = new ToolStripMenuItem { Text = $"正在运行 {0} 个服务" };

                        menu.Items.Add(TotalServiceDisplay);
                        menu.Items.Add(RunningServiceDisplay);
                        menu.Items.Add("-");
                        menu.Items.Add("显示 UI", null, ShowUI_Click);
                        menu.Items.Add("-");
                        TaskBarMenuParent = new ToolStripMenuItem() { Text = "服务" };
                        menu.Items.Add(TaskBarMenuParent);
                        menu.Items.Add("-");

                        menu.Items.Add(new ToolStripMenuItem { Text = $"框架版本: {"1.0.0"}" });
                        menu.Items.Add("退出", null, ExitItem_Click);

                        NotifyIcon.Visible = true;
                        NotifyIcon.MouseDown += NotifyIcon_MouseDown;
                        NotifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                        RebuildTaskBarMenu();
                        Application.Run();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"托盘事件循环过程发生异常：{ex}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                });
                UIThread.SetApartmentState(ApartmentState.STA);
                UIThread.Start();
            }
        }

        private static void NotifyIcon_MouseDown(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                RebuildTaskBarMenu();
            }
        }

        private static void ShowUI_Click(object? sender, EventArgs e)
        {
            OnTaskbarDoubleClicked?.Invoke();
        }

        private static void ExitItem_Click(object? sender, EventArgs e)
        {
            if (MessageBox.Show("确定要退出框架吗？", "嗯？", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }

        private static void NotifyIcon_DoubleClick(object? sender, EventArgs e)
        {
            Task.Run(() => OnTaskbarDoubleClicked?.Invoke());
        }

        private static async void StopAllItem_Click(object? sender, EventArgs e)
        {
            await ServiceManager.Instance.StopAllAsync();
        }

        private static async void StartAllItem_Click(object? sender, EventArgs e)
        {
            await ServiceManager.Instance.StartAllAsync();
        }

        public static void RebuildTaskBarMenu()
        {
            int total = ServiceManager.Instance.TotalCount, running = ServiceManager.Instance.RunningCount;
            NotifyIcon.Text = $"共 {total} 个服务; 正在运行 {running} 个服务";
            TotalServiceDisplay.Text = $"共 {total} 个服务";
            RunningServiceDisplay.Text = $"正在运行 {running} 个服务";
            TaskBarMenuParent.DropDownItems.Clear();

            foreach(var item in ServiceManager.Instance.ListServices())
            {
                ToolStripMenuItem subService = new(item.Name);
                ToolStripMenuItem startService = new("启动");
                startService.Enabled = item.Status != ServiceStatus.Running;
                startService.Tag = item;
                startService.Click += StartService_Click;
                ToolStripMenuItem stopService = new("终止");
                stopService.Enabled = item.Status != ServiceStatus.Stopped;
                stopService.Tag = item;
                stopService.Click += StopService_Click;
                ToolStripMenuItem restartService = new("重启");
                restartService.Enabled = item.Status == ServiceStatus.Running;
                restartService.Tag = item;
                restartService.Click += RestartService_Click;

                subService.DropDownItems.Add(startService);
                subService.DropDownItems.Add(stopService);
                subService.DropDownItems.Add("-");
                subService.DropDownItems.Add(restartService);

                TaskBarMenuParent.DropDownItems.Add(subService);
            }
            TaskBarMenuParent.DropDownItems.Add("-");
            TaskBarMenuParent.DropDownItems.Add("启用所有", null, StartAllItem_Click);
            TaskBarMenuParent.DropDownItems.Add("终止所有", null, StopAllItem_Click);
        }

        private static async void RestartService_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Service service)
            {
                await service.RestartAsync();
            }
        }

        private static async void StopService_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Service service)
            {
                await service.StopAsync();
            }
        }

        private static async void StartService_Click(object? sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem item && item.Tag is Service service)
            {
                await service.StartAsync();
            }
        }

        public static void Invoke(Action action)
        {
            if (NotifyIcon.ContextMenuStrip != null && NotifyIcon.ContextMenuStrip.InvokeRequired)
            {
                NotifyIcon.ContextMenuStrip.BeginInvoke(action);
            }
            else
            {
                action.Invoke();
            }
        }

        public static void ShowTrayInfo(string title, string msg, ToolTipIcon info)
        {
            NotifyIcon.ShowBalloonTip(3000, title, msg, info);
        }
    }
}

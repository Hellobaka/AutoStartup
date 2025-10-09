using System.IO;

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

                    NotifyIcon.Text = $"共 {0} 个服务; 正在运行 {0} 个服务";
                    NotifyIcon.Visible = true;
                    NotifyIcon.DoubleClick += NotifyIcon_DoubleClick;
                    RebuildTaskBarMenu();
                    Application.Run();
                });
                UIThread.SetApartmentState(ApartmentState.STA);
                UIThread.Start();
            }
        }

        private static void ShowUI_Click(object? sender, EventArgs e)
        {
            throw new NotImplementedException();
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

        private static void StopAllItem_Click(object? sender, EventArgs e)
        {

        }

        private static void StartAllItem_Click(object? sender, EventArgs e)
        {

        }

        public static void RebuildTaskBarMenu()
        {
            NotifyIcon.Text = $"共 {0} 个服务; 正在运行 {0} 个服务";
            TotalServiceDisplay.Text = $"共 {0} 个服务";
            RunningServiceDisplay.Text = $"正在运行 {0} 个服务";
            TaskBarMenuParent.DropDownItems.Clear();

            TaskBarMenuParent.DropDownItems.Add("-");
            TaskBarMenuParent.DropDownItems.Add("启用所有", null, StartAllItem_Click);
            TaskBarMenuParent.DropDownItems.Add("终止所有", null, StopAllItem_Click);
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

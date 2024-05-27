using Microsoft.Win32;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Diagnostics;

namespace AutoStartup
{
    public partial class MainForm : Form
    {
        public bool Maintance { get; set; }
        public bool HasError { get; set; }
        public string CurrentStatus
        {
            get => StatusLabel.Text;
            set
            {
                if (InvokeRequired)
                {
                    Invoke(new MethodInvoker(() => StatusLabel.Text = value));
                }
                else
                {
                    StatusLabel.Text = value;
                }
            }
        }
        public Config ConfigItem { get; set; }

        public class StartUpItem
        {
            public string Name { get; set; }
            public string Path { get; set; }
            public string Arg { get; set; }
            public bool Enabled { get; set; }
            public bool HideWindow { get; set; }
        }

        public MainForm(bool v)
        {
            Maintance = v;
            InitializeComponent();
        }

        public static void AddStartupProgram(string name, string path, bool add = true)
        {
            RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            if (rk == null)
            {
                MessageBox.Show("添加失败", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (add)
                rk.SetValue(name, path);
            else
                rk.DeleteValue(name, false);
        }

        public bool IsStartup => Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true).GetValueNames().Any(x => x == "AutoStartup");
        public bool Loaded { get; set; }
        public bool StopFlag { get; set; }
        public bool TestFlag { get; set; }
        public string ConfigPath { get; set; }

        private void StartupList_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void StartupList_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                Debug.WriteLine("a");
            }
        }

        private void AddBtn_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(ProgramName.Text))
            {
                MessageBox.Show("名称无效", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(ProgramPath.Text))
            {
                MessageBox.Show("文件路径不存在", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确认要添加吗？", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            if (ConfigItem.StartUpItems.Any(x => x.Path == ProgramPath.Text))
            {
                MessageBox.Show("存在路径相同的文件", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            ConfigItem.StartUpItems.Add(new StartUpItem
            {
                Name = ProgramName.Text,
                Path = ProgramPath.Text,
                Arg = ProgramArg.Text,
                Enabled = ProgramEnabled.Checked,
                HideWindow = HideWindowSelector.Checked
            });
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(ConfigItem, Formatting.Indented));
            RefreshList();
        }

        private void RefreshList()
        {
            StartupList.Items.Clear();
            for (int i = 0; i < StartupList.Columns.Count; i++)
            {
                StartupList.Columns[i].Width = -1;
            }
            foreach (var item in ConfigItem.StartUpItems)
            {
                ListViewItem viewItem = new();
                viewItem.SubItems[0].Text = item.Enabled ? "是" : "否";
                viewItem.SubItems.Add(item.HideWindow ? "是" : "否");
                viewItem.SubItems.Add(item.Name);
                viewItem.SubItems.Add(item.Path);
                viewItem.SubItems.Add(item.Arg);
                StartupList.Items.Add(viewItem);
            }
            StartupList.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void EditBtn_Click(object sender, EventArgs e)
        {
            if (StartupList.SelectedIndices.Count == 0)
            {
                return;
            }
            if (string.IsNullOrEmpty(ProgramName.Text))
            {
                MessageBox.Show("名称无效", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!File.Exists(ProgramPath.Text))
            {
                MessageBox.Show("文件路径不存在", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确认要更改吗？", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            var item = ConfigItem.StartUpItems[StartupList.SelectedIndices[0]];
            item.Name = ProgramName.Text;
            item.Path = ProgramPath.Text;
            item.Arg = ProgramArg.Text;
            item.Enabled = ProgramEnabled.Checked;
            item.HideWindow = HideWindowSelector.Checked;
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(ConfigItem, Formatting.Indented));
            RefreshList();
        }

        private void DeleteBtn_Click(object sender, EventArgs e)
        {
            if (StartupList.SelectedIndices.Count == 0)
            {
                MessageBox.Show("请选择一项", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (MessageBox.Show("确认要删除吗？", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            {
                return;
            }
            ConfigItem.StartUpItems.RemoveAt(StartupList.SelectedIndices[0]);
            File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(ConfigItem, Formatting.Indented));
            RefreshList();
        }

        private void StartUpSelector_CheckedChanged(object sender, EventArgs e)
        {
            if (!Loaded) return;
            if (StartUpSelector.Checked)
            {
                AddStartupProgram("AutoStartup", Application.ExecutablePath + " -o", true);
            }
            else
            {
                AddStartupProgram("AutoStartup", Application.ExecutablePath + " -o", false);
            }
        }

        private void MainForm_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void MainForm_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                string path = ((string[])e.Data.GetData(DataFormats.FileDrop)).First();
                if (File.Exists(path))
                {
                    var info = new FileInfo(path);
                    ProgramName.Text = info.Name.Replace(info.Extension, "");
                    ProgramPath.Text = info.FullName;
                    ProgramEnabled.Checked = true;
                }
            }
        }

        private void MainForm_DragLeave(object sender, EventArgs e)
        {
            ProgramName.Text = "";
            ProgramPath.Text = "";
            ProgramArg.Text = "";
            ProgramEnabled.Checked = false;
            HideWindowSelector.Checked = false;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            ConfigPath = AppDomain.CurrentDomain.BaseDirectory + "Config.json";
            if (File.Exists(ConfigPath))
            {
                try
                {
                    ConfigItem = JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
                }
                catch { }
            }
            ConfigItem ??= new();
            StartupList.MouseDoubleClick += StartupList_MouseDoubleClick;
            RefreshList();
            StartUpSelector.Checked = IsStartup;
            Loaded = true;
            if (!Maintance)
            {
                new Thread(() => StartProcess(ConfigItem.StartWaitTime)).Start();
            }
        }

        private void StartupList_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            ListViewHitTestInfo info = StartupList.HitTest(e.X, e.Y);
            ListViewItem item = info.Item;

            if (item != null)
            {
                ProgramName.Text = item.SubItems[2].Text;
                ProgramPath.Text = item.SubItems[3].Text;
                ProgramArg.Text = item.SubItems[4].Text;
                ProgramEnabled.Checked = item.SubItems[0].Text == "是";
                HideWindowSelector.Checked = item.SubItems[1].Text == "是";
            }
        }

        private void StartProcess(int waitSeconds)
        {
            HasError = false;
            StopFlag = false;
            Invoke(new MethodInvoker(() => StopBtn.Enabled = true));
            waitSeconds *= 10;
            for (int i = 0; i < waitSeconds; i++)
            {
                CurrentStatus = $"将在{((double)waitSeconds - i) / 10:f2}s后开始启动程序执行...";
                if (StopFlag)
                {
                    CurrentStatus = $"维护模式...";
                    StopFlag = false;
                    Invoke(new MethodInvoker(() => StopBtn.Enabled = false));
                    return;
                }
                Thread.Sleep(100);
            }

            foreach (var item in ConfigItem.StartUpItems.Where(x => x.Enabled))
            {
                if (StopFlag)
                {
                    CurrentStatus = $"维护模式...";
                    StopFlag = false;
                    Invoke(new MethodInvoker(() => StopBtn.Enabled = false));
                    return;
                }
                CurrentStatus = $"启动程序 {item.Name} 中...";
                try
                {
                    Process.Start(new ProcessStartInfo
                    { 
                        FileName = item.Path, 
                        WorkingDirectory = new FileInfo(item.Path).DirectoryName, 
                        Arguments = item.Arg, 
                        UseShellExecute = !item.HideWindow,
                        CreateNoWindow = item.HideWindow
                    });
                }
                catch (Exception e)
                {
                    CurrentStatus = $"程序 {item.Name} 启动发生错误.";
                    HasError = true;
                    new Thread(() =>
                    {
                        MessageBox.Show($"程序 {item.Name} 启动失败，错误原因: {e.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }).Start();
                }
            }

            if (!HasError)
            {
                if (TestFlag)
                {
                    CurrentStatus = $"测试通过，未发生错误...";
                    Invoke(new MethodInvoker(() => StopBtn.Enabled = false));
                    return;
                }
                for (int i = 0; i < waitSeconds; i++)
                {
                    CurrentStatus = $"启动完成...将在{((double)waitSeconds - i) / 10:f2}s后退出程序...";
                    if (StopFlag)
                    {
                        CurrentStatus = $"维护模式...";
                        StopFlag = false;
                        Invoke(new MethodInvoker(() => StopBtn.Enabled = false));
                        return;
                    }
                    Thread.Sleep(100);
                }
                Environment.Exit(0);
            }
            else
            {
                CurrentStatus = $"启动完成，但是部分程序启动失败，查看错误窗口获取错误信息";
            }
        }

        private void OpenFileDialog_Click(object sender, EventArgs e)
        {
            OpenFileDialog.ShowDialog();
            string path = OpenFileDialog.FileName;
            if (File.Exists(path))
            {
                var info = new FileInfo(path);
                ProgramName.Text = info.Name.Replace(info.Extension, "");
                ProgramPath.Text = info.FullName;
                ProgramEnabled.Checked = true;
            }
        }

        private void Program_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddBtn.PerformClick();
            }
        }

        private void StopBtn_Click(object sender, EventArgs e)
        {
            StopFlag = true;
        }

        private void TestBtn_Click(object sender, EventArgs e)
        {
            TestFlag = true;
            new Thread(() => StartProcess(0)).Start();
        }

        private void KillSelectButton_Click(object sender, EventArgs e)
        {
            if (StartupList.SelectedIndices.Count == 0)
            {
                MessageBox.Show("请选择一项", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (ConfigItem.StartUpItems.Count <= StartupList.SelectedIndices[0])
            {
                MessageBox.Show("索引超出数组界限", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var item = ConfigItem.StartUpItems[StartupList.SelectedIndices[0]];
            Process? process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(item.Path)).FirstOrDefault();
            if (process != null)
            {
                if (MessageBox.Show($"确定要终止 {item.Name} 进程吗？", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    try
                    {
                        process.Kill(true);
                    }
                    catch (Exception exc)
                    {
                        MessageBox.Show($"无法终止 {item.Name} 的进程，错误: {exc.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show($"无法找到 {item.Name} 的进程", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KillAllButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show($"确定要终止所有进程吗？", "Q", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                int count = 0;
                foreach (var item in ConfigItem.StartUpItems)
                {
                    Process? process = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(item.Path)).FirstOrDefault();
                    if(process != null)
                    {
                        try
                        {
                            process.Kill(true);
                            count++;
                        }
                        catch (Exception exc)
                        {
                            MessageBox.Show($"无法终止 {item.Name} 的进程，错误: {exc.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                MessageBox.Show($"终止了 {count} 个进程", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}

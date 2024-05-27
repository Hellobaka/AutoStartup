namespace AutoStartup
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            StartUpSelector = new CheckBox();
            groupBox1 = new GroupBox();
            HideWindowSelector = new CheckBox();
            TestBtn = new Button();
            OpenFileDialogBtn = new Button();
            ProgramEnabled = new CheckBox();
            DeleteBtn = new Button();
            EditBtn = new Button();
            ProgramArg = new TextBox();
            AddBtn = new Button();
            label3 = new Label();
            ProgramPath = new TextBox();
            label2 = new Label();
            ProgramName = new TextBox();
            label1 = new Label();
            StartupList = new ListView();
            columnHeader1 = new ColumnHeader();
            columnHeader5 = new ColumnHeader();
            columnHeader2 = new ColumnHeader();
            columnHeader3 = new ColumnHeader();
            columnHeader4 = new ColumnHeader();
            OpenFileDialog = new OpenFileDialog();
            groupBox2 = new GroupBox();
            KillSelectButton = new Button();
            KillAllButton = new Button();
            StopBtn = new Button();
            StatusLabel = new Label();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            SuspendLayout();
            // 
            // StartUpSelector
            // 
            StartUpSelector.AutoSize = true;
            StartUpSelector.Location = new Point(12, 12);
            StartUpSelector.Name = "StartUpSelector";
            StartUpSelector.Size = new Size(207, 21);
            StartUpSelector.TabIndex = 0;
            StartUpSelector.Text = "开机自启 (修改请给予管理员权限)";
            StartUpSelector.UseVisualStyleBackColor = true;
            StartUpSelector.CheckedChanged += StartUpSelector_CheckedChanged;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(HideWindowSelector);
            groupBox1.Controls.Add(TestBtn);
            groupBox1.Controls.Add(OpenFileDialogBtn);
            groupBox1.Controls.Add(ProgramEnabled);
            groupBox1.Controls.Add(DeleteBtn);
            groupBox1.Controls.Add(EditBtn);
            groupBox1.Controls.Add(ProgramArg);
            groupBox1.Controls.Add(AddBtn);
            groupBox1.Controls.Add(label3);
            groupBox1.Controls.Add(ProgramPath);
            groupBox1.Controls.Add(label2);
            groupBox1.Controls.Add(ProgramName);
            groupBox1.Controls.Add(label1);
            groupBox1.Location = new Point(12, 39);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(471, 168);
            groupBox1.TabIndex = 1;
            groupBox1.TabStop = false;
            groupBox1.Text = "添加参数 (可拖动文件)";
            // 
            // HideWindowSelector
            // 
            HideWindowSelector.AutoSize = true;
            HideWindowSelector.Location = new Point(85, 109);
            HideWindowSelector.Name = "HideWindowSelector";
            HideWindowSelector.Size = new Size(159, 21);
            HideWindowSelector.TabIndex = 8;
            HideWindowSelector.Text = "隐藏窗口 (仅控制台应用)";
            HideWindowSelector.UseVisualStyleBackColor = true;
            // 
            // TestBtn
            // 
            TestBtn.Location = new Point(394, 139);
            TestBtn.Name = "TestBtn";
            TestBtn.Size = new Size(75, 23);
            TestBtn.TabIndex = 7;
            TestBtn.Text = "测试";
            TestBtn.UseVisualStyleBackColor = true;
            TestBtn.Click += TestBtn_Click;
            // 
            // OpenFileDialogBtn
            // 
            OpenFileDialogBtn.Location = new Point(390, 51);
            OpenFileDialogBtn.Name = "OpenFileDialogBtn";
            OpenFileDialogBtn.Size = new Size(75, 23);
            OpenFileDialogBtn.TabIndex = 7;
            OpenFileDialogBtn.Text = "浏览";
            OpenFileDialogBtn.UseVisualStyleBackColor = true;
            OpenFileDialogBtn.Click += OpenFileDialog_Click;
            // 
            // ProgramEnabled
            // 
            ProgramEnabled.AutoSize = true;
            ProgramEnabled.Location = new Point(6, 109);
            ProgramEnabled.Name = "ProgramEnabled";
            ProgramEnabled.Size = new Size(51, 21);
            ProgramEnabled.TabIndex = 6;
            ProgramEnabled.Text = "启用";
            ProgramEnabled.UseVisualStyleBackColor = true;
            // 
            // DeleteBtn
            // 
            DeleteBtn.Location = new Point(166, 139);
            DeleteBtn.Name = "DeleteBtn";
            DeleteBtn.Size = new Size(75, 23);
            DeleteBtn.TabIndex = 5;
            DeleteBtn.Text = "删除";
            DeleteBtn.UseVisualStyleBackColor = true;
            DeleteBtn.Click += DeleteBtn_Click;
            // 
            // EditBtn
            // 
            EditBtn.Location = new Point(85, 139);
            EditBtn.Name = "EditBtn";
            EditBtn.Size = new Size(75, 23);
            EditBtn.TabIndex = 4;
            EditBtn.Text = "编辑";
            EditBtn.UseVisualStyleBackColor = true;
            EditBtn.Click += EditBtn_Click;
            // 
            // ProgramArg
            // 
            ProgramArg.Location = new Point(47, 80);
            ProgramArg.Name = "ProgramArg";
            ProgramArg.Size = new Size(418, 23);
            ProgramArg.TabIndex = 5;
            ProgramArg.KeyDown += Program_KeyDown;
            // 
            // AddBtn
            // 
            AddBtn.Location = new Point(4, 139);
            AddBtn.Name = "AddBtn";
            AddBtn.Size = new Size(75, 23);
            AddBtn.TabIndex = 3;
            AddBtn.Text = "添加";
            AddBtn.UseVisualStyleBackColor = true;
            AddBtn.Click += AddBtn_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Location = new Point(6, 83);
            label3.Name = "label3";
            label3.Size = new Size(35, 17);
            label3.TabIndex = 4;
            label3.Text = "参数:";
            // 
            // ProgramPath
            // 
            ProgramPath.Location = new Point(47, 51);
            ProgramPath.Name = "ProgramPath";
            ProgramPath.Size = new Size(337, 23);
            ProgramPath.TabIndex = 3;
            ProgramPath.KeyDown += Program_KeyDown;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(6, 54);
            label2.Name = "label2";
            label2.Size = new Size(35, 17);
            label2.TabIndex = 2;
            label2.Text = "路径:";
            // 
            // ProgramName
            // 
            ProgramName.Location = new Point(47, 22);
            ProgramName.Name = "ProgramName";
            ProgramName.Size = new Size(418, 23);
            ProgramName.TabIndex = 1;
            ProgramName.KeyDown += Program_KeyDown;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(6, 25);
            label1.Name = "label1";
            label1.Size = new Size(35, 17);
            label1.TabIndex = 0;
            label1.Text = "名称:";
            // 
            // StartupList
            // 
            StartupList.Columns.AddRange(new ColumnHeader[] { columnHeader1, columnHeader5, columnHeader2, columnHeader3, columnHeader4 });
            StartupList.FullRowSelect = true;
            StartupList.Location = new Point(12, 213);
            StartupList.Name = "StartupList";
            StartupList.Size = new Size(471, 248);
            StartupList.TabIndex = 2;
            StartupList.UseCompatibleStateImageBehavior = false;
            StartupList.View = View.Details;
            StartupList.KeyDown += StartupList_KeyDown;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "启用";
            columnHeader1.Width = 25;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "隐藏窗口";
            columnHeader5.Width = 25;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "名称";
            columnHeader2.Width = 25;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "路径";
            columnHeader3.Width = 25;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "参数";
            columnHeader4.Width = 36;
            // 
            // OpenFileDialog
            // 
            OpenFileDialog.FileName = "openFileDialog1";
            OpenFileDialog.Filter = "可执行程序|*.exe|所有文件|*.*";
            OpenFileDialog.Title = "选择需要启动程序的路径";
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(KillSelectButton);
            groupBox2.Controls.Add(KillAllButton);
            groupBox2.Controls.Add(StopBtn);
            groupBox2.Controls.Add(StatusLabel);
            groupBox2.Location = new Point(12, 467);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(471, 64);
            groupBox2.TabIndex = 6;
            groupBox2.TabStop = false;
            groupBox2.Text = "状态";
            // 
            // KillSelectButton
            // 
            KillSelectButton.Location = new Point(228, 26);
            KillSelectButton.Name = "KillSelectButton";
            KillSelectButton.Size = new Size(75, 23);
            KillSelectButton.TabIndex = 3;
            KillSelectButton.Text = "终止选中";
            KillSelectButton.UseVisualStyleBackColor = true;
            KillSelectButton.Click += KillSelectButton_Click;
            // 
            // KillAllButton
            // 
            KillAllButton.Location = new Point(309, 26);
            KillAllButton.Name = "KillAllButton";
            KillAllButton.Size = new Size(75, 23);
            KillAllButton.TabIndex = 2;
            KillAllButton.Text = "终止所有";
            KillAllButton.UseVisualStyleBackColor = true;
            KillAllButton.Click += KillAllButton_Click;
            // 
            // StopBtn
            // 
            StopBtn.Enabled = false;
            StopBtn.Location = new Point(390, 26);
            StopBtn.Name = "StopBtn";
            StopBtn.Size = new Size(75, 23);
            StopBtn.TabIndex = 1;
            StopBtn.Text = "终止";
            StopBtn.UseVisualStyleBackColor = true;
            StopBtn.Click += StopBtn_Click;
            // 
            // StatusLabel
            // 
            StatusLabel.AutoSize = true;
            StatusLabel.Location = new Point(10, 29);
            StatusLabel.Name = "StatusLabel";
            StatusLabel.Size = new Size(65, 17);
            StatusLabel.TabIndex = 0;
            StatusLabel.Text = "维护模式...";
            // 
            // MainForm
            // 
            AllowDrop = true;
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(495, 543);
            Controls.Add(groupBox2);
            Controls.Add(StartupList);
            Controls.Add(groupBox1);
            Controls.Add(StartUpSelector);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            Load += MainForm_Load;
            DragDrop += MainForm_DragDrop;
            DragEnter += MainForm_DragEnter;
            DragLeave += MainForm_DragLeave;
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private CheckBox StartUpSelector;
        private GroupBox groupBox1;
        private CheckBox ProgramEnabled;
        private TextBox ProgramArg;
        private Label label3;
        private TextBox ProgramPath;
        private Label label2;
        private TextBox ProgramName;
        private Label label1;
        private ListView StartupList;
        private ColumnHeader columnHeader4;
        private ColumnHeader columnHeader1;
        private ColumnHeader columnHeader2;
        private ColumnHeader columnHeader3;
        private Button AddBtn;
        private Button EditBtn;
        private Button DeleteBtn;
        private Button OpenFileDialogBtn;
        private OpenFileDialog OpenFileDialog;
        private GroupBox groupBox2;
        private Button StopBtn;
        private Label StatusLabel;
        private Button TestBtn;
        private Button KillSelectButton;
        private Button KillAllButton;
        private CheckBox HideWindowSelector;
        private ColumnHeader columnHeader5;
    }
}
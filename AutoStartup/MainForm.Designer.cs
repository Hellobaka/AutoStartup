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
            this.StartUpSelector = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.OpenFileDialogBtn = new System.Windows.Forms.Button();
            this.ProgramEnabled = new System.Windows.Forms.CheckBox();
            this.ProgramArg = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.ProgramPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.ProgramName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.StartupList = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.AddBtn = new System.Windows.Forms.Button();
            this.EditBtn = new System.Windows.Forms.Button();
            this.DeleteBtn = new System.Windows.Forms.Button();
            this.OpenFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.StopBtn = new System.Windows.Forms.Button();
            this.StatusLabel = new System.Windows.Forms.Label();
            this.TestBtn = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // StartUpSelector
            // 
            this.StartUpSelector.AutoSize = true;
            this.StartUpSelector.Location = new System.Drawing.Point(12, 12);
            this.StartUpSelector.Name = "StartUpSelector";
            this.StartUpSelector.Size = new System.Drawing.Size(75, 21);
            this.StartUpSelector.TabIndex = 0;
            this.StartUpSelector.Text = "开机自启";
            this.StartUpSelector.UseVisualStyleBackColor = true;
            this.StartUpSelector.CheckedChanged += new System.EventHandler(this.StartUpSelector_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.OpenFileDialogBtn);
            this.groupBox1.Controls.Add(this.ProgramEnabled);
            this.groupBox1.Controls.Add(this.ProgramArg);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.ProgramPath);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.ProgramName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 39);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(471, 139);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "添加参数 (可拖动文件)";
            // 
            // OpenFileDialogBtn
            // 
            this.OpenFileDialogBtn.Location = new System.Drawing.Point(390, 51);
            this.OpenFileDialogBtn.Name = "OpenFileDialogBtn";
            this.OpenFileDialogBtn.Size = new System.Drawing.Size(75, 23);
            this.OpenFileDialogBtn.TabIndex = 7;
            this.OpenFileDialogBtn.Text = "浏览";
            this.OpenFileDialogBtn.UseVisualStyleBackColor = true;
            this.OpenFileDialogBtn.Click += new System.EventHandler(this.OpenFileDialog_Click);
            // 
            // ProgramEnabled
            // 
            this.ProgramEnabled.AutoSize = true;
            this.ProgramEnabled.Location = new System.Drawing.Point(6, 109);
            this.ProgramEnabled.Name = "ProgramEnabled";
            this.ProgramEnabled.Size = new System.Drawing.Size(51, 21);
            this.ProgramEnabled.TabIndex = 6;
            this.ProgramEnabled.Text = "启用";
            this.ProgramEnabled.UseVisualStyleBackColor = true;
            // 
            // ProgramArg
            // 
            this.ProgramArg.Location = new System.Drawing.Point(47, 80);
            this.ProgramArg.Name = "ProgramArg";
            this.ProgramArg.Size = new System.Drawing.Size(418, 23);
            this.ProgramArg.TabIndex = 5;
            this.ProgramArg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Program_KeyDown);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 83);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 17);
            this.label3.TabIndex = 4;
            this.label3.Text = "参数:";
            // 
            // ProgramPath
            // 
            this.ProgramPath.Location = new System.Drawing.Point(47, 51);
            this.ProgramPath.Name = "ProgramPath";
            this.ProgramPath.Size = new System.Drawing.Size(337, 23);
            this.ProgramPath.TabIndex = 3;
            this.ProgramPath.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Program_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 17);
            this.label2.TabIndex = 2;
            this.label2.Text = "路径:";
            // 
            // ProgramName
            // 
            this.ProgramName.Location = new System.Drawing.Point(47, 22);
            this.ProgramName.Name = "ProgramName";
            this.ProgramName.Size = new System.Drawing.Size(418, 23);
            this.ProgramName.TabIndex = 1;
            this.ProgramName.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Program_KeyDown);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 17);
            this.label1.TabIndex = 0;
            this.label1.Text = "名称:";
            // 
            // StartupList
            // 
            this.StartupList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.StartupList.FullRowSelect = true;
            this.StartupList.Location = new System.Drawing.Point(12, 213);
            this.StartupList.Name = "StartupList";
            this.StartupList.Size = new System.Drawing.Size(471, 248);
            this.StartupList.TabIndex = 2;
            this.StartupList.UseCompatibleStateImageBehavior = false;
            this.StartupList.View = System.Windows.Forms.View.Details;
            this.StartupList.KeyDown += new System.Windows.Forms.KeyEventHandler(this.StartupList_KeyDown);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "启用";
            this.columnHeader1.Width = 25;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "名称";
            this.columnHeader2.Width = 25;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "路径";
            this.columnHeader3.Width = 25;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "参数";
            this.columnHeader4.Width = 36;
            // 
            // AddBtn
            // 
            this.AddBtn.Location = new System.Drawing.Point(12, 184);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(75, 23);
            this.AddBtn.TabIndex = 3;
            this.AddBtn.Text = "添加";
            this.AddBtn.UseVisualStyleBackColor = true;
            this.AddBtn.Click += new System.EventHandler(this.AddBtn_Click);
            // 
            // EditBtn
            // 
            this.EditBtn.Location = new System.Drawing.Point(93, 184);
            this.EditBtn.Name = "EditBtn";
            this.EditBtn.Size = new System.Drawing.Size(75, 23);
            this.EditBtn.TabIndex = 4;
            this.EditBtn.Text = "编辑";
            this.EditBtn.UseVisualStyleBackColor = true;
            this.EditBtn.Click += new System.EventHandler(this.EditBtn_Click);
            // 
            // DeleteBtn
            // 
            this.DeleteBtn.Location = new System.Drawing.Point(174, 184);
            this.DeleteBtn.Name = "DeleteBtn";
            this.DeleteBtn.Size = new System.Drawing.Size(75, 23);
            this.DeleteBtn.TabIndex = 5;
            this.DeleteBtn.Text = "删除";
            this.DeleteBtn.UseVisualStyleBackColor = true;
            this.DeleteBtn.Click += new System.EventHandler(this.DeleteBtn_Click);
            // 
            // OpenFileDialog
            // 
            this.OpenFileDialog.FileName = "openFileDialog1";
            this.OpenFileDialog.Filter = "可执行程序|*.exe|所有文件|*.*";
            this.OpenFileDialog.Title = "选择需要启动程序的路径";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.StopBtn);
            this.groupBox2.Controls.Add(this.StatusLabel);
            this.groupBox2.Location = new System.Drawing.Point(12, 467);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(471, 64);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "状态";
            // 
            // StopBtn
            // 
            this.StopBtn.Enabled = false;
            this.StopBtn.Location = new System.Drawing.Point(390, 26);
            this.StopBtn.Name = "StopBtn";
            this.StopBtn.Size = new System.Drawing.Size(75, 23);
            this.StopBtn.TabIndex = 1;
            this.StopBtn.Text = "终止";
            this.StopBtn.UseVisualStyleBackColor = true;
            this.StopBtn.Click += new System.EventHandler(this.StopBtn_Click);
            // 
            // StatusLabel
            // 
            this.StatusLabel.AutoSize = true;
            this.StatusLabel.Location = new System.Drawing.Point(10, 29);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(65, 17);
            this.StatusLabel.TabIndex = 0;
            this.StatusLabel.Text = "维护模式...";
            // 
            // TestBtn
            // 
            this.TestBtn.Location = new System.Drawing.Point(402, 184);
            this.TestBtn.Name = "TestBtn";
            this.TestBtn.Size = new System.Drawing.Size(75, 23);
            this.TestBtn.TabIndex = 7;
            this.TestBtn.Text = "测试";
            this.TestBtn.UseVisualStyleBackColor = true;
            this.TestBtn.Click += new System.EventHandler(this.TestBtn_Click);
            // 
            // MainForm
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(495, 543);
            this.Controls.Add(this.TestBtn);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.DeleteBtn);
            this.Controls.Add(this.EditBtn);
            this.Controls.Add(this.AddBtn);
            this.Controls.Add(this.StartupList);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.StartUpSelector);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "MainForm";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.MainForm_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.MainForm_DragEnter);
            this.DragLeave += new System.EventHandler(this.MainForm_DragLeave);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
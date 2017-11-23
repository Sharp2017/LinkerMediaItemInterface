namespace LinkerMediaItemInterface
{
    partial class FrmMain
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnStart = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.notifyMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.btnShowForm = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCloseForm = new System.Windows.Forms.ToolStripMenuItem();
            this.txtInfo = new System.Windows.Forms.RichTextBox();
            this.toolStrip1.SuspendLayout();
            this.notifyMenuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnStart,
            this.toolStripSeparator1,
            this.btnSetting});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(669, 27);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // btnStart
            // 
            this.btnStart.Image = global::LinkerMediaItemInterface.Properties.Resources.Start;
            this.btnStart.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(63, 24);
            this.btnStart.Text = "启动";
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 27);
            // 
            // btnSetting
            // 
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(43, 24);
            this.btnSetting.Text = "设置";
            this.btnSetting.Visible = false;
            // 
            // notifyIcon
            // 
            this.notifyIcon.ContextMenuStrip = this.notifyMenuStrip;
            this.notifyIcon.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon.Icon")));
            this.notifyIcon.Text = "媒资导出接口";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // notifyMenuStrip
            // 
            this.notifyMenuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.notifyMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnShowForm,
            this.btnCloseForm});
            this.notifyMenuStrip.Name = "notifyMenuStrip";
            this.notifyMenuStrip.Size = new System.Drawing.Size(109, 52);
            // 
            // btnShowForm
            // 
            this.btnShowForm.Name = "btnShowForm";
            this.btnShowForm.Size = new System.Drawing.Size(108, 24);
            this.btnShowForm.Text = "显示";
            this.btnShowForm.Click += new System.EventHandler(this.btnShowForm_Click);
            // 
            // btnCloseForm
            // 
            this.btnCloseForm.Name = "btnCloseForm";
            this.btnCloseForm.Size = new System.Drawing.Size(108, 24);
            this.btnCloseForm.Text = "退出";
            this.btnCloseForm.Click += new System.EventHandler(this.btnCloseForm_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtInfo.Location = new System.Drawing.Point(0, 27);
            this.txtInfo.Margin = new System.Windows.Forms.Padding(0);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.ReadOnly = true;
            this.txtInfo.Size = new System.Drawing.Size(669, 404);
            this.txtInfo.TabIndex = 3;
            this.txtInfo.Text = "";
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 431);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.toolStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FrmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "媒资导出接口";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMain_FormClosing);
            this.Load += new System.EventHandler(this.FrmMain_Load);
            this.SizeChanged += new System.EventHandler(this.FrmMain_SizeChanged);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.notifyMenuStrip.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnStart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnSetting;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip notifyMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem btnShowForm;
        private System.Windows.Forms.ToolStripMenuItem btnCloseForm;
        private System.Windows.Forms.RichTextBox txtInfo;
    }
}


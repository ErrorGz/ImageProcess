using System.Windows.Forms;

namespace ImageLabel
{
    partial class frmPlay
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.摄像头ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.开始拍摄ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.结束拍摄ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.视频文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.停止视频ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.保存视频ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.图片文件ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.打开图片ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripComboBoxWindows = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripComboBox2 = new System.Windows.Forms.ToolStripComboBox();
            this.运动阈值ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.backgroundWorker2 = new System.ComponentModel.BackgroundWorker();
            this.trackBarControl1 = new DevExpress.XtraEditors.TrackBarControl();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.摄像头ToolStripMenuItem,
            this.视频文件ToolStripMenuItem,
            this.图片文件ToolStripMenuItem,
            this.toolStripComboBoxWindows,
            this.toolStripComboBox2,
            this.运动阈值ToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(4, 1, 0, 1);
            this.menuStrip1.Size = new System.Drawing.Size(764, 27);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // 摄像头ToolStripMenuItem
            // 
            this.摄像头ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.开始拍摄ToolStripMenuItem1,
            this.结束拍摄ToolStripMenuItem1});
            this.摄像头ToolStripMenuItem.Name = "摄像头ToolStripMenuItem";
            this.摄像头ToolStripMenuItem.Size = new System.Drawing.Size(56, 25);
            this.摄像头ToolStripMenuItem.Text = "摄像头";
            // 
            // 开始拍摄ToolStripMenuItem1
            // 
            this.开始拍摄ToolStripMenuItem1.Name = "开始拍摄ToolStripMenuItem1";
            this.开始拍摄ToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.开始拍摄ToolStripMenuItem1.Text = "开始拍摄";
            this.开始拍摄ToolStripMenuItem1.Click += new System.EventHandler(this.开始拍摄ToolStripMenuItem_Click);
            // 
            // 结束拍摄ToolStripMenuItem1
            // 
            this.结束拍摄ToolStripMenuItem1.Name = "结束拍摄ToolStripMenuItem1";
            this.结束拍摄ToolStripMenuItem1.Size = new System.Drawing.Size(124, 22);
            this.结束拍摄ToolStripMenuItem1.Text = "结束拍摄";
            this.结束拍摄ToolStripMenuItem1.Click += new System.EventHandler(this.结束拍摄ToolStripMenuItem_Click);
            // 
            // 视频文件ToolStripMenuItem
            // 
            this.视频文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开文件ToolStripMenuItem,
            this.停止视频ToolStripMenuItem,
            this.保存视频ToolStripMenuItem});
            this.视频文件ToolStripMenuItem.Name = "视频文件ToolStripMenuItem";
            this.视频文件ToolStripMenuItem.Size = new System.Drawing.Size(68, 25);
            this.视频文件ToolStripMenuItem.Text = "视频文件";
            // 
            // 打开文件ToolStripMenuItem
            // 
            this.打开文件ToolStripMenuItem.Name = "打开文件ToolStripMenuItem";
            this.打开文件ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.打开文件ToolStripMenuItem.Text = "打开文件";
            this.打开文件ToolStripMenuItem.Click += new System.EventHandler(this.打开文件ToolStripMenuItem_Click);
            // 
            // 停止视频ToolStripMenuItem
            // 
            this.停止视频ToolStripMenuItem.Name = "停止视频ToolStripMenuItem";
            this.停止视频ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.停止视频ToolStripMenuItem.Text = "停止视频";
            this.停止视频ToolStripMenuItem.Click += new System.EventHandler(this.停止视频ToolStripMenuItem_Click);
            // 
            // 保存视频ToolStripMenuItem
            // 
            this.保存视频ToolStripMenuItem.CheckOnClick = true;
            this.保存视频ToolStripMenuItem.Name = "保存视频ToolStripMenuItem";
            this.保存视频ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.保存视频ToolStripMenuItem.Text = "保存视频";
            this.保存视频ToolStripMenuItem.Click += new System.EventHandler(this.保存视频ToolStripMenuItem_Click);
            // 
            // 图片文件ToolStripMenuItem
            // 
            this.图片文件ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.打开图片ToolStripMenuItem});
            this.图片文件ToolStripMenuItem.Name = "图片文件ToolStripMenuItem";
            this.图片文件ToolStripMenuItem.Size = new System.Drawing.Size(68, 25);
            this.图片文件ToolStripMenuItem.Text = "图片文件";
            // 
            // 打开图片ToolStripMenuItem
            // 
            this.打开图片ToolStripMenuItem.Name = "打开图片ToolStripMenuItem";
            this.打开图片ToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.打开图片ToolStripMenuItem.Text = "打开图片";
            this.打开图片ToolStripMenuItem.Click += new System.EventHandler(this.打开图片ToolStripMenuItem_Click);
            // 
            // toolStripComboBoxWindows
            // 
            this.toolStripComboBoxWindows.Items.AddRange(new object[] {
            "1窗口",
            "2窗口左右分割",
            "2窗口上下分割",
            "4窗口网格分割",
            "4窗口水平分割"});
            this.toolStripComboBoxWindows.Name = "toolStripComboBoxWindows";
            this.toolStripComboBoxWindows.Size = new System.Drawing.Size(121, 25);
            this.toolStripComboBoxWindows.SelectedIndexChanged += new System.EventHandler(this.toolStripComboBoxWindows_SelectedIndexChanged);
            // 
            // toolStripComboBox2
            // 
            this.toolStripComboBox2.Items.AddRange(new object[] {
            "正常显示",
            "增强显示"});
            this.toolStripComboBox2.Name = "toolStripComboBox2";
            this.toolStripComboBox2.Size = new System.Drawing.Size(121, 25);
            // 
            // 运动阈值ToolStripMenuItem
            // 
            this.运动阈值ToolStripMenuItem.Name = "运动阈值ToolStripMenuItem";
            this.运动阈值ToolStripMenuItem.Size = new System.Drawing.Size(68, 25);
            this.运动阈值ToolStripMenuItem.Text = "运动阈值";
            this.运动阈值ToolStripMenuItem.Click += new System.EventHandler(this.运动阈值ToolStripMenuItem_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            this.backgroundWorker1.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker1_RunWorkerCompleted);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.GrowStyle = System.Windows.Forms.TableLayoutPanelGrowStyle.AddColumns;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 27);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(764, 412);
            this.tableLayoutPanel1.TabIndex = 2;
            // 
            // backgroundWorker2
            // 
            this.backgroundWorker2.WorkerReportsProgress = true;
            this.backgroundWorker2.WorkerSupportsCancellation = true;
            this.backgroundWorker2.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker2_DoWork);
            this.backgroundWorker2.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker2_ProgressChanged);
            this.backgroundWorker2.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.backgroundWorker2_RunWorkerCompleted);
            // 
            // trackBarControl1
            // 
            this.trackBarControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.trackBarControl1.Location = new System.Drawing.Point(0, 439);
            this.trackBarControl1.Name = "trackBarControl1";
            this.trackBarControl1.Properties.LabelAppearance.Options.UseTextOptions = true;
            this.trackBarControl1.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.trackBarControl1.Size = new System.Drawing.Size(764, 45);
            this.trackBarControl1.TabIndex = 3;
            // 
            // frmPlay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(764, 484);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.trackBarControl1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "frmPlay";
            this.Text = "播放与检测";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.frmPlay_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MenuStrip menuStrip1;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
        private TableLayoutPanel tableLayoutPanel1;
        private ToolStripMenuItem 摄像头ToolStripMenuItem;
        private ToolStripMenuItem 开始拍摄ToolStripMenuItem1;
        private ToolStripMenuItem 结束拍摄ToolStripMenuItem1;
        private System.ComponentModel.BackgroundWorker backgroundWorker2;
        private ToolStripMenuItem 视频文件ToolStripMenuItem;
        private ToolStripMenuItem 打开文件ToolStripMenuItem;
        private ToolStripMenuItem 停止视频ToolStripMenuItem;
        private ToolStripMenuItem 图片文件ToolStripMenuItem;
        private ToolStripMenuItem 打开图片ToolStripMenuItem;
        private ToolStripComboBox toolStripComboBox2;
        private ToolStripComboBox toolStripComboBoxWindows;
        private ToolStripMenuItem 运动阈值ToolStripMenuItem;
        private ToolStripMenuItem 保存视频ToolStripMenuItem;
        private DevExpress.XtraEditors.TrackBarControl trackBarControl1;
    }
}
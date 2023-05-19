namespace ImageLabel
{
    partial class frmInputVideo
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
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.rangeTrackBarControl1 = new DevExpress.XtraEditors.RangeTrackBarControl();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.trackBarControl1 = new DevExpress.XtraEditors.TrackBarControl();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBoxXFlip = new System.Windows.Forms.CheckBox();
            this.checkBoxYFlip = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.rangeTrackBarControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rangeTrackBarControl1.Properties)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1.Properties)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.Location = new System.Drawing.Point(890, 445);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 34);
            this.button1.TabIndex = 0;
            this.button1.Text = "确定";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.Location = new System.Drawing.Point(772, 445);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 34);
            this.button2.TabIndex = 1;
            this.button2.Text = "取消";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // rangeTrackBarControl1
            // 
            this.rangeTrackBarControl1.EditValue = new DevExpress.XtraEditors.Repository.TrackBarRange(0, 0);
            this.rangeTrackBarControl1.Location = new System.Drawing.Point(50, 337);
            this.rangeTrackBarControl1.Name = "rangeTrackBarControl1";
            this.rangeTrackBarControl1.Properties.LabelAppearance.Options.UseTextOptions = true;
            this.rangeTrackBarControl1.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.rangeTrackBarControl1.Properties.Maximum = 100;
            this.rangeTrackBarControl1.Properties.ShowLabels = true;
            this.rangeTrackBarControl1.Properties.TickStyle = System.Windows.Forms.TickStyle.None;
            this.rangeTrackBarControl1.Size = new System.Drawing.Size(952, 69);
            this.rangeTrackBarControl1.TabIndex = 2;
            this.rangeTrackBarControl1.EditValueChanged += new System.EventHandler(this.rangeTrackBarControl1_EditValueChanged);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.pictureBox1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.pictureBox2, 1, 0);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(50, 12);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(952, 319);
            this.tableLayoutPanel1.TabIndex = 3;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.Location = new System.Drawing.Point(3, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(470, 313);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            // 
            // pictureBox2
            // 
            this.pictureBox2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox2.Location = new System.Drawing.Point(479, 3);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(470, 313);
            this.pictureBox2.TabIndex = 1;
            this.pictureBox2.TabStop = false;
            this.pictureBox2.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox2_Paint);
            // 
            // trackBarControl1
            // 
            this.trackBarControl1.EditValue = 15;
            this.trackBarControl1.Location = new System.Drawing.Point(192, 450);
            this.trackBarControl1.Name = "trackBarControl1";
            this.trackBarControl1.Properties.LabelAppearance.Options.UseTextOptions = true;
            this.trackBarControl1.Properties.LabelAppearance.TextOptions.HAlignment = DevExpress.Utils.HorzAlignment.Center;
            this.trackBarControl1.Properties.Maximum = 30;
            this.trackBarControl1.Size = new System.Drawing.Size(192, 69);
            this.trackBarControl1.TabIndex = 4;
            this.trackBarControl1.Value = 15;
            this.trackBarControl1.EditValueChanging += new DevExpress.XtraEditors.Controls.ChangingEventHandler(this.trackBarControl1_EditValueChanging);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(50, 450);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(136, 24);
            this.label1.TabIndex = 5;
            this.label1.Text = "设置采集帧率：";
            // 
            // checkBoxXFlip
            // 
            this.checkBoxXFlip.AutoSize = true;
            this.checkBoxXFlip.Location = new System.Drawing.Point(457, 449);
            this.checkBoxXFlip.Name = "checkBoxXFlip";
            this.checkBoxXFlip.Size = new System.Drawing.Size(102, 28);
            this.checkBoxXFlip.TabIndex = 6;
            this.checkBoxXFlip.Text = "X轴翻转";
            this.checkBoxXFlip.UseVisualStyleBackColor = true;
            this.checkBoxXFlip.CheckedChanged += new System.EventHandler(this.checkBoxXFlip_CheckedChanged);
            // 
            // checkBoxYFlip
            // 
            this.checkBoxYFlip.AutoSize = true;
            this.checkBoxYFlip.Location = new System.Drawing.Point(574, 451);
            this.checkBoxYFlip.Name = "checkBoxYFlip";
            this.checkBoxYFlip.Size = new System.Drawing.Size(101, 28);
            this.checkBoxYFlip.TabIndex = 6;
            this.checkBoxYFlip.Text = "Y轴翻转";
            this.checkBoxYFlip.UseVisualStyleBackColor = true;
            this.checkBoxYFlip.CheckedChanged += new System.EventHandler(this.checkBoxYFlip_CheckedChanged);
            // 
            // frmInputVideo
            // 
            this.AcceptButton = this.button1;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(1046, 533);
            this.Controls.Add(this.checkBoxYFlip);
            this.Controls.Add(this.checkBoxXFlip);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarControl1);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.rangeTrackBarControl1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmInputVideo";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "frmWorkSpace";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmInputVideo_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.rangeTrackBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rangeTrackBarControl1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1.Properties)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarControl1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private DevExpress.XtraEditors.RangeTrackBarControl rangeTrackBarControl1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox2;
        private DevExpress.XtraEditors.TrackBarControl trackBarControl1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBoxXFlip;
        private System.Windows.Forms.CheckBox checkBoxYFlip;
    }
}
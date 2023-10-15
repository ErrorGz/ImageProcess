namespace ImageDetectWinForm
{
    partial class frmMain
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
            toolStrip1 = new ToolStrip();
            toolStripButtonReverse = new ToolStripButton();
            toolStripButtonPlayPause = new ToolStripButton();
            toolStripButtonAdvance = new ToolStripButton();
            toolStripButtonRecord = new ToolStripButton();
            pictureBox1 = new PictureBox();
            toolStripButtonLiveDetect = new ToolStripButton();
            toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.ImageScalingSize = new Size(32, 32);
            toolStrip1.Items.AddRange(new ToolStripItem[] { toolStripButtonReverse, toolStripButtonPlayPause, toolStripButtonAdvance, toolStripButtonRecord, toolStripButtonLiveDetect });
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(800, 39);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButtonReverse
            // 
            toolStripButtonReverse.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonReverse.Image = Properties.Resources.icons8_return_64;
            toolStripButtonReverse.ImageTransparentColor = Color.Magenta;
            toolStripButtonReverse.Name = "toolStripButtonReverse";
            toolStripButtonReverse.Size = new Size(36, 36);
            toolStripButtonReverse.Text = "toolStripButton1";
            toolStripButtonReverse.Click += toolStripButtonReverse_Click;
            // 
            // toolStripButtonPlayPause
            // 
            toolStripButtonPlayPause.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonPlayPause.Image = Properties.Resources.icons8_pause_64;
            toolStripButtonPlayPause.ImageTransparentColor = Color.Magenta;
            toolStripButtonPlayPause.Name = "toolStripButtonPlayPause";
            toolStripButtonPlayPause.Size = new Size(36, 36);
            toolStripButtonPlayPause.Text = "toolStripButton2";
            toolStripButtonPlayPause.Click += toolStripButtonPlayPause_Click;
            // 
            // toolStripButtonAdvance
            // 
            toolStripButtonAdvance.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonAdvance.Image = Properties.Resources.icons8_advance_64;
            toolStripButtonAdvance.ImageTransparentColor = Color.Magenta;
            toolStripButtonAdvance.Name = "toolStripButtonAdvance";
            toolStripButtonAdvance.Size = new Size(36, 36);
            toolStripButtonAdvance.Text = "toolStripButton1";
            toolStripButtonAdvance.Click += toolStripButtonAdvance_Click;
            // 
            // toolStripButtonRecord
            // 
            toolStripButtonRecord.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonRecord.Image = Properties.Resources.icons8_record_64;
            toolStripButtonRecord.ImageTransparentColor = Color.Magenta;
            toolStripButtonRecord.Name = "toolStripButtonRecord";
            toolStripButtonRecord.Size = new Size(36, 36);
            toolStripButtonRecord.Text = "toolStripButton1";
            toolStripButtonRecord.Click += toolStripButtonRecord_Click;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 39);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(800, 411);
            pictureBox1.TabIndex = 2;
            pictureBox1.TabStop = false;
            // 
            // toolStripButtonLiveDetect
            // 
            toolStripButtonLiveDetect.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonLiveDetect.Image = Properties.Resources.icons8_offline_64;
            toolStripButtonLiveDetect.ImageTransparentColor = Color.Magenta;
            toolStripButtonLiveDetect.Name = "toolStripButtonLiveDetect";
            toolStripButtonLiveDetect.Size = new Size(36, 36);
            toolStripButtonLiveDetect.Text = "toolStripButton1";
            toolStripButtonLiveDetect.Click += toolStripButtonLiveDetect_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(pictureBox1);
            Controls.Add(toolStrip1);
            Name = "Form1";
            Text = "作业标准化自动识别";
            FormClosing += Form1_FormClosing;
            Load += Form1_Load;
            toolStrip1.ResumeLayout(false);
            toolStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private PictureBox pictureBox1;
        private ToolStripButton toolStripButtonReverse;
        private ToolStripButton toolStripButtonPlayPause;
        private ToolStripButton toolStripButtonAdvance;
        private ToolStripButton toolStripButtonRecord;
        private ToolStripButton toolStripButtonLiveDetect;
    }
}
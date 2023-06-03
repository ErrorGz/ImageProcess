namespace VideoLabel
{
    partial class PlayForm
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
            toolStrip1 = new ToolStrip();
            splitContainer1 = new SplitContainer();
            listBox1 = new ListBox();
            splitContainer2 = new SplitContainer();
            splitContainer3 = new SplitContainer();
            pictureBox1 = new PictureBox();
            dualTrackBar1 = new DualTrackBar();
            tableLayoutPanel1 = new TableLayoutPanel();
            splitContainer4 = new SplitContainer();
            toolStrip2 = new ToolStrip();
            toolStripButtonPlay = new ToolStripButton();
            toolStripButtonMake = new ToolStripButton();
            flowLayoutPanel1 = new FlowLayoutPanel();
            textBox1 = new TextBox();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer2).BeginInit();
            splitContainer2.Panel1.SuspendLayout();
            splitContainer2.Panel2.SuspendLayout();
            splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer3).BeginInit();
            splitContainer3.Panel1.SuspendLayout();
            splitContainer3.Panel2.SuspendLayout();
            splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)splitContainer4).BeginInit();
            splitContainer4.Panel1.SuspendLayout();
            splitContainer4.Panel2.SuspendLayout();
            splitContainer4.SuspendLayout();
            toolStrip2.SuspendLayout();
            SuspendLayout();
            // 
            // toolStrip1
            // 
            toolStrip1.Location = new Point(0, 0);
            toolStrip1.Name = "toolStrip1";
            toolStrip1.Size = new Size(987, 25);
            toolStrip1.TabIndex = 0;
            toolStrip1.Text = "toolStrip1";
            // 
            // splitContainer1
            // 
            splitContainer1.BorderStyle = BorderStyle.FixedSingle;
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 25);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(listBox1);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(splitContainer2);
            splitContainer1.Size = new Size(987, 533);
            splitContainer1.SplitterDistance = 202;
            splitContainer1.TabIndex = 1;
            // 
            // listBox1
            // 
            listBox1.AllowDrop = true;
            listBox1.Dock = DockStyle.Fill;
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 17;
            listBox1.Location = new Point(0, 0);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(200, 531);
            listBox1.TabIndex = 0;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            listBox1.DragDrop += listBox1_DragDrop;
            listBox1.DragEnter += listBox1_DragEnter;
            // 
            // splitContainer2
            // 
            splitContainer2.Dock = DockStyle.Fill;
            splitContainer2.Location = new Point(0, 0);
            splitContainer2.Name = "splitContainer2";
            splitContainer2.Orientation = Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            splitContainer2.Panel1.Controls.Add(splitContainer3);
            // 
            // splitContainer2.Panel2
            // 
            splitContainer2.Panel2.Controls.Add(splitContainer4);
            splitContainer2.Size = new Size(779, 531);
            splitContainer2.SplitterDistance = 376;
            splitContainer2.TabIndex = 0;
            // 
            // splitContainer3
            // 
            splitContainer3.BorderStyle = BorderStyle.FixedSingle;
            splitContainer3.Dock = DockStyle.Fill;
            splitContainer3.Location = new Point(0, 0);
            splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            splitContainer3.Panel1.Controls.Add(pictureBox1);
            splitContainer3.Panel1.Controls.Add(dualTrackBar1);
            // 
            // splitContainer3.Panel2
            // 
            splitContainer3.Panel2.Controls.Add(tableLayoutPanel1);
            splitContainer3.Size = new Size(779, 376);
            splitContainer3.SplitterDistance = 307;
            splitContainer3.TabIndex = 0;
            // 
            // pictureBox1
            // 
            pictureBox1.Dock = DockStyle.Fill;
            pictureBox1.Location = new Point(0, 0);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(305, 351);
            pictureBox1.TabIndex = 1;
            pictureBox1.TabStop = false;
            pictureBox1.Paint += pictureBox1_Paint;
            // 
            // dualTrackBar1
            // 
            dualTrackBar1.Dock = DockStyle.Bottom;
            dualTrackBar1.Location = new Point(0, 351);
            dualTrackBar1.Maximum = 100;
            dualTrackBar1.Minimum = 0;
            dualTrackBar1.Name = "dualTrackBar1";
            dualTrackBar1.Size = new Size(305, 23);
            dualTrackBar1.TabIndex = 0;
            dualTrackBar1.Text = "dualTrackBar1";
            dualTrackBar1.ThumbSize = 16;
            dualTrackBar1.TickFrequency = 10;
            dualTrackBar1.Value1 = 0;
            dualTrackBar1.Value2 = 100;
            dualTrackBar1.OnValue1Changed += DualTrackBar1_OnValue1Changed;
            dualTrackBar1.OnValue2Changed += DualTrackBar1_OnValue2Changed;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.ColumnCount = 2;
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Dock = DockStyle.Fill;
            tableLayoutPanel1.Location = new Point(0, 0);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 2;
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tableLayoutPanel1.Size = new Size(466, 374);
            tableLayoutPanel1.TabIndex = 0;
            // 
            // splitContainer4
            // 
            splitContainer4.BorderStyle = BorderStyle.FixedSingle;
            splitContainer4.Dock = DockStyle.Fill;
            splitContainer4.Location = new Point(0, 0);
            splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            splitContainer4.Panel1.Controls.Add(textBox1);
            splitContainer4.Panel1.Controls.Add(toolStrip2);
            // 
            // splitContainer4.Panel2
            // 
            splitContainer4.Panel2.Controls.Add(flowLayoutPanel1);
            splitContainer4.Size = new Size(779, 151);
            splitContainer4.SplitterDistance = 556;
            splitContainer4.TabIndex = 0;
            // 
            // toolStrip2
            // 
            toolStrip2.ImageScalingSize = new Size(32, 32);
            toolStrip2.Items.AddRange(new ToolStripItem[] { toolStripButtonPlay, toolStripButtonMake });
            toolStrip2.Location = new Point(0, 0);
            toolStrip2.Name = "toolStrip2";
            toolStrip2.Size = new Size(554, 39);
            toolStrip2.TabIndex = 0;
            toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButtonPlay
            // 
            toolStripButtonPlay.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonPlay.Image = Resource.icons8_play_64;
            toolStripButtonPlay.ImageTransparentColor = Color.Magenta;
            toolStripButtonPlay.Name = "toolStripButtonPlay";
            toolStripButtonPlay.Size = new Size(36, 36);
            toolStripButtonPlay.Text = "Play";
            toolStripButtonPlay.Click += toolStripButton1_Click;
            // 
            // toolStripButtonMake
            // 
            toolStripButtonMake.DisplayStyle = ToolStripItemDisplayStyle.Image;
            toolStripButtonMake.Image = Resource.icons8_networking_manager_64;
            toolStripButtonMake.ImageTransparentColor = Color.Magenta;
            toolStripButtonMake.Name = "toolStripButtonMake";
            toolStripButtonMake.Size = new Size(36, 36);
            toolStripButtonMake.Text = "Make";
            toolStripButtonMake.Click += toolStripButton2_Click;
            // 
            // flowLayoutPanel1
            // 
            flowLayoutPanel1.Dock = DockStyle.Fill;
            flowLayoutPanel1.Location = new Point(0, 0);
            flowLayoutPanel1.Name = "flowLayoutPanel1";
            flowLayoutPanel1.Size = new Size(217, 149);
            flowLayoutPanel1.TabIndex = 0;
            // 
            // textBox1
            // 
            textBox1.Dock = DockStyle.Fill;
            textBox1.Location = new Point(0, 39);
            textBox1.Multiline = true;
            textBox1.Name = "textBox1";
            textBox1.Size = new Size(554, 110);
            textBox1.TabIndex = 1;
            // 
            // PlayForm
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(987, 558);
            Controls.Add(splitContainer1);
            Controls.Add(toolStrip1);
            Name = "PlayForm";
            Text = "PlayForm";
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            splitContainer2.Panel1.ResumeLayout(false);
            splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer2).EndInit();
            splitContainer2.ResumeLayout(false);
            splitContainer3.Panel1.ResumeLayout(false);
            splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer3).EndInit();
            splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            splitContainer4.Panel1.ResumeLayout(false);
            splitContainer4.Panel1.PerformLayout();
            splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer4).EndInit();
            splitContainer4.ResumeLayout(false);
            toolStrip2.ResumeLayout(false);
            toolStrip2.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private ToolStrip toolStrip1;
        private SplitContainer splitContainer1;
        private ListBox listBox1;
        private SplitContainer splitContainer2;
        private SplitContainer splitContainer3;
        private PictureBox pictureBox1;
        private DualTrackBar dualTrackBar1;
        private TableLayoutPanel tableLayoutPanel1;
        private SplitContainer splitContainer4;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButtonPlay;
        private ToolStripButton toolStripButtonMake;
        private FlowLayoutPanel flowLayoutPanel1;
        private TextBox textBox1;
    }
}
using DevExpress.XtraRichEdit.Import.Html;
using DXApplication;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageLabel
{
    public partial class frmInputVideo : Form
    {

        VideoCapture capture;
        public int StartPos { get; private set; }
        public int EndPos { get; private set; }
        public int FPS { get; private set; }

        public bool XFlip { get; private set; }
        public bool YFlip { get; private set; }
        public frmInputVideo(string file)
        {
            InitializeComponent();
            capture = VideoCapture.FromFile(file);
            var fc = capture.Get(VideoCaptureProperties.FrameCount);
            var fps = capture.Get(VideoCaptureProperties.Fps);
            var total = fc / fps;
            SetMaxValue((int)fc - 1);
            FPS = trackBarControl1.Value;
        }

        public void SetMaxValue(int count)
        {
            rangeTrackBarControl1.Properties.Maximum = count;
            for (int i = 0; i < count; i += count / 10)
                rangeTrackBarControl1.Properties.Labels.Add(new DevExpress.XtraEditors.Repository.TrackBarLabel() { Label = $"{i}", Value = i, Visible = true });

        }

        private void rangeTrackBarControl1_EditValueChanged(object sender, EventArgs e)
        {
            StartPos = rangeTrackBarControl1.Value.Minimum;
            EndPos = rangeTrackBarControl1.Value.Maximum;

            pictureBox1.Refresh();
            pictureBox2.Refresh();
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var ctrl = sender as PictureBox;
            Debug.WriteLine(rangeTrackBarControl1.Value.Minimum);
            capture.Set(VideoCaptureProperties.PosFrames, rangeTrackBarControl1.Value.Minimum);
            var mat = capture.RetrieveMat();
            if (checkBoxXFlip.Checked)
                mat = mat.Flip(FlipMode.X);
            if (checkBoxYFlip.Checked)
                mat = mat.Flip(FlipMode.Y);
            var bitmap = BitmapConverter.ToBitmap(mat);
            var img = StaticLib.resizeImage((Image)bitmap, ctrl.ClientSize);
            e.Graphics.DrawImage(img, 0, 0);
        }

        private void pictureBox2_Paint(object sender, PaintEventArgs e)
        {
            var ctrl = sender as PictureBox;
            Debug.WriteLine(rangeTrackBarControl1.Value.Maximum);
            capture.Set(VideoCaptureProperties.PosFrames, rangeTrackBarControl1.Value.Maximum);
            var mat = capture.RetrieveMat();
            if (checkBoxXFlip.Checked)
                mat = mat.Flip(FlipMode.X);
            if (checkBoxYFlip.Checked)
                mat = mat.Flip(FlipMode.Y);
            var bitmap = BitmapConverter.ToBitmap(mat);
            var img = StaticLib.resizeImage((Image)bitmap, ctrl.ClientSize);
            e.Graphics.DrawImage(img, 0, 0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FPS = trackBarControl1.Value;
            XFlip = checkBoxXFlip.Checked;
            YFlip = checkBoxYFlip.Checked;
        }

        private void frmInputVideo_FormClosing(object sender, FormClosingEventArgs e)
        {
            capture.Dispose();
        }

        private void trackBarControl1_EditValueChanging(object sender, DevExpress.XtraEditors.Controls.ChangingEventArgs e)
        {

            trackBarControl1.ToolTip = trackBarControl1.Value.ToString();
            trackBarControl1.ShowToolTips = true;
        }

        private void checkBoxXFlip_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();
        }

        private void checkBoxYFlip_CheckedChanged(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox2.Refresh();
        }
    }
}

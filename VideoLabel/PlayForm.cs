using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoLabel
{
    public partial class PlayForm : Form
    {
        VideoCapture capture = new VideoCapture();
        VideoProject vp = new VideoProject();
        VideoData _CurrentVideoData;
        System.Drawing.Image CurrentImage;

        Thread DisplayThread;
        bool DisplayThreadRunningFlag = false;
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        public VideoData CurrentVideoData
        {
            get
            {
                return _CurrentVideoData;
            }
            set
            {
                _CurrentVideoData = value;
                if (value != null)
                {
                    SetVideoFile(_CurrentVideoData);
                    ReadImageFromCapture();
                    pictureBox1.Invalidate();
                }
            }
        }


        public PlayForm()
        {
            InitializeComponent();
        }


        private void SetTableView(VideoData vd)
        {
            var labelcount = 3;
            var framecount = 10;
            //var labelcount = vd.frames.Count;
            //if (labelcount == 0)
            //    return;
            //var framecount = vd.frames.Select(o => o.FrameCount).Max();
            if (labelcount > 0 && framecount > 0)
            {
                tableLayoutPanel1.SuspendLayout();
                // 重新设置行和列
                tableLayoutPanel1.RowCount = labelcount;
                tableLayoutPanel1.ColumnCount = framecount;
                tableLayoutPanel1.RowStyles.Clear();
                tableLayoutPanel1.ColumnStyles.Clear();
                tableLayoutPanel1.Controls.Clear();

                // 添加新的行和列
                for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
                {
                    tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
                }
                for (int i = 0; i < tableLayoutPanel1.ColumnCount; i++)
                {
                    tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200));
                }

                // 添加 PictureBox 控件到每个格子
                for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
                {
                    for (int col = 0; col < tableLayoutPanel1.ColumnCount; col++)
                    {
                        PictureBox pictureBox = new PictureBox();
                        pictureBox.Dock = DockStyle.Fill;
                        pictureBox.BorderStyle = BorderStyle.FixedSingle;

                        pictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
                        tableLayoutPanel1.Controls.Add(pictureBox, col, row);
                    }
                }


                // 设置 tableLayoutPanel1 的大小
                int totalWidth = tableLayoutPanel1.ColumnStyles.Cast<ColumnStyle>().Sum(cs => (int)cs.Width);
                int totalHeight = tableLayoutPanel1.RowStyles.Cast<RowStyle>().Sum(rs => (int)rs.Height);
                tableLayoutPanel1.Size = new System.Drawing.Size(totalWidth, totalHeight);
                // 重新布局控件
                tableLayoutPanel1.ResumeLayout(true);


            }
        }
        private void DualTrackBar1_OnValue1Changed(object? sender, EventArgs e)
        {
            Debug.WriteLine(dualTrackBar1.Value1);
            ReadImageFromCapture(dualTrackBar1.Value1);
            pictureBox1.Invalidate();

        }
        private void DualTrackBar1_OnValue2Changed(object? sender, EventArgs e)
        {
            Debug.WriteLine(dualTrackBar1.Value2);
            ReadImageFromCapture(dualTrackBar1.Value2);
            pictureBox1.Invalidate();
        }


        public void ReadImageFromCapture(int? pos = null)
        {
            if (pos != null)
                capture.Set(VideoCaptureProperties.PosFrames, pos.Value);
            var frame = new Mat();
            capture.Read(frame);
            var frame2 = Resize(frame, pictureBox1.Width, pictureBox1.Height);
            CurrentImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame2);

            frame.Dispose();
            frame2.Dispose();
            GC.Collect(10);
        }
        public static new OpenCvSharp.Mat Resize(Mat img_mat, int w_new, int h_new)
        {
            //如果img_mat没有图像，则退出
            if (img_mat.Empty())
                return img_mat;

            Mat output = null;
            var 原图比例 = img_mat.Width / img_mat.Height;
            var 新图比例 = w_new / h_new;


            if (w_new > h_new)
            {
                //宽图片，处理高度
                var temp_height = img_mat.Height * ((float)w_new / img_mat.Width);
                var size = new OpenCvSharp.Size(w_new, temp_height);
                output = img_mat.Resize(size);

            }
            else
            {
                //高图片，处理宽度
                var temp_width = img_mat.Width * ((float)h_new / img_mat.Height);
                var size = new OpenCvSharp.Size(temp_width, h_new);
                output = img_mat.Resize(size);


            }
            var top_bottom = 0;
            var left_right = 0;
            if (h_new > output.Height)
                top_bottom = (h_new - output.Height) / 2;
            if (w_new > output.Width)
                left_right = (w_new - output.Width) / 2;
            if (top_bottom > 0 || left_right > 0)
                output = output.CopyMakeBorder(top_bottom, top_bottom, left_right, left_right, BorderTypes.Constant, Scalar.Black);


            return output;
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            var s = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).ToList();
            Task.Run(() =>
            {
                s.ForEach(file =>
                {
                    if (Directory.Exists(file))
                    {
                        var files = Directory.GetFiles(file, "*.*", SearchOption.AllDirectories).Where(y => StaticLib.VideoFile.Contains(Path.GetExtension(y).ToLower())).ToList();
                        files.ForEach(file =>
                        {
                            AddInVideoProject(file);
                        });
                    }
                    else if (File.Exists(file))
                    {
                        if (StaticLib.VideoFile.Contains(Path.GetExtension(file).ToLower()))
                        {
                            AddInVideoProject(file);
                        }
                    }
                });

            });
        }

        private void AddInVideoProject(string file)
        {

            var exists = vp.VideoDatas.Where(o => o.VideoFile == file).Any();
            if (exists == false)
            {
                listBox1.Invoke(() =>
                {
                    vp.VideoDatas.Add(new VideoData() { VideoFile = file });
                    listBox1.DisplayMember = null;
                    listBox1.DisplayMember = "VideoFile";
                    listBox1.DataSource = vp.VideoDatas;
                });

            }
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                CurrentVideoData = listBox1.SelectedItem as VideoData;
                SetTableView(CurrentVideoData);
            }

        }

        private void SetVideoFile(VideoData vd)
        {
            if (vd != null && vd.VideoFile != null && File.Exists(vd.VideoFile))
            {
                capture.Open(vd.VideoFile);
                var FrameCount = capture.Get(VideoCaptureProperties.FrameCount);
                var VideoFPS = capture.Get(VideoCaptureProperties.Fps);

                dualTrackBar1.Value1 = 0;
                dualTrackBar1.Value2 = dualTrackBar1.Maximum = (int)FrameCount;
                using (Mat frame = new Mat())
                {
                    capture.Set(VideoCaptureProperties.PosFrames, 0);
                    capture.Read(frame);
                    CurrentImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                }
            }

        }


        private void toolStripButton1_Click(object sender, EventArgs e)
        {

            if (capture.IsOpened())
            {
                if (DisplayThreadRunningFlag == true)
                {
                    if (DisplayThread != null)
                    {
                        //终止线程 

                        cancellationTokenSource.Cancel();
                        Thread.Sleep(100);
                        DisplayThread = null;
                    }
                    DisplayThreadRunningFlag = false;
                    this.Invoke(() => { toolStripButtonPlay.Image = Resource.icons8_play_64; });
                    return;
                }

                else
                {
                    Stopwatch sw = new Stopwatch();
                    if (DisplayThread != null)
                    {
                        //终止线程 

                        cancellationTokenSource.Cancel();
                        Thread.Sleep(100);
                        DisplayThread = null;
                    }
                    cancellationTokenSource = new CancellationTokenSource();
                    DisplayThread = new Thread(() =>
                        {
                            DisplayThreadRunningFlag = true;
                            this.Invoke(() => { toolStripButtonPlay.Image = Resource.icons8_stop_64; });
                            var VideoFPS = capture.Get(VideoCaptureProperties.Fps);
                            var frameDelay = (int)(1000 / VideoFPS); // 计算每帧之间的延时时间，单位为毫秒

                            capture.Set(VideoCaptureProperties.PosFrames, dualTrackBar1.Value1);
                            for (int i = dualTrackBar1.Value1; i < dualTrackBar1.Value2; i++)
                            {
                                if (cancellationTokenSource.IsCancellationRequested)
                                {
                                    break;
                                }
                                sw.Restart();
                                ReadImageFromCapture();
                                pictureBox1.Invalidate();
                                sw.Stop();
                                int delay = frameDelay - (int)sw.ElapsedMilliseconds;
                                if (delay >= 0)
                                    Thread.Sleep(delay); // 每帧之间延时一定时间，以控制播放速度
                                Debug.WriteLine($"framedelay{frameDelay},elapsed{sw.ElapsedMilliseconds},delay{delay}");

                            }
                            DisplayThreadRunningFlag = false;
                            this.Invoke(() => { toolStripButtonPlay.Image = Resource.icons8_play_64; });

                        });
                    DisplayThread.Start();
                }
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            if (pictureBox1.Image == null)
            {
                pictureBox1.Image = CurrentImage;
                GC.Collect();
            }
            else
            {
                e.Graphics.DrawImage(CurrentImage, 0, 0);
            }
        }
    }
}

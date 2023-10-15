using DevExpress.Utils.Extensions;
using DevExpress.XtraDashboardLayout;
using Microsoft.VisualBasic;
using OpenCvSharp;
using OpenCvSharp.Aruco;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace VideoLabel
{
    //
    public partial class frmMain : Form
    {
        VideoProject vp = new VideoProject();
        VideoData CurrentVideoData;
        VideoLabel CurrentVideoLabel;
        System.Drawing.Image CurrentImage;
        object WriteImageLocker = new object();
        bool IsClosing = false;

        CancellationTokenSource ReadThreadToken;
        CancellationTokenSource DisplayThreadToken;

        private int FramePos = 0;
        private int FrameStep = 10;
        private int FrameCount = 10;

        private bool FramesInterConnect = false;
        public frmMain()
        {

            InitializeComponent();
            CurrentImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            //BufferImage = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            DisplayThreadToken = new CancellationTokenSource();
            Thread display = new Thread(() =>
            {
                Stopwatch sw = new Stopwatch();
                while (true)
                {
                    sw.Restart();
                    if (DisplayThreadToken.IsCancellationRequested)
                        break;
                    this.pictureBox1.Invalidate();
                    sw.Stop();
                    var delay = 40l - sw.ElapsedMilliseconds;
                    if (delay > 0)
                        Thread.Sleep((int)delay);
                }

            });
            display.Start();


        }
        private void frmMain_Load(object sender, EventArgs e)
        {
            //AddInVideoProject(new string[] { "rtsp://10.102.139.236:554/avstream/channel=1/stream=0.sdp", "rtsp://10.102.139.236:554/avstream/channel=1/stream=1.sdp" });            
        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            IsClosing = true;
            ReadThreadToken?.Cancel();
            DisplayThreadToken?.Cancel();
            Thread.Sleep(500);

        }
        public System.Drawing.Image ReadImageFromCapture(VideoCapture capture, int? pos = null)
        {

            System.Drawing.Image image = null;
            if (capture != null)
            {
                if (pos != null)
                    capture.Set(VideoCaptureProperties.PosFrames, pos.Value);
                using (var frame = new Mat())
                {
                    capture.Read(frame);
                    if (frame.Empty() == false)
                    {
                        using (var frame2 = Resize(frame, pictureBox1.Width, pictureBox1.Height))
                        {
                            image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame2);
                        }
                        //image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                    }
                }
            }
            GC.Collect();
            return image;
        }
        private OpenCvSharp.Mat Resize(Mat img_mat, int w_new, int h_new)
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
        private void AddLabel(string text)
        {
            var key = vp.Labels.AddOrGetKey<int, string>(text);
            System.Windows.Forms.RadioButton r = new System.Windows.Forms.RadioButton() { Text = text, Tag = key };
            flowLayoutPanel1.Controls.Add(r);
        }
        private void SetLabel(VideoProject vp)
        {
            flowLayoutPanel1.Controls.Clear();
            vp.Labels.ForEach(o =>
            {
                System.Windows.Forms.RadioButton r = new System.Windows.Forms.RadioButton() { Text = o.Value, Tag = o.Key };
                flowLayoutPanel1.Controls.Add(r);
            });
        }
        private (int?, string) GetLabel()
        {
            var r = flowLayoutPanel1.Controls.OfType<System.Windows.Forms.RadioButton>().Where(o => o.Checked).FirstOrDefault();
            if (r != null)
                return (r.Tag as int?, r.Text);
            else
                return (null, null);
        }
        private async void SetTableView(VideoData vd)
        {
            splitContainer3.Panel2.Controls.Clear();

            TableLayoutPanel tableLayoutPanel1 = new TableLayoutPanel();
            tableLayoutPanel1.Dock = DockStyle.Fill;
            splitContainer3.Panel2.Controls.Add(tableLayoutPanel1);


            const int ImageWidth = 300, ImageHeight = 200;

            if (vd == null || vd.frames.Count == 0)
            {
                tableLayoutPanel1.Controls.Clear();
                return;
            }
            //数据集数量
            var VideosCount = vd.frames.Count;

            //帧数量
            var FrameMax = vd.frames.Max(o => o.FrameCount);


            tableLayoutPanel1.AutoScroll = true;
            tableLayoutPanel1.RowStyles.Clear();
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowCount = VideosCount;
            tableLayoutPanel1.ColumnCount = 1;
            tableLayoutPanel1.Controls.Clear();

            int tablelayoutpanel_width = 0;
            int tablelayoutpanel_height = 0;
            // 添加新的行和列
            for (int i = 0; i < tableLayoutPanel1.RowCount; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, ImageHeight));
                tablelayoutpanel_height += ImageHeight;
            }
            for (int i = 0; i < tableLayoutPanel1.ColumnCount; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, ImageWidth * FrameMax));
                tablelayoutpanel_width += ImageWidth * FrameMax;
            }

            // 并行循环添加 Panel 控件和获取合并图像的任务
            for (int row = 0; row < tableLayoutPanel1.RowCount; row++)
            {
                var col = 0;
                System.Windows.Forms.Panel panel = new System.Windows.Forms.Panel();
                panel.Tag = (vd.frames[row], row);
                panel.Width = vd.frames[row].FrameCount * 300;
                panel.Height = 200;
                panel.TabStop = true;
                panel.BorderStyle = BorderStyle.FixedSingle;
                panel.BackColor = SystemColors.AppWorkspace;
                panel.MouseClick += (s, e) =>
                {
                    var ctrl = s as System.Windows.Forms.Panel; ;
                    var row = ctrl.Tag as (VideoLabel, int)?;
                    panel.Focus();
                    //Debug.WriteLine($"{row.Value.Item2}:panel.MouseClick ");
                };
                panel.GotFocus += (s, e) =>
                {
                    var ctrl = s as System.Windows.Forms.Panel; ;
                    var row = ctrl.Tag as (VideoLabel, int)?;
                    CurrentVideoLabel = row.Value.Item1;
                    panel.BackColor = System.Drawing.Color.Red;
                    //Debug.WriteLine($"{row.Value.Item2}:panel.GotFocus");
                };
                panel.LostFocus += (s, e) =>
                {
                    var ctrl = s as System.Windows.Forms.Panel; ;
                    var row = ctrl.Tag as (VideoLabel, int)?;
                    //CurrentVideoLabel = null;
                    panel.BackColor = SystemColors.AppWorkspace;
                    //Debug.WriteLine($"{row.Value.Item2}:panel.LostFocus");
                };
                // 在 UI 线程上添加 Panel 控件

                tableLayoutPanel1.Controls.Add(panel, col, row);


                var mergedImage = await Task.Run(() =>
                {
                    return GetMergedImage(vd, row, ImageWidth, ImageHeight);
                });
                panel.BackgroundImageLayout = ImageLayout.Stretch;
                panel.BackgroundImage = mergedImage;

            }
            CurrentVideoLabel = null;

        }

        private static List<Bitmap> GetVideoLabelImageList(string url, VideoLabel vl)
        {
            List<System.Drawing.Bitmap> imageList = new List<System.Drawing.Bitmap>();

            for (int i = 0; i < vl.FrameCount; i++)
            {
                var framepos = vl.FrameStart + i * vl.FrameStep;
                VideoCapture vc = new VideoCapture();
                vc.Open(url);
                vc.Set(VideoCaptureProperties.PosFrames, framepos);
                Mat mat = new Mat();
                vc.Read(mat);
                var image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);

                imageList.Add(image);
            }

            return imageList;

        }

        private static Bitmap GetMergedImage(VideoData vd, int row, int ImageWidth, int ImageHeight)
        {
            List<System.Drawing.Image> imageList = new List<System.Drawing.Image>();
            for (int i = 0; i < vd.frames[row].FrameCount; i++)
            {
                var framepos = vd.frames[row].FrameStart + i * vd.frames[row].FrameStep;
                VideoCapture vc = new VideoCapture();
                vc.Open(vd.VideoURL);
                vc.Set(VideoCaptureProperties.PosFrames, framepos);
                Mat mat = new Mat();
                vc.Read(mat);
                var image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(mat);
                using (Graphics g = Graphics.FromImage(image))
                {
                    //q:新建一个32像素的Font
                    Font font = new Font("Arial", 32);
                    g.DrawString(framepos.ToString(), font, System.Drawing.Brushes.Red, 0, 0);
                }
                imageList.Add(image);
            }

            // 计算拼接后的长图片的宽度和高度
            int margin = 2;
            int width_cell = ImageWidth;
            int height_cell = ImageHeight;
            int width = imageList.Count * width_cell;
            int height = height_cell;

            // 创建空白的长图片
            Bitmap mergedImage = new Bitmap(width, height);

            // 在长图片上绘制每一张小图片
            using (Graphics g = Graphics.FromImage(mergedImage))
            {
                int x = 0;
                foreach (var image in imageList)
                {
                    g.DrawImage(image, x + margin, margin, width_cell - margin * 2, height_cell - margin * 2);
                    x += width_cell;
                }
            }

            return mergedImage;
        }

        private void DualTrackBar1_OnValue1Changed(object? sender, EventArgs e)
        {
            //lock (WriteImageLocker)
            //{
            //CurrentImage = ReadImageFromCapture(CurrentVideoData.capture, dualTrackBar1.Value1);
            FramePos = dualTrackBar1.Value1;
            ChangedFramePos();
            //SwapImages(ref CurrentImage, ref BufferImage);
            toolStripTextBoxStart.Text = $"{dualTrackBar1.Value1}帧";
            //}
        }
        private void DualTrackBar1_OnValue2Changed(object? sender, EventArgs e)
        {
            //lock (WriteImageLocker)
            //{
            //CurrentImage = ReadImageFromCapture(CurrentVideoData.capture, dualTrackBar1.Value2);
            if (FramesInterConnect)
            {
                FramePos = dualTrackBar1.Value2 - FrameStep * FrameCount;
                ChangedFramePos();
            }
            //SwapImages(ref CurrentImage, ref BufferImage);
            //}

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
                        var vds = files.Select(o => new VideoData { VideoURL = o }).ToList();
                        AddInVideoProject(vds);

                    }
                    else if (File.Exists(file))
                    {
                        if (StaticLib.VideoFile.Contains(Path.GetExtension(file).ToLower()))
                        {
                            List<VideoData> vds = new List<VideoData>();
                            vds.Add(new VideoData { VideoURL = file });
                            AddInVideoProject(vds);
                        }
                    }
                });

            });
        }
        private void AddInVideoProject(List<VideoData> vdlist)
        {
            vdlist.ForEach(vd =>
            {
                var check = vp.VideoDatas.Where(o => o.VideoURL == vd.VideoURL && o.isURB == vd.isURB && o.isRTSP == vd.isRTSP).Any();
                if (check != true)
                    vp.VideoDatas.Add(vd);
            });
            if (CurrentVideoData == null)
                CurrentVideoData = vp.VideoDatas.LastOrDefault();

            RefreshUI(vp);

        }
        private VideoProject LoadProject()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "YAML 文件 (*.yaml)|*.yaml|所有文件 (*.*)|*.*";
            openFileDialog.DefaultExt = "*.yaml";
            openFileDialog.Title = "打开工程";
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var input = new StringReader(File.ReadAllText(openFileDialog.FileName));
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(NullNamingConvention.Instance)
                    .Build();
                var videoProject = deserializer.Deserialize<VideoProject>(input);
                return videoProject;
            }
            else
            {
                return null;
            }
        }
        private void SaveProject()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "YAML 文件 (*.yaml)|*.yaml|所有文件 (*.*)|*.*";
            saveFileDialog.Title = "保存工程";
            saveFileDialog.DefaultExt = ".yaml";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string yaml = GetProjectString();
                File.WriteAllText(saveFileDialog.FileName, yaml);
            }
        }
        private string GetProjectString()
        {
            var serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            //.WithTypeConverter(new AutoDictionaryConverter())
            .Build();
            var yaml = serializer.Serialize(vp);
            return yaml;
        }
        private void SetCurrentImage(VideoData vd)
        {
            if (vd != null && vd.VideoURL != null)
            {
                if (vd.isURB)
                {
                    vd.capture = new VideoCapture(int.Parse(vd.VideoURL));

                }
                else if (vd.isRTSP)
                {
                    vd.capture = new VideoCapture(vd.VideoURL);

                }
                else
                {
                    vd.capture = new VideoCapture(vd.VideoURL);
                }

                var FrameCount = vd.capture.Get(VideoCaptureProperties.FrameCount);
                var VideoFPS = vd.capture.Get(VideoCaptureProperties.Fps);
                Debug.WriteLine($"FrameCount:{FrameCount}  VideoFPS:{VideoFPS}");

                if (vd.isRTSP != true && vd.isURB != true)
                {
                    dualTrackBar1.Value1 = 0;
                    dualTrackBar1.Value2 = dualTrackBar1.Maximum = (int)FrameCount;
                }
                using (Mat frame = new Mat())
                {
                    vd.capture.Set(VideoCaptureProperties.PosFrames, 0);
                    //vd.capture.Read(frame);
                    lock (WriteImageLocker)
                    {
                        CurrentImage = ReadImageFromCapture(vd.capture);
                        //CurrentImage = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(frame);
                        //SwapImages(ref CurrentImage, ref BufferImage);
                    }
                }
            }

        }
        private void RefreshUI(VideoProject vp)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(() =>
                {
                    this.SuspendLayout();
                    listBox1.DisplayMember = null;
                    listBox1.DisplayMember = "VideoURL";
                    listBox1.DataSource = vp.VideoDatas;
                    SetLabel(vp);
                    SetCurrentImage(CurrentVideoData);
                    this.ResumeLayout();
                });
            }
            else
            {
                this.SuspendLayout();
                listBox1.DisplayMember = null;
                listBox1.DisplayMember = "VideoURL";
                listBox1.DataSource = vp.VideoDatas;
                SetLabel(vp);
                SetCurrentImage(CurrentVideoData);
                this.ResumeLayout();
            }

        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            lock (WriteImageLocker)
            {
                if (CurrentImage != null)
                {
                    e.Graphics.DrawImage(CurrentImage, 0, 0);
                    var msg = DateTime.Now.ToString("hh:mm:ss");
                    e.Graphics.DrawString(msg, new Font("宋体", 32), System.Drawing.Brushes.Red, 0, 0);
                }
            }
        }
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex != -1)
            {
                if (ReadThreadToken != null)
                {
                    ReadThreadToken.Cancel();
                }
                if (CurrentVideoData != null && CurrentVideoData.capture != null)
                {
                    if (CurrentVideoData.capture.IsOpened())
                    {
                        CurrentVideoData.capture.Release();
                    }
                }
                CurrentVideoData = listBox1.SelectedItem as VideoData;
                SetCurrentImage(CurrentVideoData);
                SetTableView(CurrentVideoData);
            }

        }
        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {

            if (ReadThreadToken != null)
            {
                ReadThreadToken.Cancel();
            }
            else
            {
                if (CurrentVideoData == null)
                    return;
                if (string.IsNullOrEmpty(CurrentVideoData.VideoURL))
                    return;
                if (CurrentVideoData.capture == null)
                    SetCurrentImage(CurrentVideoData);

                if (CurrentVideoData.capture.IsOpened())
                {


                    Stopwatch sw = new Stopwatch();
                    ReadThreadToken = new CancellationTokenSource();
                    var DisplayThread = new Thread(() =>
                        {

                            this.Invoke(() => { toolStripButtonPlay.Image = Resource.icons8_stop_64; });
                            var VideoFPS = CurrentVideoData.capture.Get(VideoCaptureProperties.Fps);
                            var frameDelay = 1000f / VideoFPS; // 计算每帧之间的延时时间，单位为毫秒
                            if (CurrentVideoData.isRTSP != true)
                                CurrentVideoData.capture.Set(VideoCaptureProperties.PosFrames, dualTrackBar1.Value1);

                            int pos_start = dualTrackBar1.Value1;
                            int pos_end = dualTrackBar1.Value2;
                            long currentTimestamp = 0;
                            long prevTimestamp = 0;
                            while (true)
                            {

                                sw.Restart();
                                if (ReadThreadToken.IsCancellationRequested)
                                {
                                    break;
                                }
                                if (this.IsDisposed)
                                {
                                    break;
                                }

                                lock (WriteImageLocker)
                                {
                                    CurrentImage = ReadImageFromCapture(CurrentVideoData.capture);
                                    //SwapImages(ref CurrentImage, ref BufferImage);
                                }

                                sw.Stop();

                                if (CurrentVideoData.isRTSP == true || CurrentVideoData.isURB == true)
                                {
                                    var delay = frameDelay - sw.ElapsedMilliseconds - 10;
                                    if (delay >= 0)
                                    {
                                        //Thread.Sleep(delay); // 每帧之间延时一定时间，以控制播放速度
                                        var sw2 = Stopwatch.StartNew();
                                        while (sw2.ElapsedMilliseconds < delay)
                                        {
                                            System.Threading.Thread.SpinWait(10); // 以较小的间隔快速检查时间是否到达
                                        }
                                    }
                                    Debug.WriteLine($"framedelay{frameDelay:F2},elapsed{sw.ElapsedMilliseconds:F2},delay{delay:F2}");


                                }
                                else
                                {
                                    var delay = frameDelay - sw.ElapsedMilliseconds;
                                    if (delay >= 0)
                                    {
                                        //Thread.Sleep(delay); // 每帧之间延时一定时间，以控制播放速度
                                        var sw2 = Stopwatch.StartNew();
                                        while (sw2.ElapsedMilliseconds < delay)
                                        {
                                            System.Threading.Thread.SpinWait(10); // 以较小的间隔快速检查时间是否到达
                                        }
                                    }
                                    Debug.WriteLine($"framedelay{frameDelay:F2},elapsed{sw.ElapsedMilliseconds:F2},delay{delay:F2}");
                                }

                                if (CurrentVideoData.isRTSP != true && CurrentVideoData.isURB != true)
                                {
                                    pos_start++;
                                    if (pos_start > pos_end)
                                        break;
                                }
                            }
                            //CurrentVideoData.capture.Release();
                            //CurrentVideoData.capture = null;
                            GC.Collect();

                            if (IsClosing == false && this.IsDisposed == false)
                            {
                                this.Invoke(() =>
                                {
                                    toolStripButtonPlay.Image = Resource.icons8_play_64;
                                });
                            }
                            ReadThreadToken = null;
                        });

                    DisplayThread.Start();

                }
            }
        }
        private void toolStripButtonMake_Click(object sender, EventArgs e)
        {
            VideoLabel vl = new VideoLabel();

            (var labelid, var label) = GetLabel();
            if (labelid != null)
            {
                vl.VideoLabelId = labelid.Value;
                vl.FrameStart = dualTrackBar1.Value1;
                vl.FrameStep = FrameStep;
                vl.FrameCount = FrameCount;
                CurrentVideoData.frames.Add(vl);
                SetTableView(CurrentVideoData);
            }

        }
        private void toolStripButtonDelete_Click(object sender, EventArgs e)
        {
            if (CurrentVideoLabel != null)
            {
                CurrentVideoData.frames.Remove(CurrentVideoLabel);
                CurrentVideoLabel = null;
                SetTableView(CurrentVideoData);
            }
        }
        private void toolStripButtonAddLabel_Click(object sender, EventArgs e)
        {
            frmInput input = new frmInput("添加标签", "请输入标签名：", "");
            if (input.ShowDialog() == DialogResult.OK && string.IsNullOrWhiteSpace(input.InputString) == false)
            {
                AddLabel(input.InputString);
            }
        }
        private void toolStripButtonRemoveLabel_Click(object sender, EventArgs e)
        {
            (var k, var s) = GetLabel();
            if (k != null)
            {
                vp.Labels.Remove(k.Value);
                SetLabel(vp);
            }
        }

        private void toolStripButtonNewProject_Click(object sender, EventArgs e)
        {
            if (vp.VideoDatas.Count > 0 || vp.Labels.Count > 0)
            {
                var r = MessageBox.Show($"工程未保存，是否保存工程？", "提示", MessageBoxButtons.YesNoCancel);
                if (r == DialogResult.Yes)
                {
                    SaveProject();
                }
                if (r != DialogResult.Cancel)
                {
                    vp = new VideoProject();
                }
            }
        }
        private void toolStripButtonOpenProject_Click(object sender, EventArgs e)
        {
            var vptemp = LoadProject();
            if (vptemp != null)
            {
                vp = vptemp;
                RefreshUI(vp);
            }


        }
        private void toolStripButtonSaveProject_Click(object sender, EventArgs e)
        {
            SaveProject();

        }
        private void toolStripButtonDataSet_Click(object sender, EventArgs e)
        {
            var s = GetProjectString();
            frmText frm = new frmText();
            frm.Text = "VideoProject数据集";
            frm.textBox1.Text = s;
            frm.ShowDialog();

        }
        private void toolStripButtonRTSP_Click(object sender, EventArgs e)
        {
            frmInput frm = new frmInput("", "请输入rtsp地址", "");
            var r = frm.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                List<VideoData> vds = new List<VideoData>();
                vds.Add(new VideoData { VideoURL = frm.InputString, isRTSP = true });
                AddInVideoProject(vds);
            }
        }
        private void toolStripButtonUSB_Click(object sender, EventArgs e)
        {
            frmInput frm = new frmInput("", "请输入本地摄像头编号", "0");
            frm.FormBorderStyle = FormBorderStyle.FixedSingle;
            var r = frm.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                var check = int.TryParse(frm.InputString, out int camearID);
                if (check == true)
                {
                    List<VideoData> vds = new List<VideoData>();
                    vds.Add(new VideoData { VideoURL = camearID.ToString(), isURB = true });
                    AddInVideoProject(vds);

                }


            }
        }

        private void toolStripButtonMakeDataSet_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new FolderBrowserDialog();
            var r = dialog.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                var basePath = dialog.SelectedPath;
                vp.Labels.ForEach(o =>
                {
                    var labelPath = Path.Combine(basePath, o.Value);
                    Directory.CreateDirectory(labelPath);
                });

                foreach (var vd in vp.VideoDatas)
                {
                    var filename = Path.GetFileNameWithoutExtension(vd.VideoURL);

                    foreach (var frame in vd.frames)
                    {
                        var label = vp.Labels.Where(o => o.Key == frame.VideoLabelId).Select(o => o.Value).FirstOrDefault();
                        var path = GetNewPathName(basePath, filename, label);
                        Directory.CreateDirectory(path);
                        var imagelist = GetVideoLabelImageList(vd.VideoURL, frame);
                        for (int i = 0; i < imagelist.Count; i++)
                        {
                            var imagefilename = Path.Combine(path, $"image{i:D3}.png");
                            imagelist[i].Save(imagefilename, ImageFormat.Png);
                        }

                    }
                }
            }

        }

        private string GetNewPathName(string basePath, string filename, string? label)
        {
            string PathName = null;
            var count_list = Enumerable.Range(0, 1000).Select(x => x.ToString("D3")).ToList();

            foreach (var count in count_list)
            {
                var base_label_image_path = Path.Join(basePath, label, filename, count);
                if (Directory.Exists(base_label_image_path) == false)
                {
                    PathName = base_label_image_path;
                    break;
                }
            }
            return PathName;
        }

        private void toolStripTextBoxStart_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SetPos();
            }
        }
        private void toolStripTextBoxStart_Validated(object sender, EventArgs e)
        {
            SetPos();
        }
        private void SetPos()
        {
            var text = toolStripTextBoxStart.Text;
            Regex regex = new Regex("([0-9]+)帧");
            var r = regex.Match(text);
            if (r.Success)
            {
                FramePos = int.Parse(r.Groups[1].Value);
                ChangedFramePos();
            }
        }




        private void ChangedFramePos()
        {
            if (CurrentVideoData != null)
            {
                dualTrackBar1.Value1 = FramePos;
                if (FramesInterConnect)
                    dualTrackBar1.Value2 = FramePos + FrameStep * FrameCount;
                //Debug.WriteLine($"pos_start{dualTrackBar1.Value1},pos_end{dualTrackBar1.Value2}");
                CurrentImage = ReadImageFromCapture(CurrentVideoData.capture, FramePos);
            }
        }
        private void toolStripTextBoxStep_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SetStep();
            }
        }
        private void toolStripTextBoxStep_Validated(object sender, EventArgs e)
        {
            SetStep();
        }



        private void SetStep()
        {
            var text = toolStripTextBoxStep.Text;
            Regex regex = new Regex("([0-9]+)帧");
            var r = regex.Match(text);
            if (r.Success)
            {
                var step = int.Parse(r.Groups[1].Value);
                FrameStep = step;
                ChangedFramePos();
            }
        }
        private void toolStripTextBoxCount_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                SetCount();
            }
        }
        private void toolStripTextBoxCount_Validated(object sender, EventArgs e)
        {
            SetCount();
        }



        private void SetCount()
        {
            var text = toolStripTextBoxCount.Text;
            Regex regex = new Regex("([0-9]+)帧");
            var r = regex.Match(text);
            if (r.Success)
            {
                var count = int.Parse(r.Groups[1].Value);
                FrameCount = count;
                ChangedFramePos();
            }
        }

        private void toolStripButton关联_Click(object sender, EventArgs e)
        {
            var ctrl = sender as ToolStripButton;
            FramesInterConnect = !FramesInterConnect;
            Debug.WriteLine($"FramesInterConnect:{FramesInterConnect}");
            if (FramesInterConnect)
            {
                ctrl.Image = Resource.icons8_link_64;
            }
            else
            {
                ctrl.Image = Resource.icons8_broken_link_64;
            }
        }
    }
}

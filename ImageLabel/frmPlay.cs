using Compunet.YoloV8;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Compunet.YoloV8.Data;

namespace ImageLabel
{
    public partial class frmPlay : Form
    {
        bool flagFormClosing = false;
        VideoCapture capture;
        YoloV8 yoloModel;
        Dictionary<int, string> yoloLabels;

        PictureBox PictureBox1 = null;
        PictureBox PictureBox2 = null;
        PictureBox PictureBox3 = null;
        PictureBox PictureBox4 = null;

        Threshold threshold = new Threshold();

        // 创建卡尔曼滤波器对象
        //KalmanFilter kf = new KalmanFilter(4, 4, 0);
        List<KeyValuePair<string, KalmanFilter>> ObjectKF = new List<KeyValuePair<string, KalmanFilter>>();

        public frmPlay(YoloV8 yolo, Dictionary<int, string> listLabel)
        {
            InitializeComponent();
            //InitKF(kf);
            yoloModel = yolo;
            yoloLabels = listLabel;
            capture = new VideoCapture();
        }
        private void frmPlay_Load(object sender, EventArgs e)
        {
            toolStripComboBox2.SelectedIndex = 1;
            toolStripComboBoxWindows.SelectedIndex = 4;

            Task.Run(() =>
            {
                var filename = "Threshold.yaml";
                if (File.Exists(filename) == false)
                {
                    threshold.移动阈值宽度 = 20;
                    threshold.移动阈值高度 = 20;
                    threshold.放大阈值宽度 = 110;
                    threshold.放大阈值高度 = 110;
                    threshold.缩小阈值宽度 = 90;
                    threshold.缩小阈值高度 = 90;
                    SaveThreshold(filename);
                }
                {
                    LoadThreshold(filename);
                }
            });


        }

        private void LoadThreshold(string filename)
        {
            var yaml = File.ReadAllText(filename);
            var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            threshold = deserializer.Deserialize<Threshold>(yaml);
        }

        private void SaveThreshold(string filename)
        {
            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(threshold);
            File.WriteAllText(filename, yaml);
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            flagFormClosing = true;
            backgroundWorker1.CancelAsync();
            capture.Dispose();
        }

        private List<ToolStripMenuItem> initMenu()
        {
            return new List<ToolStripMenuItem> { 摄像头ToolStripMenuItem, 视频文件ToolStripMenuItem, 图片文件ToolStripMenuItem };
        }
        private void EnableAllMenu()
        {
            var menus = initMenu();
            menus.ForEach(o => { o.Enabled = true; });
        }
        private void DisableMenu(ToolStripMenuItem item)
        {
            var menus = initMenu();
            menus.ForEach(o =>
            {
                if (o.Equals(item))
                    o.Enabled = true;
                else
                    o.Enabled = false;
            });

        }
        private void 开始拍摄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DisableMenu(摄像头ToolStripMenuItem);
            if (capture.IsOpened())
            {
                backgroundWorker1.CancelAsync();
            }
            capture.Open(0, VideoCaptureAPIs.ANY);
            backgroundWorker1.RunWorkerAsync();

        }

        private void 结束拍摄ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableAllMenu();
            backgroundWorker1.CancelAsync();


        }

        private async void 打开文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Video Files|*.mp4;*.avi|All Files|*.*";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DisableMenu(视频文件ToolStripMenuItem);
                if (capture.IsOpened())
                {
                    backgroundWorker2.CancelAsync();
                }
                capture.Open(dialog.FileName);
                backgroundWorker2.RunWorkerAsync(dialog.FileName);
            }
        }

        private void 停止视频ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EnableAllMenu();
            backgroundWorker2.CancelAsync();

        }

        private void backgroundWorker1_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            var bgWorker = (BackgroundWorker)sender;
            //capture.Open(0, VideoCaptureAPIs.ANY);
            RunCaptureVideo(bgWorker);
            //capture.Release();
        }

        private void backgroundWorker1_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            try
            {
                if (flagFormClosing == false)
                {
                    var rd = e.UserState as ReportData;
                    var image = rd.ImageDetect;
                    image.Mutate(o => { o.Resize(PictureBox1.ClientSize.Width, PictureBox1.ClientSize.Height); });
                    var bitmap=image.ToArray().ToNetImage();
                    PictureBox1.Image = bitmap;
                    if (toolStripComboBox2.SelectedIndex == 1)
                        高阶显示(rd);
                    //img_mat.Dispose();
                    //img_bitmap = null;
                }
            }
            catch
            {

            }
        }
        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (flagFormClosing == false)
            {
                capture.Release();
                EnableAllMenu();
            }
        }
        private void 高阶显示(ReportData rd)
        {
            var img_source = rd.ImageDetect;

            //var Train = rd.predictionModels.Where(o => o.LabelId == 81).OrderByDescending(o => o.Score).FirstOrDefault();
            //var ctrl1 = PictureBox2;
            //if (Train != null && ctrl1 != null)
            //{
            //    if (ctrl1 != null)
            //    {
            //        var (cx, cy) = (Train.Rectangle.X + Train.Rectangle.Width / 2, Train.Rectangle.Y + Train.Rectangle.Height / 2);
            //        var mat_train = img_source.GetRectSubPix(new OpenCvSharp.Size(Train.Rectangle.Width, Train.Rectangle.Height), new Point2f(cx, cy));
            //        mat_train = ImageSharpExtensions.Resize(mat_train, ctrl1.ClientSize.Width, ctrl1.ClientSize.Height);
            //        var bitmap_train = mat_train.ToBitmap();
            //        this.Invoke(() => { PutImageToPictureBox(ctrl1, bitmap_train); });
            //        mat_train.Dispose();
            //        bitmap_train = null;
            //    }
            //}



            //var Driver = rd.predictionModels.Where(o => o.LabelId == 83).OrderByDescending(o => o.Score).FirstOrDefault();
            //var ctrl2 = PictureBox3;
            //if (Driver != null && ctrl2 != null)
            //{
            //    if (ctrl2 != null)
            //    {
            //        var (cx, cy) = (Driver.Rectangle.X + Driver.Rectangle.Width / 2, Driver.Rectangle.Y + Driver.Rectangle.Height / 2);
            //        var mat_driver = img_source.GetRectSubPix(new OpenCvSharp.Size(Driver.Rectangle.Width, Driver.Rectangle.Height), new Point2f(cx, cy));
            //        mat_driver = ImageSharpExtensions.Resize(mat_driver, ctrl2.ClientSize.Width, ctrl2.ClientSize.Height);
            //        var bitmap_driver = mat_driver.ToBitmap();
            //        this.Invoke(() => { PutImageToPictureBox(ctrl2, bitmap_driver); });
            //        mat_driver.Dispose();
            //        bitmap_driver = null;
            //    }
            //}

        }


        private void PutImageToPictureBox(PictureBox pb, System.Drawing.Image img)
        {
            //if (pb.Image == null)
            //    pb.Image = new Bitmap(img.Width, img.Height);
            //using var g = Graphics.FromImage(pb.Image);
            //g.DrawImage(img, 0, 0);
            pb.Image = img;

        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgWorker = (BackgroundWorker)sender;
            var filename = e.Argument as string;
            //capture.Open(filename);
            RunCaptureVideo(bgWorker);
            //capture.Release();
        }

        private void backgroundWorker2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            try
            {
                if (flagFormClosing == false)
                {
                    var rd = e.UserState as ReportData;
                    var image = rd.ImageDetect;
                    image.Mutate(o => { o.Resize(PictureBox1.ClientSize.Width, PictureBox1.ClientSize.Height); });
                    var bitmap = image.ToArray().ToNetImage();
                    PictureBox1.Image = bitmap;
                    if (toolStripComboBox2.SelectedIndex == 1)
                        高阶显示(rd);
                    //img_mat.Dispose();
                    //img_bitmap = null;
                }
            }
            catch
            {

            }
        }

        private void backgroundWorker2_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (flagFormClosing == false)
            {
                capture.Release();
                EnableAllMenu();
            }
        }


        private void 打开图片ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "All Image Files|*.bmp;*.ico;*.gif;*.jpeg;*.jpg;*.png;*.tif;*.tiff|All Files|*.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var fs = File.Open(dialog.FileName, FileMode.Open);
                var image = SixLabors.ImageSharp.Image.Load(fs);

                var predictions = yoloModel.Detect(image);
                var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.DashDot(SixLabors.ImageSharp.Color.Yellow, 3);
                var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 16);
                foreach (var box in predictions.Boxes)
                {
                    double score = Math.Round(box.Confidence, 2);

                    // 在图像上绘制矩形
                    image.Mutate(ctx =>
                    {
                        ctx.Draw(pen, new SixLabors.ImageSharp.Rectangle(box.Bounds.X, box.Bounds.Y, box.Bounds.Width, box.Bounds.Height));
                    });

                    var (x, y) = (box.Bounds.X - 3, box.Bounds.Y - 23);
                    var msg = $"{box.Class.Id} ({score})";

                    // 在图像上绘制文本
                    image.Mutate(ctx =>
                    {
                        ctx.DrawText(msg, font,
                        SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.Point(x, y));
                    });


                }


                //高阶显示(img_bitmap);
                image.Dispose();
            }
        }

        private Bitmap 生成并绘画轮廓(Mat mat输入)
        {
            var mat输出 = new Mat(new OpenCvSharp.Size(mat输入.Width, mat输入.Height), MatType.CV_8UC1);

            Cv2.FindContours(mat输入,
                             out OpenCvSharp.Point[][] contours,
                             out HierarchyIndex[] outputArray,
                             RetrievalModes.External,
                             ContourApproximationModes.ApproxTC89KCOS);

            var list_contours = contours.OrderByDescending(o => o.Length).Take(10).ToList();


            for (int i = 0; i < list_contours.Count; i++)
            {
                OpenCvSharp.Scalar color = OpenCvSharp.Scalar.RandomColor();
                if (list_contours[i].Length > 100)
                {
                    Cv2.DrawContours(
                        mat输出,
                        list_contours,
                        contourIdx: i,
                        color: color,
                        thickness: 2,
                        lineType: LineTypes.Link8,
                        hierarchy: outputArray,
                        maxLevel: 0);
                }
            }
            var bitmap输出 = mat输出.ToBitmap();


            return bitmap输出;
        }

        private void toolStripComboBoxWindows_SelectedIndexChanged(object sender, EventArgs e)
        {
            var table = tableLayoutPanel1;
            table.Controls.Clear();
            table.RowStyles.Clear();
            table.ColumnStyles.Clear();
            switch (toolStripComboBoxWindows.SelectedIndex)
            {
                case 0:
                    table.RowCount = 1;
                    table.ColumnCount = 1;
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.RowStyles.Add(new RowStyle());

                    PictureBox1 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox1, 0, 0);

                    break;
                case 1:
                    table.RowCount = 1;
                    table.ColumnCount = 2;
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50f);
                    table.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50f);

                    PictureBox1 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox1, 0, 0);
                    PictureBox2 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox2, 1, 0);

                    break;
                case 2:
                    table.RowCount = 2;
                    table.ColumnCount = 1;
                    table.RowStyles.Add(new RowStyle());
                    table.RowStyles.Add(new RowStyle());
                    table.RowStyles[0] = new RowStyle(SizeType.Percent, 50f);
                    table.RowStyles[1] = new RowStyle(SizeType.Percent, 50f);

                    PictureBox1 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox1, 0, 0);
                    PictureBox2 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox2, 0, 1);


                    break;
                case 3:
                    table.RowCount = 2;
                    table.ColumnCount = 2;
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.RowStyles.Add(new RowStyle());
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.RowStyles.Add(new RowStyle());

                    table.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 50f);
                    table.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 50f);
                    table.RowStyles[0] = new RowStyle(SizeType.Percent, 50f);
                    table.RowStyles[1] = new RowStyle(SizeType.Percent, 50f);

                    PictureBox1 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox1, 0, 0);
                    PictureBox2 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox2, 1, 0);
                    PictureBox3 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox3, 0, 1);
                    PictureBox4 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox4, 1, 1);
                    break;
                case 4:
                    table.RowCount = 1;
                    table.ColumnCount = 4;
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles.Add(new ColumnStyle());
                    table.ColumnStyles[0] = new ColumnStyle(SizeType.Percent, 25f);
                    table.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 25f);
                    table.ColumnStyles[2] = new ColumnStyle(SizeType.Percent, 25f);
                    table.ColumnStyles[3] = new ColumnStyle(SizeType.Percent, 25f);

                    PictureBox1 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox1, 0, 0);
                    PictureBox2 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox2, 1, 0);
                    PictureBox3 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox3, 2, 0);
                    PictureBox4 = new PictureBox() { Dock = DockStyle.Fill };
                    table.Controls.Add(PictureBox4, 3, 0);
                    break;


            }

            GC.Collect();

        }

        //public void Test()
        //{




        //    InitKF(kf);

        //    while (true)
        //    {
        //        // 定义边界框参数
        //        float x = 100;
        //        float y = 200;
        //        float w = 50;
        //        float h = 75;

        //        // 初始化状态向量和控制向量
        //        Filter(ref x, ref y, ref w, ref h);

        //        Console.WriteLine("Prediction: ({0}, {1}, {2}, {3})", x, y, w, h);
        //    }

        //}

        private void Filter(string name, ref float x, ref float y, ref float w, ref float h)
        {
            Mat state = new Mat(4, 1, MatType.CV_32F, new float[] { (float)x, (float)y, (float)w, (float)h });
            var kf = ObjectKF.Find(o => o.Key == name);
            if (kf.Key == null)
            {
                kf = new KeyValuePair<string, KalmanFilter>(name, new KalmanFilter(4, 4, 0));
                ObjectKF.Add(kf);
                InitKF(kf.Value);

            }
            kf.Value.Correct(state);
            // 使用卡尔曼滤波器对边界框进行平滑过滤
            Mat prediction = kf.Value.Predict();
            x = prediction.At<float>(0, 0);
            y = prediction.At<float>(1, 0);
            w = prediction.At<float>(2, 0);
            h = prediction.At<float>(3, 0);
        }

        //private RectangleF Filter(string name, RectangleF r)
        //{
        //    (var x, var y, var w, var h) = ((float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
        //    Filter(name, ref x, ref y, ref w, ref h);
        //    return new RectangleF(x, y, w, h);


        //}

        private static void InitKF(KalmanFilter kf)
        {
            // 初始化卡尔曼滤波器
            kf.TransitionMatrix = new Mat(4, 4, MatType.CV_32F, new float[] {
                                        1, 0, 1, 0,
                                        0, 1, 0, 1,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1
                                    });
            kf.MeasurementMatrix = new Mat(4, 4, MatType.CV_32F, new float[] {
                                        1, 0, 1, 0,
                                        0, 1, 0, 1,
                                        0, 0, 1, 0,
                                        0, 0, 0, 1
                                    });
            kf.MeasurementNoiseCov = Mat.Eye(4, 4, MatType.CV_32F);
            kf.ProcessNoiseCov = Mat.Eye(4, 4, MatType.CV_32F);
        }
        private void RunCaptureVideo(BackgroundWorker bgWorker)
        {
            var ShowMsg = "";
            var FrameCount = capture.Get(VideoCaptureProperties.FrameCount);
            var VideoFPS = capture.Get(VideoCaptureProperties.Fps);
            var Font = new System.Drawing.Font("Consolas", 32, GraphicsUnit.Pixel);
            var Frames = 0f;
            var StartVideoDateTime = DateTime.Now;

            //FPS f = new FPS();
            //Track track = new Track(threshold);
            Stopwatch watch_train = new Stopwatch();
            bool flag_train = false;
            //track.On出现 = (o) =>
            //{
            //    var labelname = yoloLabels.Where(p => p.Key == o.Current_Prediction.LabelId).Select(o => o.Value).FirstOrDefault();
            //    Debug.WriteLine($"{o.Current_FrameSecond:N2}:出现[{labelname}]");

            //    //if (labelname == "SubwayTrain")
            //    //{
            //    //    TrainWatch.Restart();
            //    //}
            //};

            //track.On消失 = (o) =>
            //{
            //    var labelname = yoloLabels.Where(p => p.Key == o.Last_Prediction.LabelId).Select(o => o.Value).FirstOrDefault();
            //    Debug.WriteLine($"{o.Current_FrameSecond:N2}:消失[{labelname}]");
            //    if (labelname == "SubwayTrain")
            //    {
            //        if (flag_train == true)
            //        {
            //            watch_train.Stop();
            //            flag_train = false;
            //            Debug.WriteLine($"{o.Current_FrameSecond:N2}:列车消失，计时清零。");
            //        }
            //    }
            //};

            //track.On移动 = (o) =>
            //{
            //    var labelname = yoloLabels.Where(p => p.Key == o.Last_Prediction.LabelId).Select(o => o.Value).FirstOrDefault();
            //    Debug.WriteLine($"{o.Current_FrameSecond:N2}:移动[{labelname}]");
            //    if (labelname == "SubwayTrain")
            //    {
            //        if (flag_train == true)
            //        {
            //            watch_train.Stop();
            //            //flag_train = false;
            //            Debug.WriteLine($"{o.Current_FrameSecond:N2}:列车再次移动，计时停止。");
            //        }
            //    }
            //};

            //track.On停止 = (o) =>
            //{
            //    var labelname = yoloLabels.Where(p => p.Key== o.Last_Prediction.LabelId).Select(o => o.Value).FirstOrDefault();
            //    Debug.WriteLine($"{o.Current_FrameSecond:N2}:停止[{labelname}]");
            //    if (labelname == "SubwayTrain")
            //    {
            //        if (flag_train == false)
            //        {
            //            watch_train.Restart();
            //            flag_train = true;
            //            Debug.WriteLine($"{o.Current_FrameSecond:N2}:发现停车，开始计时。");
            //        }
            //    }
            //};

            try
            {
                while (!bgWorker.CancellationPending)
                {
                    Stopwatch fpsStopWatch = new Stopwatch();
                    fpsStopWatch.Start();
                    var img_mat = capture.RetrieveMat();
                    var image = img_mat.ToBytes().ToImage();
                    var image_clone = image.Clone(null);
                    //var img_source = img_mat.Clone();
                    var detect = yoloModel.Detect(image);

                    var FrameSecond = Frames / VideoFPS;
                    //track.Update(FrameSecond, predictions);                


                    var font = SixLabors.Fonts.SystemFonts.CreateFont("Arial", 16);
                    foreach (var box in detect.Boxes) // iterate predictions to draw results
                    {
                        var labelcolor = lib.Colors[box.Class.Id % 32];
                        var color = SixLabors.ImageSharp.Color.FromRgb(labelcolor.R, labelcolor.G, labelcolor.B);
                        var pen = SixLabors.ImageSharp.Drawing.Processing.Pens.DashDot(color, 3);
                        var labelname = yoloLabels[box.Class.Id];
                        var score = Math.Round(box.Confidence, 2);

                        var rect = new SixLabors.ImageSharp.Rectangle(box.Bounds.X, box.Bounds.Y, box.Bounds.Width, box.Bounds.Height);
                        (var cx, var cy) = (box.Bounds.X + box.Bounds.Width / 2, box.Bounds.Y + box.Bounds.Height / 2);
                        var circle = new SixLabors.ImageSharp.Drawing.EllipsePolygon(cx, cy, 2);

                        var msg = $"{box.Class.Id}:{labelname} ({score})";
                        var text_point = new SixLabors.ImageSharp.Point(box.Bounds.X, box.Bounds.Y);
                        image.Mutate(ctx =>
                        {
                            ctx.Draw(pen, rect);
                            ctx.Draw(pen, circle);
                            ctx.DrawText(msg, font, color, text_point);

                        });

                    }




                    #region 显示列车出现的秒数
                    var msg_TrainSecond = $"Time In Station:{watch_train.ElapsedMilliseconds / 1000f:N2}";
                    image.Mutate(ctx => { ctx.DrawText(msg_TrainSecond, font, SixLabors.ImageSharp.Color.Black, new SixLabors.ImageSharp.Point(0, 0)); });

                    #endregion



                    bgWorker.ReportProgress(0, new ReportData() { ImageSource = image_clone, ImageDetect = image, DetectResult = detect });

                    fpsStopWatch.Stop();

                    #region 显示视频帧率
                    var seconds = fpsStopWatch.ElapsedMilliseconds / 1000f;
                    var fps = 1f / seconds;
                    var JumpFrames = (float)VideoFPS * seconds;
                    Frames += JumpFrames + 1;
                    ShowMsg = $"TotalTime:{(DateTime.Now - StartVideoDateTime).TotalSeconds}s\nTime/Frame:{fpsStopWatch.ElapsedMilliseconds}ms\nFPS:{fps}\nJumpFrames:{JumpFrames}+1\nSetFrames:{Frames}/{FrameCount}";
                    //Debug.WriteLine(ShowMsg);
                    lib.PutText(img_mat, ShowMsg, 0, 34);
                    #endregion
                    if (FrameCount != -1)
                    {

                        if (Frames >= FrameCount)
                            break;
                        capture.Set(VideoCaptureProperties.PosFrames, Frames);
                    }
                    //img_mat.Dispose();
                    //img_source.Dispose();
                }
            }
            catch
            {

            }
            //capture.Dispose();

        }

        private void 运动阈值ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frm设置运动阈值 frm = new frm设置运动阈值(threshold);
            var r = frm.ShowDialog();
            if (r == DialogResult.OK)
            {
                threshold = frm.threshold;
                var filename = "Threshold.yaml";
                SaveThreshold(filename);
            }
        }

        private void 保存视频ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }

    public class ReportData
    {
        public SixLabors.ImageSharp.Image ImageSource;
        public SixLabors.ImageSharp.Image ImageDetect;
        public IDetectionResult DetectResult;

    }




}
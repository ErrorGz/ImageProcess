using Compunet.YoloV8;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Diagnostics;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Text;


internal class Program
{
    static void Main(string[] args)
    {

        string url = @"I:\BaiduSyncdisk\PY\DataSet\视频\9月14日.mp4";
        //var url = @"rtsp://guest:123456@10.103.8.236:554/avstream/channel=1/stream=1.sdp";
        string model = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath), @".\onnx\yolov8n-pose.onnx");
        RectangleF people_rect_float = new RectangleF() { X = 230f / 962, Y = 95f / 752, Width = 300f / 962, Height = 600f / 752 };
        RectangleF train_rect_float = new RectangleF() { X = 190f / 962, Y = 0f, Width = 470f / 962, Height = 630f / 752 };


        foreach (string arg in args)
        {
            if (arg.StartsWith("rtsp://"))
            {
                url = arg;
            }
            else
            {
                FileInfo fi = new FileInfo(arg);
                if (fi.Exists)
                {
                    if (fi.Extension == ".mp4")
                    {
                        url = fi.FullName;
                    }
                    if (fi.Extension == ".onnx")
                    {
                        model = fi.FullName;
                    }
                }
            }
        }
        Console.WriteLine($"ImageDetect Source:{url}");
        Console.WriteLine($"Model File:{model}");

        ImageDetect id = new ImageDetect();
        id.Run(url, model, people_rect_float, train_rect_float);

    }
}
public class ImageDetect
{


    bool ExitFlag = false;
    bool pause = true;
    bool advance = false;
    bool reverse = false;
    bool live = false;
    bool record = false;
    bool recording = false;
    bool recapture = false;


    double[] scale = { 1.0, 0.75, 0.5, 0.25 };
    int scaleIndex = 0;
    double fps = 30;
    string fps_text = "";

    Color[] colors = {
    Color.AliceBlue,
    Color.AntiqueWhite,
    Color.Aqua,
    Color.Aquamarine,
    Color.Azure,
    Color.Beige,
    Color.Bisque,
    Color.Black,
    Color.BlanchedAlmond,
    Color.Blue,
    Color.BlueViolet,
    Color.Brown,
    Color.BurlyWood,
    Color.CadetBlue,
    Color.Chartreuse,
    Color.Chocolate,
    Color.Coral,
    Color.CornflowerBlue,
    Color.Cornsilk,
    Color.Crimson,
    Color.Cyan,
    Color.DarkBlue,
    Color.DarkCyan,
    Color.DarkGoldenrod,
    Color.DarkGray,
    Color.DarkGreen,
    Color.DarkKhaki,
    Color.DarkMagenta,
    Color.DarkOliveGreen,
    Color.DarkOrange,
    Color.DarkOrchid,
    Color.DarkRed,
    Color.DarkSalmon,
    Color.DarkSeaGreen,
    Color.DarkSlateBlue,
    Color.DarkSlateGray,
    Color.DarkTurquoise,
    Color.DarkViolet,
    Color.DeepPink,
    Color.DeepSkyBlue,
    Color.DimGray,
    Color.DodgerBlue,
    Color.Firebrick,
    Color.FloralWhite,
    Color.ForestGreen,
    Color.Fuchsia,
    Color.Gainsboro,
    Color.GhostWhite,
    Color.Gold,
    Color.Goldenrod,
    Color.Gray,
    Color.Green,
    Color.GreenYellow,
    Color.Honeydew,
    Color.HotPink,
    Color.IndianRed,
    Color.Indigo,
    Color.Ivory,
    Color.Khaki,
    Color.Lavender,
    Color.LavenderBlush,
    Color.LawnGreen,
    Color.LemonChiffon,
    Color.LightBlue,
    Color.LightCoral,
    Color.LightCyan,
    Color.LightGoldenrodYellow,
    Color.LightGray,
    Color.LightGreen,
    Color.LightPink,
    Color.LightSalmon,
    Color.LightSeaGreen,
    Color.LightSkyBlue,
    Color.LightSlateGray,
    Color.LightSteelBlue,
    Color.LightYellow,
    Color.Lime,
    Color.LimeGreen,
    Color.Linen,
    Color.Magenta,
    Color.Maroon,
    Color.MediumAquamarine,
    Color.MediumBlue,
    Color.MediumOrchid,
    Color.MediumPurple,
    Color.MediumSeaGreen,
    Color.MediumSlateBlue,
    Color.MediumSpringGreen,
    Color.MediumTurquoise,
    Color.MediumVioletRed,
    Color.MidnightBlue,
    Color.MintCream,
    Color.MistyRose,
    Color.Moccasin,
    Color.NavajoWhite,
    Color.Navy,
};



    void CaptureThread(object data)
    {
        double currentTime = 0;
        double skipInterval = 10000;
        (var source, var ds) = ((string, ThreadSafeDataSet))data;
        VideoCapture cap = new VideoCapture(source);

        fps = cap.Get(VideoCaptureProperties.Fps);
        var width = cap.Get(VideoCaptureProperties.FrameWidth);
        var height = cap.Get(VideoCaptureProperties.FrameHeight);

        //Debug.WriteLine($"fps:{fps}");
        //if (source.StartsWith("rtsp://"))
        //{
        //    fps = 35;
        //    Debug.WriteLine($"更改fps:{fps}");//对于这里我设置35，才能跟上视频流的速度，未明白为什么
        //}

        var ms = 1000 / (fps + 10);
        Mat mat_temp = new Mat();
        VideoWriter video_writer = null;

        Stopwatch swCapture = new Stopwatch();
        while (!ExitFlag)
        {
            if (pause)
                continue;

            swCapture.Restart();
            DataSet d = new DataSet();
            if (cap.Read(d.Source))
            {
                if (scaleIndex != 0)
                    Cv2.Resize(d.Source, d.Source, new OpenCvSharp.Size(width * scale[scaleIndex], height * scale[scaleIndex]));
                ds.AddDataSet(d);

                if (record)
                {
                    record = false;
                    if (recording == false || video_writer == null)
                    {
                        recording = true;
                        var filename = NewFileName(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath), "./Record"), "Video_", ".mp4");
                        if (filename != null)
                            video_writer = new VideoWriter(filename, FourCC.H264, fps, new OpenCvSharp.Size(width, height));
                    }
                    else
                    {
                        recording = false;
                        video_writer?.Release();
                        video_writer = null;
                    }

                }

                if (recording)
                {
                    video_writer.Write(d.Source);
                }

            }
            else
            {
                Console.WriteLine($"读取失败！");
                recapture = true;
            }
            if (recapture)
            {
                recapture = false;
                cap.Release();
                cap = new VideoCapture(source);
                fps = cap.Get(VideoCaptureProperties.Fps);
                width = cap.Get(VideoCaptureProperties.FrameWidth);
                height = cap.Get(VideoCaptureProperties.FrameHeight);
                Console.WriteLine($"重新读取视频");
            }


            while (ds.Count > 100)
                ds.RemoveDataSet(ds.First());

            if (advance)
            {
                // 前进10秒操作
                advance = false;
                // 计算目标时间位置
                double targetTime = currentTime + skipInterval;
                Console.WriteLine($"前进10秒，上前时间位置:{currentTime}  目标时间位置:{targetTime}");
                // 调整视频的时间位置
                cap.Set(VideoCaptureProperties.PosMsec, targetTime);
                // 更新当前时间位置
                currentTime = targetTime;

            }
            else if (reverse)
            {
                // 后退10秒操作
                reverse = false;
                // 计算目标时间位置
                double targetTime = currentTime - skipInterval;
                Console.WriteLine($"回退10秒，上前时间位置:{currentTime}  目标时间位置:{targetTime}");
                // 调整视频的时间位置
                cap.Set(VideoCaptureProperties.PosMsec, targetTime);
                // 更新当前时间位置
                currentTime = targetTime;

            }
            else
            {
                currentTime = cap.Get(VideoCaptureProperties.PosMsec);
            }

            swCapture.Stop();
            int delay = (int)(0.5f + ms - swCapture.ElapsedMilliseconds);
            //Debug.WriteLine($"delay{delay}={ms}-{swCapture.ElapsedMilliseconds}");
            if (delay > 0)
            {
                //Console.WriteLine($"延时：{delay}ms");
                Thread.Sleep(delay);

                //Stopwatch stopwatch = Stopwatch.StartNew();
                //while(stopwatch.ElapsedMilliseconds<delay)
                //{
                //    Thread.Sleep(1);
                //}                
            }
            else
            {
                var skip_frames = (int)(Math.Abs(delay / 1000) * fps) + 1;
                //Console.WriteLine($"录制跳帧：{skip_frames}帧");
                while (skip_frames-- > 0)
                    cap.Read(mat_temp);
            }
        }



    }

    (string, int, int) getDataSetInfo(ThreadSafeDataSet ds)
    {
        int count = 0;
        StringBuilder sbPrintChat = new StringBuilder();
        sbPrintChat.Append('[');
        var ds_list = ds.GetAllDataSets().ToArray();
        for (int i = 0; i < ds_list.Length; i++)
        {
            if (i != 0 && i != ds_list.Length - 1 && i % 10 == 0)
                sbPrintChat.Append("][");
            if (ds_list[i].Detect != null)
            {
                sbPrintChat.Append('#');
                count++;
            }
            else
            {
                //sbPrintChat.Append((char)('0' + i % 10));
                sbPrintChat.Append('-');
            }
        }



        sbPrintChat.Append(']');
        var strDataSetInfo = sbPrintChat.ToString();
        return (strDataSetInfo, ds.Count, count);
    }


    void DetectThread(object data)
    {
        (var model, var ds, var people_rect_float, var train_rect_float) = ((string, ThreadSafeDataSet, RectangleF, RectangleF))data;
        Console.WriteLine($"加载模型中...");
        SessionOptions so = new SessionOptions();
        so.EnableProfiling = true;
        //so.AppendExecutionProvider_DML();
        var predictor = new YoloV8(model, so);

        Console.WriteLine($"加载完毕！");
        pause = false;

        FontCollection collection = new();
        collection.AddCollection(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath), "./MSYH.TTC"));
        collection.TryGet("Microsoft YaHei", out var family);
        var font = family.CreateFont(18, FontStyle.Italic);
        var brush_white = Brushes.Solid(Color.White);
        var bursh_OrangeRed = Pens.DashDot(Color.OrangeRed, 1);
        var bursh_LightYellow = Pens.DashDot(Color.LightYellow, 1);
        while (!ExitFlag)
        {
            var d = ds.LastNoneDetectDataSet();
            if (d != null)
            {
                Image<Rgb24> image = ImageLib.MatToImage(d.Source);
                var result = predictor.Pose(image);
                //var detect = result.PlotImage(image);
                var detect = image.Clone();
                var people_rect_int = new Rectangle((int)(people_rect_float.X * detect.Width), (int)(people_rect_float.Y * detect.Height), (int)(people_rect_float.Width * detect.Width), (int)(people_rect_float.Height * detect.Height));
                var train_rect_int = new Rectangle((int)(train_rect_float.X * detect.Width), (int)(train_rect_float.Y * detect.Height), (int)(train_rect_float.Width * detect.Width), (int)(train_rect_float.Height * detect.Height));
                var peopleBox = result.Boxes.Where(o => people_rect_int.Contains(o.Bounds) && (o.Bounds.Width > 0.5 * people_rect_int.Width || o.Bounds.Height > 0.5 * people_rect_int.Height)).OrderByDescending(o => o.Bounds.Width * o.Bounds.Height).FirstOrDefault();


                detect.Mutate(o =>
                {
                    o.Draw(bursh_LightYellow, people_rect_int);
                    o.Draw(bursh_OrangeRed, train_rect_int);
                    o.DrawText(fps_text, font, brush_white, new PointF(0, 0));
                    o.DrawText(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), font, brush_white, new PointF(0, detect.Height - 60));

                    o.DrawText("A = 退回10秒, D = 前进10秒, L = 实时/检测，R = 录制/不录制", font, brush_white, new PointF(100, 0));

                    if (recording)
                    {
                        var ellipse = new EllipsePolygon(15, 15, 10);
                        o.Fill(Color.Red, ellipse);
                    }
                });


                if (peopleBox != null)
                {
                    var colorindex = peopleBox.Class.Id % colors.Length;
                    var pen = Pens.Solid(colors[colorindex], 3);
                    var brush = Brushes.Solid(colors[colorindex]);

                    var name = peopleBox.Class.Name;
                    detect.Mutate(o => { o.Draw(pen, new RectangleF(peopleBox.Bounds.X, peopleBox.Bounds.Y, peopleBox.Bounds.Width, peopleBox.Bounds.Height)); });
                    if (peopleBox.Keypoints.Count == 17)
                    {
                        detect.Mutate((Action<IImageProcessingContext>)(o =>
                        {
                            var p0 = peopleBox.Keypoints[0].Point;
                            var p1 = peopleBox.Keypoints[1].Point;
                            var p2 = peopleBox.Keypoints[2].Point;
                            var p3 = peopleBox.Keypoints[3].Point;
                            var p4 = peopleBox.Keypoints[4].Point;
                            var p5 = peopleBox.Keypoints[5].Point;
                            var p6 = peopleBox.Keypoints[6].Point;
                            var p7 = peopleBox.Keypoints[7].Point;
                            var p8 = peopleBox.Keypoints[8].Point;
                            var p9 = peopleBox.Keypoints[9].Point;
                            var p10 = peopleBox.Keypoints[10].Point;
                            var p5c = peopleBox.Keypoints[5].Confidence;
                            var p6c = peopleBox.Keypoints[6].Confidence;
                            var p7c = peopleBox.Keypoints[7].Confidence;
                            var p8c = peopleBox.Keypoints[8].Confidence;
                            var p9c = peopleBox.Keypoints[9].Confidence;
                            var p10c = peopleBox.Keypoints[10].Confidence;


                            for (int i = 1; i <= 4; i++)
                                DrawPoint(detect, peopleBox, colors[colorindex], i);

                            var 眼睛中间点 = new PointF((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
                            var 耳朵中间点 = new PointF((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);


                            //以"耳朵中间点"为中心，计算"眼睛中间点"的角度
                            double 角度 = Math.Atan2(眼睛中间点.Y - 耳朵中间点.Y, 眼睛中间点.X - 耳朵中间点.X) * 180 / Math.PI;
                            var txt角度 = $"角度：{角度:F0}";
                            o.DrawText(txt角度, font, brush, new PointF(peopleBox.Bounds.X, peopleBox.Bounds.Y));

                            var path = new SixLabors.ImageSharp.Drawing.ArcLineSegment(眼睛中间点, new SizeF(50, 50), 0, (float)角度 - 45 / 2, 45);
                            var path_point = path.Flatten().ToArray().ToList(); ;
                            path_point.Insert(0, 眼睛中间点);
                            path_point.Append(眼睛中间点);
                            o.DrawPolygon(pen, path_point.ToArray());


                            //o.DrawText(name, font, brush, new PointF(box.Bounds.X, box.Bounds.Y - 30));
                            if (p5c > 0.7 && p7c > 0.7 && p9c > 0.7)
                            {
                                if (!SixLabors.ImageSharp.Point.Equals(p5, p7) && !SixLabors.ImageSharp.Point.Equals(p7, p9) && !SixLabors.ImageSharp.Point.Equals(p5, p9))
                                    o.DrawLine(pen, p5, p7, p9);
                            }
                            if (p6c > 0.7 && p8c > 0.7 && p10c > 0.7)
                                if (!SixLabors.ImageSharp.Point.Equals(p6, p8) && !SixLabors.ImageSharp.Point.Equals(p8, p10) && !SixLabors.ImageSharp.Point.Equals(p6, p10))
                                    o.DrawLine(pen, p6, p8, p10);
                        }));
                    }
                    else
                    {
                        foreach (var key in peopleBox.Keypoints)
                        {
                            var c = colors[colorindex];
                            detect.Mutate(o => o.Draw(pen, new RectangleF(key.Point.X - 2, key.Point.Y - 2, 4, 4)));
                            //if (key.Point.X >= 0 && key.Point.Y >= 0 && key.Point.X < detect.Width && key.Point.Y < detect.Height)
                            //    detect[key.Point.X, key.Point.Y] = c.ToPixel<Rgb24>();
                        }
                    }

                }


                Mat mat = ImageLib.ImageToMat(detect);

                d.Detect = mat;

            }
        }

    }


    void DrawPoint(Image<Rgb24> detect, Compunet.YoloV8.Data.IPoseBoundingBox box, Color c, int keyIndex)
    {
        if (box.Keypoints[keyIndex].Point.X >= 0 && box.Keypoints[keyIndex].Point.Y >= 0 && box.Keypoints[keyIndex].Point.X < detect.Width && box.Keypoints[keyIndex].Point.Y < detect.Height)
            if (box.Keypoints[keyIndex].Confidence > 0.5)
                detect[box.Keypoints[keyIndex].Point.X, box.Keypoints[keyIndex].Point.Y] = c.ToPixel<Rgb24>();
    }

    bool GetPoint(Compunet.YoloV8.Data.IPoseBoundingBox box, int keyIndex, float threshold, out SixLabors.ImageSharp.Point p)
    {
        if (box.Keypoints[keyIndex].Confidence > threshold)
        {
            p = box.Keypoints[keyIndex].Point;
            return true;
        }
        p = new SixLabors.ImageSharp.Point();
        return false;
    }


    string NewFileName(string path, string prefix, string ext)
    {
        if (Directory.Exists(path) == false)
            Directory.CreateDirectory(path);
        for (int i = 0; i < 1000; i++)
        {
            var filename = System.IO.Path.Combine(path, $"{prefix}{i:D3}{ext}");
            FileInfo fi = new FileInfo(filename);

            if (fi.Exists == false)
            {
                return fi.FullName;
            }
        }
        return null;
    }

    public void Run(string url, string model, RectangleF peopel_rect_float, RectangleF train_rect_float)
    {

        ThreadSafeDataSet ds = new ThreadSafeDataSet();

        Console.WriteLine($"启动视频捕捉线程...");
        Thread CaptureThread = new Thread(this.CaptureThread);
        CaptureThread.Start((url, ds));

        Console.WriteLine($"启动检测线程...");
        Thread DetectThread = new Thread(this.DetectThread);
        DetectThread.Start((model, ds, peopel_rect_float, train_rect_float));

        Stopwatch swDisplay = new Stopwatch();
        while (!ExitFlag)
        {
            if (!pause)
            {
                swDisplay.Restart();
                (string strDatasetInfo, var total, var count) = getDataSetInfo(ds);
                Console.WriteLine(strDatasetInfo);

                if (total != 0)
                    fps_text = $"FPS:{(double)count / total * fps:F1}";


                Mat display = null;
                if (live)
                {
                    var d1 = ds.LastOrDefault();
                    if (d1 != null)
                        display = d1.Source;
                }
                else
                {
                    var d2 = ds.LastDetectDataSet();
                    if (d2 != null)
                        display = d2.Detect;
                }
                if (display != null)
                {
                    Cv2.ImShow("YOLOv8 Detect Video", display);
                }

                swDisplay.Stop();
                int delay = (int)(1000f / fps - swDisplay.ElapsedMilliseconds);
                if (delay > 0)
                {
                    //Console.WriteLine($"显示延时:{delay}");
                    Thread.Sleep(delay);
                }
            }
            else
            {
                Thread.Sleep(10);
            }

            var key = Cv2.WaitKey(1);
            switch (key)
            {
                case 27:
                    ExitFlag = true;
                    break;
                case ' ':
                    pause = !pause;
                    Console.WriteLine("按了空格键");
                    break;
                case 'A':
                case 'a':
                    if (!reverse)
                    {
                        reverse = true;
                        Console.WriteLine("按了a键");
                    }
                    break;
                case 'D':
                case 'd':
                    if (!advance)
                    {
                        advance = true;
                        Console.WriteLine("按了d键");
                    }
                    break;
                case 'L':
                case 'l':
                    live = !live;
                    Console.WriteLine("按了l键");
                    break;
                case 'R':
                case 'r':
                    record = true;
                    Console.WriteLine("按了r键");
                    break;
                case 'Q':
                case 'q':
                    recapture = true;
                    Console.WriteLine("按了q键");
                    break;

                case '1':
                    scaleIndex = 0;
                    break;
                case '2':
                    scaleIndex = 1;
                    break;
                case '3':
                    scaleIndex = 2;
                    break;
                case '4':
                    scaleIndex = 3;
                    break;
            }
        }

        Cv2.DestroyAllWindows();
    }
}


class DataSet
{
    public Mat Source { get; set; } = new Mat();
    public Mat Detect { get; set; } = null;
    public Dictionary<string, double> Values { get; set; }
}
class ThreadSafeDataSet
{
    private readonly ReaderWriterLockSlim lockSlim = new ReaderWriterLockSlim();
    private List<DataSet> MyDataSet = new List<DataSet>();
    public int Count
    {
        get
        {
            lockSlim.EnterReadLock();
            try
            {
                return MyDataSet.Count;
            }
            finally
            {
                lockSlim.ExitReadLock();
            }
        }
    }

    public DataSet First()
    {
        lockSlim.EnterReadLock();
        try
        {
            if (MyDataSet.Count > 0)
            {
                return MyDataSet[0];
            }
            else
            {
                return null; // 或者抛出异常，具体根据需求决定
            }
        }
        finally
        {
            lockSlim.ExitReadLock();
        }
    }

    public DataSet LastOrDefault()
    {
        lockSlim.EnterReadLock();
        var r = MyDataSet.LastOrDefault();
        lockSlim.ExitReadLock();
        return r;

    }

    public DataSet LastDetectDataSet()
    {
        lockSlim.EnterReadLock();
        try
        {
            for (int i = MyDataSet.Count - 1; i >= 0; i--)
            {
                DataSet ds = MyDataSet[i];
                if (ds.Detect != null)
                {
                    return ds;
                }
            }
            return null;
        }
        finally
        {
            lockSlim.ExitReadLock();
        }

    }

    public DataSet LastNoneDetectDataSet()
    {
        lockSlim.EnterReadLock();
        var r = MyDataSet.LastOrDefault();
        lockSlim.ExitReadLock();
        if (r == null || r.Detect != null)
            return null;
        else
            return r;

    }


    public void AddDataSet(DataSet dataSet)
    {
        lockSlim.EnterWriteLock();
        try
        {
            MyDataSet.Add(dataSet);
            while (MyDataSet.Count > 100)
            {
                MyDataSet.RemoveAt(0);
            }
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public void RemoveDataSet(DataSet dataSet)
    {
        lockSlim.EnterWriteLock();
        try
        {
            MyDataSet.Remove(dataSet);
        }
        finally
        {
            lockSlim.ExitWriteLock();
        }
    }

    public List<DataSet> GetAllDataSets()
    {
        lockSlim.EnterReadLock();
        try
        {
            return new List<DataSet>(MyDataSet);
        }
        finally
        {
            lockSlim.ExitReadLock();
        }
    }
}

static public class ImageLib
{

    static public Mat ImageToMat(Image<Rgb24> detect)
    {
        detect.DangerousTryGetSinglePixelMemory(out Memory<Rgb24> pixels);
        var span = pixels.Span;
        var bytes = MemoryMarshal.Cast<Rgb24, byte>(span).ToArray();
        var mat = new Mat(detect.Height, detect.Width, MatType.CV_8UC3);
        int length = detect.Width * detect.Height * 3;
        Marshal.Copy(bytes, 0, mat.Data, length);
        mat = mat.CvtColor(ColorConversionCodes.RGB2BGR);
        return mat;
    }

    static public Image<Rgb24> MatToImage(Mat source)
    {
        byte[] image_byte = new byte[source.Width * source.Height * 3];

        using (var source2 = source.CvtColor(ColorConversionCodes.BGR2RGB).Reshape(1))
        {
            source2.GetArray(out image_byte);
        }
        var image = Image.LoadPixelData<Rgb24>(image_byte, source.Width, source.Height);
        return image;
    }
}


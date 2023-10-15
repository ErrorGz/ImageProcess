using DevExpress.Mvvm.Native;
using DevExpress.XtraReports.Design;
using OpenCvSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using SixLabors.ImageSharp;
using System.Diagnostics;

namespace ImageLabel
{

    public enum OperatingModes
    {
        None, MoveBounding, AddBounding, AddPoint, AddLine, AddPolygon, AddCircle, AddText
    }
    public class Threshold
    {
        public int 移动阈值宽度 { get; set; }
        public int 移动阈值高度 { get; set; }
        public int 放大阈值宽度 { get; set; }
        public int 放大阈值高度 { get; set; }
        public int 缩小阈值宽度 { get; set; }
        public int 缩小阈值高度 { get; set; }
    }
    public class Project
    {
        public List<Classifier> listItemData { get; set; }
        public List<WorkSpaceClass> listworkspace { get; set; }
        public Dictionary<int, string> listyololabel { get; set; }
        public List<string> listYoloModelFilePath { get; set; }

        public Project()
        {

        }
        public Project(List<WorkSpaceClass> w, Dictionary<int, string> l, List<Classifier> d, List<string> p)
        {
            listworkspace = w;
            listyololabel = l;
            listItemData = d;
            listYoloModelFilePath = p;
        }
    }
    public class Classifier
    {
        public string Text { get; set; }
        public ItemParameter Parameter { get; set; }
    }
    public class ItemParameter
    {
        public ShowParameterType type { get; set; }
        public ShowParameterTag tag { get; set; }

        public int Id { get; set; }
        public string Name { get; set; }

        public string Text { get; set; }
    }


    public enum ShowParameterType
    {
        WorkSpace, Label
    }
    public enum ShowParameterTag
    {
        显示全部, 显示已标签, 显示未标签
    }

    public class DoubleInt : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int NewId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class DoubleString : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string NewName { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Prediction
    {
        public Box Box { get; set; }
        public string Label { get; set; }
        public float Confidence { get; set; }
    }

    public class Box
    {
        public float Xmin { get; set; }
        public float Ymin { get; set; }
        public float Xmax { get; set; }
        public float Ymax { get; set; }

        public Box(float xmin, float ymin, float xmax, float ymax)
        {
            Xmin = xmin;
            Ymin = ymin;
            Xmax = xmax;
            Ymax = ymax;

        }
    }
    public static class ImageSharpExtensions
    {

        public static OpenCvSharp.Mat Resize(Mat img_mat, int w_new, int h_new)
        {
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


        public static System.Drawing.Image FixedSize(System.Drawing.Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = System.Convert.ToInt16((Width -
                              (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = System.Convert.ToInt16((Height -
                              (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height,
                              PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution,
                             imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(System.Drawing.Color.White);
            grPhoto.InterpolationMode =
                    InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new System.Drawing.Rectangle(destX, destY, destWidth, destHeight),
                new System.Drawing.Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

    }

    static public class lib
    {
        static public void NormalizeRectangle(double ScaleFactor, System.Drawing.Rectangle inputRect, System.Drawing.Size inputImageSize, BoxClass outputBox)
        {
            outputBox.x = (double)inputRect.X / inputImageSize.Width / ScaleFactor;
            outputBox.y = (double)inputRect.Y / inputImageSize.Height / ScaleFactor;
            outputBox.width = (double)inputRect.Width / inputImageSize.Width / ScaleFactor;
            outputBox.height = (double)inputRect.Height / inputImageSize.Height / ScaleFactor;
            outputBox.cx = outputBox.x + outputBox.width / 2;
            outputBox.cy = outputBox.y + outputBox.height / 2;

        }
        static public void PutText(Mat img_mat, string msg, int x, int y)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;
            var listmsg = msg.Split('\n');
            for (int row = 0; row < listmsg.Count(); row++)
            {
                var str = listmsg[row];
                int baseline = 0;
                var textsize = Cv2.GetTextSize(str, HersheyFonts.HersheySimplex, 0.8f, 2, out baseline);
                var point = new OpenCvSharp.Point(x, y + (row + 1) * (textsize.Height * 1.2));
                Cv2.PutText(img_mat, str, point, HersheyFonts.HersheySimplex, 0.8f, new OpenCvSharp.Scalar(255, 255, 255), 2, LineTypes.Link8, false);

            }

        }
        static public void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }


        static public System.Drawing.Color[] Colors = {
                                    System.Drawing.Color.LightSalmon ,System.Drawing. Color.LightSeaGreen ,System.Drawing.Color.LightSteelBlue , System.Drawing.Color.LightYellow ,
                                    System.Drawing.Color.MediumOrchid, System.Drawing.Color.MediumSeaGreen, System.Drawing.Color.MediumBlue,System.Drawing.Color.NavajoWhite,
                                    System.Drawing.Color.Orchid,System.Drawing.Color.OliveDrab,System.Drawing.Color.DarkSlateBlue,System.Drawing.Color.PapayaWhip,
                                    System.Drawing.Color.PaleVioletRed,System.Drawing.Color.DarkSlateGray,System.Drawing.Color.DeepSkyBlue,System.Drawing.Color.Gold,
                                    System.Drawing.Color.HotPink,System.Drawing.Color.SpringGreen,System.Drawing.Color.SteelBlue,System.Drawing.Color.Khaki,
                                    System.Drawing.Color.Tomato,System.Drawing.Color.LawnGreen,System.Drawing.Color.LightBlue,System.Drawing.Color.LightGoldenrodYellow,
                                    System.Drawing.Color.LightCoral,System.Drawing.Color.Teal,System.Drawing.Color.Turquoise,System.Drawing.Color.Wheat,
                                    System.Drawing.Color.LightPink,System.Drawing.Color.LightGreen,System.Drawing.Color.LightCyan,System.Drawing.Color.Yellow
        };
    }
    public class StaticLib
    {
        static public string[] VideoFormat = { ".mp4", ".avi" };
        static public string[] ImageFormat = { ".jpg", ".png", ".bmp", ".gif" };
        static public string[] TextFormat = { ".txt" };
        static public string[] YAMLFormat = { ".yaml" };
        static public string[] ONNXFormat = { ".onnx" };

        static public Dictionary<int, string> YoloLabels = new Dictionary<int, string>()
        {
            {0,"person" },
            {1,"bicycle" },
            {2,"car" },
            {3,"motorcycle" },
            {4,"airplane" },
            {5,"bus" },
            {6,"train" },
            {7,"truck" },
            {8,"boat" },
            {9,"traffic light" },
            {10,"fire hydrant" },
            {11,"stop sign" },
            {12,"parking meter" },
            {13,"bench" },
            {14,"bird" },
            {15,"cat" },
            {16,"dog" },
            {17,"horse" },
            {18,"sheep" },
            {19,"cow" },
            {20,"elephant" },
            {21,"bear" },
            {22,"zebra" },
            {23,"giraffe" },
            {24,"backpack" },
            {25,"umbrella" },
            {26,"handbag" },
            {27,"tie" },
            {28,"suitcase" },
            {29,"frisbee" },
            {30,"skis" },
            {31,"snowboard" },
            {32,"sports ball" },
            {33,"kite" },
            {34,"baseball bat" },
            {35,"baseball glove" },
            {36,"skateboard" },
            {37,"surfboard" },
            {38,"tennis racket" },
            {39,"bottle" },
            {40,"wine glass" },
            {41,"cup" },
            {42,"fork" },
            {43,"knife" },
            {44,"spoon" },
            {45,"bowl" },
            {46,"banana" },
            {47,"apple" },
            {48,"sandwich" },
            {49,"orange" },
            {50,"broccoli" },
            {51,"carrot" },
            {52,"hot dog" },
            {53,"pizza" },
            {54,"donut" },
            {55,"cake" },
            {56,"chair" },
            {57,"couch" },
            {58,"potted plant" },
            {59,"bed" },
            {60,"dining table" },
            {61,"toilet" },
            {62,"tv" },
            {63,"laptop" },
            {64,"mouse" },
            {65,"remote" },
            {66,"keyboard" },
            {67,"cell phone" },
            {68,"microwave" },
            {69,"oven" },
            {70,"toaster" },
            {71,"sink" },
            {72,"refrigerator" },
            {73,"book" },
            {74,"clock" },
            {75,"vase" },
            {76,"scissors" },
            {77,"teddy bear" },
            {78,"hair drier" },
            {79,"toothbrush" }
};


        static public ImageDataClass FindImageDataInWorkSpaceList(List<WorkSpaceClass> wsc, string ImageFileName)
        {
            ImageDataClass idc = null;
            foreach (var o in wsc)
            {
                idc = o.ImageData.Where(p => p.ImageFileName == ImageFileName).FirstOrDefault();
                if (idc != null)
                    break;
            }
            return idc;
        }

        static public System.Drawing.Image resizeImage(System.Drawing.Image imgToResize, System.Drawing.Size size)
        {

            //Get the image current width  
            int sourceWidth = imgToResize.Width;
            //Get the image current height  
            int sourceHeight = imgToResize.Height;
            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;
            //Calulate  width with new desired size  
            nPercentW = ((float)size.Width / (float)sourceWidth);
            //Calculate height with new desired size  
            nPercentH = ((float)size.Height / (float)sourceHeight);
            if (nPercentH < nPercentW)
                nPercent = nPercentH;
            else
                nPercent = nPercentW;
            //New Width  
            int destWidth = (int)(sourceWidth * nPercent);
            //New Height  
            int destHeight = (int)(sourceHeight * nPercent);
            Bitmap b = new Bitmap(destWidth, destHeight);
            Graphics g = Graphics.FromImage((System.Drawing.Image)b);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            // Draw image with new width and height  
            g.DrawImage(imgToResize, 0, 0, destWidth, destHeight);
            g.Dispose();
            return (System.Drawing.Image)b;
        }


        static public string GetNewDirName(string strPath, string Strprefix)
        {
            int maxnumber = 0;
            var dirs = Directory.GetDirectories(strPath);
            dirs.ForEach(o =>
            {
                var dir = Path.GetFileName(o);
                Regex r = new Regex($@"{Strprefix}_(\d*)");
                var m = r.Match(dir);
                if (m.Success)
                {
                    var number = int.Parse(m.Groups[1].Value);
                    if (number > maxnumber)
                        maxnumber = number;
                }
            });
            return Path.Combine(strPath, $"{Strprefix}_{(maxnumber + 1).ToString("00000")}");
        }

        static public string GetNewFileName(string strPath, string Strprefix, string strExt)
        {
            int maxnumber = 0;
            var files = Directory.GetFiles(strPath);
            var imagefiles = files.Where(o => StaticLib.ImageFormat.Contains(Path.GetExtension(o)));
            imagefiles.ForEach(o =>
            {

                Regex r = new Regex($@"{Strprefix}_(\d*){strExt}");
                var m = r.Match(o);
                if (m.Success)
                {
                    var number = int.Parse(m.Groups[1].Value);
                    if (number > maxnumber)
                        maxnumber = number;
                }
            });
            return Path.Combine(strPath, $"{Strprefix}_{(maxnumber + 1).ToString("00000")}{strExt}");
        }

        static public string GetShowParameterTagName(ShowParameterTag tag)
        {
            return Enum.GetName(typeof(ShowParameterTag), tag);
        }
    }


    public class YOLODataClass
    {
        public string train { get; set; }
        public string val { get; set; }
        public string test { get; set; }
        public int nc { get; set; }
        public List<string> names { get; set; }

        public Dictionary<string, string> roboflow { get; set; }

    }

    public class WorkSpaceClass
    {
        Action<string> OnPutMessage = null;
        public string WorkSpacePath { get; set; }
        public string YAMLFileName { get; set; }
        public List<ImageDataClass> ImageData { get; set; }
        public Dictionary<int, string> Labels { get; set; }


        public WorkSpaceClass()
        {
            ImageData = new();
            Labels = new();
        }
        public WorkSpaceClass(string FilePath, Dictionary<int, string> listYoloLabel, Action<string> PutMessage = null)
        {
            ImageData = new();
            Labels = new();
            OnPutMessage = PutMessage;
            WorkSpacePath = FilePath;
            if (System.IO.Directory.Exists(FilePath) == false)
            {
                OnPutMessage("读取工作文件夹：无效文件夹");
                return;
            }

            OnPutMessage($"读取工作文件夹...");
            var allfiles = GetAllFilesInPath(FilePath);


            OnPutMessage($"读取工作文件夹：创建ImageData对象");
            var imagefilefullpath = allfiles.Where(o =>
            {
                var ext = Path.GetExtension(o);
                return !string.IsNullOrEmpty(ext) && StaticLib.ImageFormat.Contains(ext);

            });


            imagefilefullpath.ForEach(o =>
            {
                var filename = Path.GetFileName(o);
                var name = Path.GetFileNameWithoutExtension(filename);
                var textfilename = name + ".txt";
                var textfilefullpath = allfiles.Where(o => o.EndsWith(textfilename)).FirstOrDefault();
                ImageData.Add(new ImageDataClass() { ImageFileName = o, TextFileName = textfilefullpath, ObjectBox = new List<BoxClass>() });

            });



            OnPutMessage($"读取工作文件夹：创建Labels对象");
            var yamlfilefullpath = allfiles.Where(o =>
            {
                var ext = Path.GetExtension(o);
                return !string.IsNullOrEmpty(ext) && StaticLib.YAMLFormat.Contains(ext);

            });
            yamlfilefullpath.ForEach(o =>
           {

               try
               {
                   var yamltext = File.ReadAllText(o);
                   var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                   try
                   {
                       var yolo = deserializer.Deserialize<YOLODataClass>(yamltext);
                       for (int i = 0; i < yolo.names.Count; i++)
                       {
                           Labels.Add(i, yolo.names[i]);
                       }
                   }
                   catch (Exception)
                   {

                   }
               }
               catch (Exception ex)
               {
                   MessageBox.Show(ex.Message);
               }




           });
            OnPutMessage($"读取工作文件夹：创建ObjectBox对象");

            ImageData.ForEach(o =>
            {
                using var img_mat = Cv2.ImRead(o.ImageFileName);
                o.ImageWidth = img_mat.Width;
                o.ImageHeight = img_mat.Height;
                //using var image = SixLabors.ImageSharp.Image.Load(o.ImageFileName);
                //o.ImageWidth = image.Width;
                //o.ImageHeight = image.Height;

                if (o.TextFileName != null)
                {
                    OnPutMessage($"读取工作文件夹：读取{o.TextFileName}信息");
                    var textlist = System.IO.File.ReadAllLines(o.TextFileName);
                    textlist.ForEach(line =>
                    {
                        var pstr = line.Trim().Split(' ').ToList();
                        pstr.RemoveAll(string.IsNullOrWhiteSpace);
                        if (pstr.Count >= 5)
                        {
                            int Id = int.Parse(pstr[0]);
                            double cx = double.Parse(pstr[1]);
                            double cy = double.Parse(pstr[2]);
                            double w = double.Parse(pstr[3]);
                            double h = double.Parse(pstr[4]);
                            var labelName = listYoloLabel.Where(o => o.Key == Id).Select(o => o.Value).FirstOrDefault();

                            var box = new BoxClass(Id, labelName, cx, cy, cx - w / 2, cy - h / 2, w, h);

                            for (int i = 5; i < pstr.Count; i = i + 3)
                            {
                                try
                                {
                                    var r1 = double.TryParse(pstr[i], out double dx);
                                    var r2 = double.TryParse(pstr[i + 1], out double dy);
                                    var r3 = int.TryParse(pstr[i + 2], out int ic);
                                    if (r1 && r2 && r3)
                                        box.points.Add(new PointClass() { x = dx, y = dy, c = ic });
                                }
                                catch (Exception e)
                                {
                                    Debug.WriteLine($"box.points.Add:{e.Message}");

                                }
                            }
                            o.ObjectBox.Add(box);
                        }



                        //var regex = new Regex("^([0-9]\\d*) ([0-9]\\d*\\.\\d*|0\\.\\d*[0-9]\\d*) ([0-9]\\d*\\.\\d*|0\\.\\d*[0-9]\\d*) ([0-9]\\d*\\.\\d*|0\\.\\d*[0-9]\\d*) ([0-9]\\d*\\.\\d*|0\\.\\d*[0-9]\\d*)$");
                        //var m = regex.Match(line);
                        //if (m.Success)
                        //{
                        //    var Id = int.Parse(m.Groups[1].Value);
                        //    var cx = double.Parse(m.Groups[2].Value);
                        //    var cy = double.Parse(m.Groups[3].Value);
                        //    var w = double.Parse(m.Groups[4].Value);
                        //    var h = double.Parse(m.Groups[5].Value);


                        //    //var label = Labels.Find(q => q.Key == Id);
                        //    var labelName = listYoloLabel.Where(o => o.Key == Id).Select(o => o.Value).FirstOrDefault();

                        //    o.ObjectBox.Add(new ObjectBoxClass(Id, labelName, cx, cy, cx - w / 2, cy - h / 2, w, h));

                        //}

                    });
                }

            });
            OnPutMessage($"读取工作文件夹：读取完毕");

        }
        public List<string> GetAllFilesInPath(string FilePath)
        {
            List<string> files = new List<string>();
            files.AddRange(System.IO.Directory.GetFiles(FilePath));
            var dirs = System.IO.Directory.GetDirectories(FilePath);
            dirs.ForEach(o =>
            {
                files.AddRange(GetAllFilesInPath(o));
            });
            if (OnPutMessage != null)
            {
                OnPutMessage($"读取工作文件夹：搜索{FilePath}");
            }
            return files;

        }
        static public WorkSpaceClass Add(WorkSpaceClass ws1, WorkSpaceClass ws2)
        {
            WorkSpaceClass nws = new WorkSpaceClass();
            if (ws1 != null)
            {
                nws.ImageData.AddRange(ws1.ImageData);
                ws1.Labels.ForEach(o =>
                {
                    if (nws.Labels.ContainsKey(o.Key) == false)
                        nws.Labels.Add(o.Key, o.Value);
                });
                nws.WorkSpacePath = ws1.WorkSpacePath;
            }
            if (ws2 != null)
            {
                nws.ImageData.AddRange(ws2.ImageData);
                ws2.Labels.ForEach(o =>
                {
                    if (nws.Labels.ContainsKey(o.Key) == false)
                        nws.Labels.Add(o.Key, o.Value);
                });
                nws.WorkSpacePath = ws2.WorkSpacePath;
            }
            if (ws1 != null && ws2 != null)
            {
                nws.WorkSpacePath = ws1.WorkSpacePath.Length <= ws2.WorkSpacePath.Length ? ws1.WorkSpacePath : ws2.WorkSpacePath;
            }
            return nws;
        }
    }
    public class ImageDataClass
    {
        public string ImageFileName { get; set; }
        public string TextFileName { get; set; }
        public float ImageWidth { get; set; }
        public float ImageHeight { get; set; }
        public List<BoxClass> ObjectBox { get; set; }

        public bool Saved = true;
        public Mat[] hist { get; set; }

    }

    public class PointClass
    {
        public double x { get; set; }
        public double y { get; set; }
        public int c { get; set; }
    }
    public class BoxClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double cx { get; set; }
        public double cy { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }

        public List<PointClass> points { get; set; } = new List<PointClass>();

        public BoxClass()
        {

        }
        public BoxClass(int iId, string strName, System.Drawing.Size s, System.Drawing.Rectangle r)
        {
            this.Id = iId;
            this.Name = strName;
            this.x = (double)r.X / s.Width;
            this.y = (double)r.Y / s.Height;
            this.width = (double)r.Width / s.Width;
            this.height = (double)r.Height / s.Height;
            this.cx = this.x + this.width / 2;
            this.cy = this.y + this.height / 2;
        }

        public BoxClass(int iId, string strName, double icx, double icy, double ix, double iy, double iw, double ih)
        {
            Id = iId;
            Name = strName;
            cx = icx;
            cy = icy;
            x = ix;
            y = iy;
            width = iw;
            height = ih;
        }

        public void CalBox_xywh(System.Drawing.Size s, System.Drawing.Rectangle r, double ScaleFactor)
        {
            this.x = (double)r.X / s.Width / ScaleFactor;
            this.y = (double)r.Y / s.Height / ScaleFactor;
            this.width = (double)r.Width / s.Width / ScaleFactor;
            this.height = (double)r.Height / s.Height / ScaleFactor;
            this.cx = this.x + this.width / 2;
            this.cy = this.y + this.height / 2;
        }

        public void AddPoint(PointClass p)
        {
            this.points.Add(p);
        }

        public void ClearPoint()
        {
            this.points.Clear();
        }
    }



    public class FPS
    {
        DateTime LastSeconds = DateTime.Now;
        Queue<float> queueDifferentSeconds = new Queue<float>();

        public FPS()
        {
            LastSeconds = DateTime.Now;
        }

        public float Get()
        {
            var now = DateTime.Now;
            var different_Secondes = (now - LastSeconds).Ticks / 10000000f;
            LastSeconds = now;
            queueDifferentSeconds.Enqueue(different_Secondes);
            if (queueDifferentSeconds.Count > 30)
                queueDifferentSeconds.Dequeue();
            var AgvsFPS = 1 / (queueDifferentSeconds.Sum() / queueDifferentSeconds.Count);

            //Debug.WriteLine($"{different_Secondes}  1/({queueDifferentSeconds.Sum()}/{queueDifferentSeconds.Count})={AgvsFPS}");
            return AgvsFPS;
        }
    }


    internal static class ImageExtensions
    {
        #region Public Methods

        /// <summary>
        /// Extension method that converts a Image to an byte array
        /// </summary>
        /// <param name="imageIn">The Image to convert</param>
        /// <returns>An byte array containing the JPG format Image</returns>
        public static byte[] ToArray(this SixLabors.ImageSharp.Image imageIn)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, JpegFormat.Instance);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Extension method that converts a Image to an byte array
        /// </summary>
        /// <param name="imageIn">The Image to convert</param>
        /// <param name="fmt"></param>
        /// <returns>An byte array containing the JPG format Image</returns>
        public static byte[] ToArray(this SixLabors.ImageSharp.Image imageIn, IImageFormat fmt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, fmt);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Extension method that converts a Image to an byte array
        /// </summary>
        /// <param name="imageIn">The Image to convert</param>
        /// <returns>An byte array containing the JPG format Image</returns>
        public static byte[] ToArray(this global::System.Drawing.Image imageIn)
        {
            return ToArray(imageIn, ImageFormat.Png);
        }

        /// <summary>
        /// Converts the image data into a byte array.
        /// </summary>
        /// <param name="imageIn">The image to convert to an array</param>
        /// <param name="fmt">The format to save the image in</param>
        /// <returns>An array of bytes</returns>
        public static byte[] ToArray(this global::System.Drawing.Image imageIn, ImageFormat fmt)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                imageIn.Save(ms, fmt);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Extension method that converts a byte array with JPG data to an Image
        /// </summary>
        /// <param name="byteArrayIn">The byte array with JPG data</param>
        /// <returns>The reconstructed Image</returns>
        public static SixLabors.ImageSharp.Image ToImage(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                SixLabors.ImageSharp.Image returnImage = SixLabors.ImageSharp.Image.Load(ms);
                return returnImage;
            }
        }

        public static global::System.Drawing.Image ToNetImage(this byte[] byteArrayIn)
        {
            using (MemoryStream ms = new MemoryStream(byteArrayIn))
            {
                global::System.Drawing.Image returnImage = global::System.Drawing.Image.FromStream(ms);
                return returnImage;
            }
        }

        #endregion Public Methods
    }
}

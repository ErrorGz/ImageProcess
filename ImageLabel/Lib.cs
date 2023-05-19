using DevExpress.Mvvm.Native;
using DevExpress.XtraReports.Design;
using OpenCvSharp;
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
using Yolo5.NetCore.Models;
using Color = System.Drawing.Color;

namespace ImageLabel
{
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
        public List<ItemData> listItemData { get; set; }
        public List<WorkSpaceClass> listworkspace { get; set; }
        public List<YoloLabelModel> listyololabel { get; set; }
        public List<string> listYoloModelFilePath { get; set; }

        public Project()
        {

        }
        public Project(List<WorkSpaceClass> w, List<YoloLabelModel> l, List<ItemData> d, List<string> p)
        {
            listworkspace = w;
            listyololabel = l;
            listItemData = d;
            listYoloModelFilePath = p;
        }
    }
    public class ItemData
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
        //public static System.Drawing.Bitmap ToBitmap<TPixel>(this Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        var imageEncoder = image.GetConfiguration().ImageFormatsManager.FindEncoder(PngFormat.Instance);
        //        image.Save(memoryStream, imageEncoder);

        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        return new System.Drawing.Bitmap(memoryStream);
        //    }
        //}

        //public static Image<TPixel> ToImageSharpImage<TPixel>(this System.Drawing.Bitmap bitmap) where TPixel : unmanaged, IPixel<TPixel>
        //{
        //    using (var memoryStream = new MemoryStream())
        //    {
        //        bitmap.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);

        //        memoryStream.Seek(0, SeekOrigin.Begin);

        //        return SixLabors.ImageSharp.Image.Load<TPixel>(memoryStream);
        //    }
        //}


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
                Cv2.PutText(img_mat, str, point,  HersheyFonts.HersheySimplex, 0.8f, new OpenCvSharp.Scalar(255, 255, 255), 2, LineTypes.Link8, false);

            }

        }
        static public void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }


        static public Color[] Colors = {   Color.LightSalmon , Color.LightSeaGreen , Color.LightSteelBlue , Color.LightYellow ,
                                    Color.MediumOrchid, Color.MediumSeaGreen, Color.MediumBlue, Color.NavajoWhite,
                                    Color.Orchid,Color.OliveDrab,Color.DarkSlateBlue,Color.PapayaWhip,
                                    Color.PaleVioletRed,Color.DarkSlateGray,Color.DeepSkyBlue,Color.Gold,
                                    Color.HotPink,Color.SpringGreen,Color.SteelBlue,Color.Khaki,
                                    Color.Tomato,Color.LawnGreen,Color.LightBlue,Color.LightGoldenrodYellow,
                                    Color.LightCoral,Color.Teal,Color.Turquoise,Color.Wheat,
                                    Color.LightPink,Color.LightGreen,Color.LightCyan,Color.Yellow
        };
    }
    public class StaticLib
    {
        static public string[] VideoFormat = { ".mp4", ".avi" };
        static public string[] ImageFormat = { ".jpg", ".png", ".bmp", ".gif" };
        static public string[] TextFormat = { ".txt" };
        static public string[] YAMLFormat = { ".yaml" };
        static public string[] ONNXFormat = { ".onnx" };

        static public List<YoloLabelModel> YoloLabels = new List<YoloLabelModel>()
        {
            new YoloLabelModel { Id = 0, Name = "person" },
            new YoloLabelModel { Id = 1, Name = "bicycle" },
            new YoloLabelModel { Id = 2, Name = "car" },
            new YoloLabelModel { Id = 3, Name = "motorcycle" },
            new YoloLabelModel { Id = 4, Name = "airplane" },
            new YoloLabelModel { Id = 5, Name = "bus" },
            new YoloLabelModel { Id = 6, Name = "train" },
            new YoloLabelModel { Id = 7, Name = "truck" },
            new YoloLabelModel { Id = 8, Name = "boat" },
            new YoloLabelModel { Id = 9, Name = "traffic light" },
            new YoloLabelModel { Id = 10, Name = "fire hydrant" },
            new YoloLabelModel { Id = 11, Name = "stop sign" },
            new YoloLabelModel { Id = 12, Name = "parking meter" },
            new YoloLabelModel { Id = 13, Name = "bench" },
            new YoloLabelModel { Id = 14, Name = "bird" },
            new YoloLabelModel { Id = 15, Name = "cat" },
            new YoloLabelModel { Id = 16, Name = "dog" },
            new YoloLabelModel { Id = 17, Name = "horse" },
            new YoloLabelModel { Id = 18, Name = "sheep" },
            new YoloLabelModel { Id = 19, Name = "cow" },
            new YoloLabelModel { Id = 20, Name = "elephant" },
            new YoloLabelModel { Id = 21, Name = "bear" },
            new YoloLabelModel { Id = 22, Name = "zebra" },
            new YoloLabelModel { Id = 23, Name = "giraffe" },
            new YoloLabelModel { Id = 24, Name = "backpack" },
            new YoloLabelModel { Id = 25, Name = "umbrella" },
            new YoloLabelModel { Id = 26, Name = "handbag" },
            new YoloLabelModel { Id = 27, Name = "tie" },
            new YoloLabelModel { Id = 28, Name = "suitcase" },
            new YoloLabelModel { Id = 29, Name = "frisbee" },
            new YoloLabelModel { Id = 30, Name = "skis" },
            new YoloLabelModel { Id = 31, Name = "snowboard" },
            new YoloLabelModel { Id = 32, Name = "sports ball" },
            new YoloLabelModel { Id = 33, Name = "kite" },
            new YoloLabelModel { Id = 34, Name = "baseball bat" },
            new YoloLabelModel { Id = 35, Name = "baseball glove" },
            new YoloLabelModel { Id = 36, Name = "skateboard" },
            new YoloLabelModel { Id = 37, Name = "surfboard" },
            new YoloLabelModel { Id = 38, Name = "tennis racket" },
            new YoloLabelModel { Id = 39, Name = "bottle" },
            new YoloLabelModel { Id = 40, Name = "wine glass" },
            new YoloLabelModel { Id = 41, Name = "cup" },
            new YoloLabelModel { Id = 42, Name = "fork" },
            new YoloLabelModel { Id = 43, Name = "knife" },
            new YoloLabelModel { Id = 44, Name = "spoon" },
            new YoloLabelModel { Id = 45, Name = "bowl" },
            new YoloLabelModel { Id = 46, Name = "banana" },
            new YoloLabelModel { Id = 47, Name = "apple" },
            new YoloLabelModel { Id = 48, Name = "sandwich" },
            new YoloLabelModel { Id = 49, Name = "orange" },
            new YoloLabelModel { Id = 50, Name = "broccoli" },
            new YoloLabelModel { Id = 51, Name = "carrot" },
            new YoloLabelModel { Id = 52, Name = "hot dog" },
            new YoloLabelModel { Id = 53, Name = "pizza" },
            new YoloLabelModel { Id = 54, Name = "donut" },
            new YoloLabelModel { Id = 55, Name = "cake" },
            new YoloLabelModel { Id = 56, Name = "chair" },
            new YoloLabelModel { Id = 57, Name = "couch" },
            new YoloLabelModel { Id = 58, Name = "potted plant" },
            new YoloLabelModel { Id = 59, Name = "bed" },
            new YoloLabelModel { Id = 60, Name = "dining table" },
            new YoloLabelModel { Id = 61, Name = "toilet" },
            new YoloLabelModel { Id = 62, Name = "tv" },
            new YoloLabelModel { Id = 63, Name = "laptop" },
            new YoloLabelModel { Id = 64, Name = "mouse" },
            new YoloLabelModel { Id = 65, Name = "remote" },
            new YoloLabelModel { Id = 66, Name = "keyboard" },
            new YoloLabelModel { Id = 67, Name = "cell phone" },
            new YoloLabelModel { Id = 68, Name = "microwave" },
            new YoloLabelModel { Id = 69, Name = "oven" },
            new YoloLabelModel { Id = 70, Name = "toaster" },
            new YoloLabelModel { Id = 71, Name = "sink" },
            new YoloLabelModel { Id = 72, Name = "refrigerator" },
            new YoloLabelModel { Id = 73, Name = "book" },
            new YoloLabelModel { Id = 74, Name = "clock" },
            new YoloLabelModel { Id = 75, Name = "vase" },
            new YoloLabelModel { Id = 76, Name = "scissors" },
            new YoloLabelModel { Id = 77, Name = "teddy bear" },
            new YoloLabelModel { Id = 78, Name = "hair drier" },
            new YoloLabelModel { Id = 79, Name = "toothbrush" }

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
        static public void BoundsToYoloData(System.Drawing.Size s, System.Drawing.Rectangle r, ref ObjectBoxClass objectBoxClass)
        {

            //Debug.Write($"{objectBoxClass.x},{objectBoxClass.y}-{objectBoxClass.width},{objectBoxClass.height}");
            objectBoxClass.x = (double)r.X / s.Width;
            objectBoxClass.y = (double)r.Y / s.Height;
            objectBoxClass.width = (double)r.Width / s.Width;
            objectBoxClass.height = (double)r.Height / s.Height;
            objectBoxClass.cx = objectBoxClass.x + objectBoxClass.width / 2;
            objectBoxClass.cy = objectBoxClass.y + objectBoxClass.height / 2;
            //Debug.Write($"{objectBoxClass.x},{objectBoxClass.y}-{objectBoxClass.width},{objectBoxClass.height}");
            //Debug.WriteLine("");
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
                var dir = o.Substring(o.LastIndexOf("\\") + 1);
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
            var imagefiles = files.Where(o => StaticLib.ImageFormat.Contains(o.Substring(o.LastIndexOf("."))));
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
        public List<KeyValuePair<int, string>> Labels { get; set; }


        public WorkSpaceClass()
        {
            ImageData = new();
            Labels = new();
        }
        public WorkSpaceClass(string FilePath, List<YoloLabelModel> listYoloLabel, Action<string> PutMessage = null)
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
                var ext = o.Substring(o.LastIndexOf("."));
                return StaticLib.ImageFormat.Contains(ext);

            });


            imagefilefullpath.ForEach(o =>
            {
                var filename = o.Substring(o.LastIndexOf("\\") + 1);
                var name = filename.Substring(0, filename.LastIndexOf("."));
                var textfilename = name + ".txt";
                var textfilefullpath = allfiles.Where(o => o.EndsWith(textfilename)).FirstOrDefault();
                ImageData.Add(new ImageDataClass() { ImageFileName = o, TextFileName = textfilefullpath, ObjectBox = new List<ObjectBoxClass>() });

            });



            OnPutMessage($"读取工作文件夹：创建Labels对象");
            var yamlfilefullpath = allfiles.Where(o =>
            {
                var ext = o.Substring(o.LastIndexOf("."));
                return StaticLib.YAMLFormat.Contains(ext);
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
                       for (int i = 1; i <= yolo.names.Count; i++)
                       {
                           Labels.Add(new KeyValuePair<int, string>(i, yolo.names[i]));
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
                    textlist.ForEach(p =>
                    {
                        var regex = new Regex("^([1-9]\\d*) ([1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*) ([1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*) ([1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*) ([1-9]\\d*\\.\\d*|0\\.\\d*[1-9]\\d*)$");
                        var m = regex.Match(p);
                        if (m.Success)
                        {
                            var Id = int.Parse(m.Groups[1].Value);
                            var x = double.Parse(m.Groups[2].Value);
                            var y = double.Parse(m.Groups[3].Value);
                            var w = double.Parse(m.Groups[4].Value);
                            var h = double.Parse(m.Groups[5].Value);


                            //var label = Labels.Find(q => q.Key == Id);
                            var labelName = listYoloLabel.Where(o => o.Id == Id).Select(o => o.Name).FirstOrDefault();

                            o.ObjectBox.Add(new ObjectBoxClass(Id, labelName, x, y, x - w / 2, y - h / 2, w, h));

                        }

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
                nws.Labels.AddRange(ws1.Labels);
                nws.WorkSpacePath = ws1.WorkSpacePath;
            }
            if (ws2 != null)
            {
                nws.ImageData.AddRange(ws2.ImageData);
                nws.Labels.AddRange(ws2.Labels);
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
        public List<ObjectBoxClass> ObjectBox { get; set; }

        public bool Saved = true;
        public Mat[] hist { get; set; }

    }

    public class ObjectBoxClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double cx { get; set; }
        public double cy { get; set; }
        public double x { get; set; }
        public double y { get; set; }
        public double width { get; set; }
        public double height { get; set; }


        public ObjectBoxClass(int iId, string name, System.Drawing.Size s, System.Drawing.Rectangle r)
        {
            this.Id = iId;
            this.Name = name;
            this.x = (double)r.X / s.Width;
            this.y = (double)r.Y / s.Height;
            this.width = (double)r.Width / s.Width;
            this.height = (double)r.Height / s.Height;
            this.cx = this.x + this.width / 2;
            this.cy = this.y + this.height / 2;
        }

        public ObjectBoxClass(int iId, string name, double icx, double icy, double ix, double iy, double iw, double ih)
        {
            Id = iId;
            Name = name;
            cx = icx;
            cy = icy;
            x = ix;
            y = iy;
            width = iw;
            height = ih;
        }

        public void SetBounds(System.Drawing.Size s, System.Drawing.Rectangle r)
        {
            this.x = (double)r.X / s.Width;
            this.y = (double)r.Y / s.Height;
            this.width = (double)r.Width / s.Width;
            this.height = (double)r.Height / s.Height;
            this.cx = this.x + this.width / 2;
            this.cy = this.y + this.height / 2;
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
}

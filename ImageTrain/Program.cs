using OpenCvSharp;
using OpenCvSharp.ML;
using System.Diagnostics;
using System.Drawing.Printing;
using System.Linq;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;
using static TorchSharp.torch.utils;
using static TorchSharp.torch.utils.data;

namespace ImageTrain
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var DataPath = @"E:\BaiduSyncdisk\PY\DataSet\ImageLib1-3";
            Image_DataBase image_db = new Image_DataBase(DataPath);
            var video_yaml_file = @"E:\BaiduSyncdisk\PY\DataSet\视频\工程E盘.yaml";
            Video_DataBase video_db = new Video_DataBase(video_yaml_file);


            //db.Save(configFilePath);
            Train.DoTrain(image_db);


            //db = Image_DataBase.LoadYAML(configFilePath);
            //Detect det = new Detect(db);


            //Stopwatch s = new Stopwatch();
            //foreach (var file in db.ValidImage)
            //{
            //    s.Restart();
            //    var labels = det.GetDetectLabel(file.Value.ImageFile);

            //    s.Stop();

            //    var msg检测目标 = string.Join(", ", file.Value.bbox.Select(o => new { id = o.LabelId}));
            //    string msg检测结果 = string.Join(", ", labels.Select(o => new { id = o.id }));


            //    Console.WriteLine($"{file.Value.ImageFile} {s.ElapsedMilliseconds}ms \t [{msg检测结果}]-[{msg检测目标}]");

            //}


        }


    }


}
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
            var DataPath = @"DataSet\animals.v2-release.yolov5pytorch";
            var configFilePath = Path.Combine(DataPath, "config.yaml");
            YAMLdatabase db = new YAMLdatabase(DataPath);
            db.Save(configFilePath);
            //Train.DoTrain(db);


            db = YAMLdatabase.LoadYAML(configFilePath);
            Detect det = new Detect(db);

            
            Stopwatch s = new Stopwatch();
            foreach (var file in db.ValidImage)
            {
                s.Restart();
                var labels=det.GetDetectLabel(file.Value.ImageFile);
                s.Stop();          

                var msg实际类型 = string.Join(", ", file.Value.bbox.Select(o => o.Item1));
                string msg检测类型 = string.Join(", ", labels.Select(label => $"{label.Item1}:{label.Item3}({label.Item2:F4})"));


                Console.WriteLine($"{file.Value.ImageFile} {s.ElapsedMilliseconds}ms \t Detect:{msg检测类型} Actual:{msg实际类型}" );
                
            }


        }
                 

    }


}
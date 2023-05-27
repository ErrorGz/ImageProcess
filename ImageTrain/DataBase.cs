using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using TorchSharp;
using TorchSharp.Modules;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils;

namespace ImageTrain
{
    public class YOLOv5config
    {
        public string train { get; set; }
        public string val { get; set; }
        public string test { get; set; }
        public int nc { get; set; }
        public string[] names { get; set; }
        //public string[] roboflow { get; set; }
    }

    public class VideoData
    {
        public string VideoFile { get; set; }
        public int FrameCount { get; set; }
        public int FrameStart { get; set; }
        public int FrameStep { get; set; }
        public string VideoLabel { get; set; }
    }
    public class ImageData
    {
        public string ImageFile { get; set; }
        public List<(int, float, float, float, float)> bbox { get; set; }
    }

    public class YAMLdatabase : IDisposable
    {
        public string YAMLfile { get; set; }
        public Dictionary<long, string> Labels { get; set; } = new Dictionary<long, string>();
        public Dictionary<long, ImageData> TrainImage { get; set; } = new Dictionary<long, ImageData>();
        public Dictionary<long, ImageData> ValidImage { get; set; } = new Dictionary<long, ImageData>();
        public int? _epochs { get; set; } = 200;
        public int? _epochs_current { get; set; } = 0;
        public int? _trainBatchSize { get; set; } = 15;
        public int? _testBatchSize { get; set; } = 15;

        public int? _numWorker { get; set; } = 4;
        public int? _logInterval { get; set; } = 25;
        //public int? _numClasses { get; set; } = 0;
        public int? _timeout { get; set; } = 3600;    // One hour by default.

        public string bestModelPath { get; set; }
        public string lastModelPath { get; set; }

        public string trainpath { get; set; }
        public string validpath { get; set; }
        public string trainimagepath { get; set; }
        public string trainlabelpath { get; set; }
        public string validimagepath { get; set; }
        public string validlabelpath { get; set; }

        [YamlIgnore] public Module<Tensor, Tensor> Model { get; set; }
        [YamlIgnore] public Device device { get; set; } = torch.cuda.is_available() ? torch.CUDA : torch.CPU;

        private YAMLdatabase()
        {

        }
        public YAMLdatabase(string rootpath)
        {
            bestModelPath = Path.Combine(rootpath, "best.pt");
            lastModelPath = Path.Combine(rootpath, "last.pt");
            trainpath = Path.Combine(rootpath, "train");
            validpath = Path.Combine(rootpath, "valid");
            trainimagepath = Path.Combine(trainpath, "images");
            trainlabelpath = Path.Combine(trainpath, "labels");
            validimagepath = Path.Combine(validpath, "images");
            validlabelpath = Path.Combine(validpath, "labels");

            string[] imageext = { ".jpg", ".png", ".jpeg", ".bmp" };
            string[] labelext = { ".txt" };
            string[] yamlext = { "*.yaml" };

            var yamlfiles = Directory.EnumerateFiles(rootpath, "*.yaml");
            foreach (var file in yamlfiles)
            {
                var input = new StreamReader(file);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(NullNamingConvention.Instance)
                    .IgnoreUnmatchedProperties()
                    .Build();
                try
                {
                    var yaml = deserializer.Deserialize<YOLOv5config>(input);
                    // Check if all properties are not null
                    if (yaml.train == null || yaml.val == null || yaml.nc == 0 || yaml.names == null || yaml.names.Length == 0)
                    {
                        //StaticLib.Log("Error: YAML file is missing required properties.");
                        continue;
                    }
                    Console.WriteLine($"加载yaml配置文件：{file}");
                    foreach (var name in yaml.names)
                    {
                        Labels.AddOrGetKey<long, string>(name);
                    }
                    //_numClasses = Labels.Count();
                }
                catch
                {
                    StaticLib.Log("Error: YAML file format is not yolov5 configuration file.");
                    continue;
                }

            }

            var trainfiles = Directory.EnumerateFiles(trainimagepath, "*.*");
            foreach (var file in trainfiles)
            {
                var ext = Path.GetExtension(file);
                if (imageext.Contains(ext))
                {
                    var filename = Path.GetFileNameWithoutExtension(file);
                    var labelfile = Path.Combine(trainlabelpath, filename + ".txt");
                    if (File.Exists(labelfile))
                    {
                        Console.WriteLine($"加载label标签文件：{labelfile}");
                        labelfile = new FileInfo(labelfile).FullName;
                        var label = ReadLabelFile(labelfile);

                        ImageData image = new ImageData() { ImageFile = file, bbox = label };
                        TrainImage.AddOrGetKey<long, ImageData>(image);
                    }
                }
            }
            var validfiles = Directory.EnumerateFiles(validimagepath, "*.*");
            foreach (var file in validfiles)
            {
                var ext = Path.GetExtension(file);
                if (imageext.Contains(ext))
                {
                    var filename = Path.GetFileNameWithoutExtension(file);
                    var labelfile = Path.Combine(validlabelpath, filename + ".txt");
                    if (File.Exists(labelfile))
                    {
                        Console.WriteLine($"加载label标签文件：{labelfile}");
                        labelfile = new FileInfo(labelfile).FullName;
                        var label = ReadLabelFile(labelfile);

                        ImageData image = new ImageData() { ImageFile = file, bbox = label };
                        ValidImage.AddOrGetKey<long, ImageData>(image);
                    }
                }
            }


        }

        public List<(int, float, float, float, float)> ReadLabelFile(string filePath)
        {
            var result = new List<(int, float, float, float, float)>();
            using (var reader = new StreamReader(filePath))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var values = line.Split(' ');
                    if (values.Length != 5)
                    {
                        throw new ArgumentException($"Invalid line format in file {filePath}.");
                    }

                    int labelId = int.Parse(values[0]);
                    float centerX = float.Parse(values[1]);
                    float centerY = float.Parse(values[2]);
                    float width = float.Parse(values[3]);
                    float height = float.Parse(values[4]);

                    result.Add((labelId, centerX, centerY, width, height));
                }
            }

            return result;
        }

        public void Save(string filename)
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                //.WithTypeConverter(new AutoDictionaryConverter())
                .Build();
            var yaml = serializer.Serialize(this);
            File.WriteAllText(filename, yaml);
        }

        public void Load(string filename)
        {
            var yaml = LoadYAML(filename);
            this.Labels = yaml.Labels;
            this.TrainImage = yaml.TrainImage;
            this.ValidImage = yaml.ValidImage;
            this._epochs = yaml._epochs;
            this._epochs_current = yaml._epochs_current;
            this._trainBatchSize = yaml._trainBatchSize;
            this._testBatchSize = yaml._testBatchSize;
            this._logInterval = yaml._logInterval;
            //this._numClasses = yaml._numClasses;
            this._timeout = yaml._timeout;
        }

        static public YAMLdatabase LoadYAML(string yamlFile)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();

            using (var reader = new StreamReader(yamlFile))
            {

                var result = new YAMLdatabase();
                var yaml = deserializer.Deserialize<Dictionary<string, object>>(reader);

                if (yaml.TryGetValue("YAMLfile", out var yamlFileValue))
                {
                    result.YAMLfile = yamlFileValue as string;
                }

                if (yaml.TryGetValue("Labels", out var labelsValue))
                {
                    var labelsDict = labelsValue as Dictionary<object, object>;
                    foreach (var label in labelsDict)
                    {
                        var labelId = long.Parse(label.Key.ToString());
                        var labelName = label.Value as string;
                        result.Labels.Add(labelId, labelName);
                    }
                }
                {
                    if (yaml.TryGetValue("_epochs", out var obj))
                    {
                        result._epochs = int.Parse(obj as string);
                    }
                }

                {
                    if (yaml.TryGetValue("_epochs_current", out var obj))
                    {
                        result._epochs_current = int.Parse(obj as string);
                    }
                }
                {
                    if (yaml.TryGetValue("_trainBatchSize", out var obj))
                    {
                        result._trainBatchSize = int.Parse(obj as string);
                    }
                }
                {
                    if (yaml.TryGetValue("_testBatchSize", out var obj))
                    {
                        result._testBatchSize = int.Parse(obj as string);
                    }
                }
                {
                    if (yaml.TryGetValue("_numWorker", out var obj))
                    {
                        result._numWorker = int.Parse(obj as string);
                    }
                }
                {
                    if (yaml.TryGetValue("_logInterval", out var obj))
                    {
                        result._logInterval = int.Parse(obj as string);
                    }
                }
                //{
                //    if (yaml.TryGetValue("_numClasses", out var obj))
                //    {
                //        result._numClasses =  int.Parse(obj as string);
                //    }

                //}
                {
                    if (yaml.TryGetValue("_timeout", out var obj))
                    {
                        result._timeout = int.Parse(obj as string);
                    }

                }
                {
                    if (yaml.TryGetValue("bestModelPath", out var obj))
                    {
                        result.bestModelPath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("lastModelPath", out var obj))
                    {
                        result.lastModelPath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("trainpath", out var obj))
                    {
                        result.trainpath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("validpath", out var obj))
                    {
                        result.validpath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("trainimagepath", out var obj))
                    {
                        result.trainimagepath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("trainlabelpath", out var obj))
                    {
                        result.trainlabelpath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("validimagepath", out var obj))
                    {
                        result.validimagepath = obj as string;
                    }

                }
                {
                    if (yaml.TryGetValue("validlabelpath", out var obj))
                    {
                        result.validlabelpath = obj as string;
                    }

                }



                result.TrainImage = ParseImageYAML("TrainImage", yaml);
                result.ValidImage = ParseImageYAML("ValidImage", yaml);

                return result;
            }
        }

        static private Dictionary<long, ImageData> ParseImageYAML(string ImageType, Dictionary<string, object> yaml)
        {
            Dictionary<long, ImageData> result = new Dictionary<long, ImageData>();
            if (yaml.TryGetValue(ImageType, out var trainImageValue))
            {
                var ImageDict = trainImageValue as Dictionary<object, object>;
                foreach (var Image in ImageDict)
                {
                    var imageId = long.Parse(Image.Key.ToString());
                    var imageDataDict = (Dictionary<object, object>)Image.Value;
                    var imageData = new ImageData
                    {
                        ImageFile = imageDataDict["ImageFile"].ToString(),
                        bbox = new List<(int, float, float, float, float)>()
                    };
                    if (imageDataDict.TryGetValue("bbox", out var bboxValue))
                    {
                        var bboxList = (List<object>)bboxValue;
                        foreach (var bboxObj in bboxList)
                        {
                            var bboxArray = (bboxObj as Dictionary<object, object>).ToArray();

                            var bbox = (
                                int.Parse(bboxArray[0].Value.ToString()),
                                float.Parse(bboxArray[1].Value.ToString()),
                                float.Parse(bboxArray[2].Value.ToString()),
                                float.Parse(bboxArray[3].Value.ToString()),
                                float.Parse(bboxArray[4].Value.ToString())
                            );
                            imageData.bbox.Add(bbox);
                        }
                    }
                    result.Add(imageId, imageData);
                }
            }
            return result;
        }

        public Module<Tensor, Tensor> LoadModel()
        {
            Model = ImageTrain.torchvision.models.resnet34(num_classes: Labels.Count, device: device);
            if (File.Exists(bestModelPath))
            {
                Model.load(bestModelPath);
            }

            return Model;
        }
        public void Dispose()
        {
            if (Model != null)
                Model.Dispose();
        }
    }


}

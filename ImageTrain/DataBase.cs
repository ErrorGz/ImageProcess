using Newtonsoft.Json;
using OpenCvSharp;
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


    public class VideoProject
    {
        public List<VideoData> VideoDatas { get; set; } = new List<VideoData>();
        public Dictionary<int, string> Labels { get; set; } = new Dictionary<int, string>();
    }
    public class VideoData
    {
        public string VideoURL { get; set; }
        public List<VideoLabel> frames { get; set; } = new List<VideoLabel>();

        public bool isRTSP { get; set; }
        public bool isURB { get; set; }

        [YamlIgnore]
        public VideoCapture capture { get; set; }


    }
    public class VideoLabel
    {
        public int VideoLabelId { get; set; }
        public int FrameStart { get; set; }
        public int FrameCount { get; set; } = 10;
        public int FrameStep { get; set; }

    }

    public abstract class BaseData<TData> : IDisposable where TData : class
    {
        public string YAMLfile { get; set; }

        public Dictionary<int, string> Labels { get; set; } = new Dictionary<int, string>();

        public Dictionary<int, TData> TrainData { get; set; } = new Dictionary<int, TData>();

        public Dictionary<int, TData> ValidData { get; set; } = new Dictionary<int, TData>();

        public int? _epochs { get; set; } = 200;

        public int? _epochs_current { get; set; } = 0;

        public int? _trainBatchSize { get; set; } = 100;

        public int? _testBatchSize { get; set; } = 15;

        public int? _numWorker { get; set; } = 4;

        public int? _logInterval { get; set; } = 25;

        public int? _timeout { get; set; } = 3600;

        public string bestModelPath { get; set; }

        public string lastModelPath { get; set; }

        [YamlIgnore]
        public Module<Tensor, Tensor> Model { get; set; }

        [YamlIgnore]
        public Device device { get; set; } = torch.cuda.is_available() ? torch.CUDA : torch.CPU;

        // 构造函数及其他公共方法

        public abstract Module<Tensor, Tensor> LoadModel(bool loadBestModel = false);

        public void Dispose()
        {
            if (Model != null)
                Model.Dispose();
        }
    }


    public class Video_DataBase : BaseData<VideoData>
    {

        public Video_DataBase(string YAMLfile)
        {
            this.YAMLfile = YAMLfile;
            Console.WriteLine($"加载yaml配置文件：{YAMLfile}");
            using (var reader = new System.IO.StreamReader(YAMLfile))
            {
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(NullNamingConvention.Instance)
                    .Build();
                var config = deserializer.Deserialize<VideoProject>(reader);
                if (config.Labels == null || config.VideoDatas == null)
                    return;
                this.Labels.Clear();
                this.TrainData.Clear();
                this.ValidData.Clear();

                config.Labels.ToList().ForEach(o => { this.Labels.Add(o.Key, o.Value); });
                Random random = new Random();
                for (int c = 0; c < 3; c++)
                    for (int i = 0; i < config.VideoDatas.Count; i++)
                    {
                        var pos = random.Next(config.VideoDatas.Count);
                        var temp = config.VideoDatas[i];
                        config.VideoDatas[i] = config.VideoDatas[pos];
                        config.VideoDatas[pos] = temp;
                    }
                int takeCount = (int)(config.VideoDatas.Count * 0.8);
                List<VideoData> TrainVideo = config.VideoDatas.Take(takeCount).ToList();
                List<VideoData> ValidVideo = config.VideoDatas.Skip(takeCount).ToList();
                TrainVideo.ForEach(o => { this.TrainData.AddOrGetKey<int, VideoData>(o); });
                ValidVideo.ForEach(o => { this.ValidData.AddOrGetKey<int, VideoData>(o); });

            }
        }

        public void Dispose()
        {
            this.Labels.Clear();
            this.TrainData.ToList().ForEach(o =>
            {
                if (o.Value.capture != null)
                    o.Value.capture.Dispose();
            });
            this.ValidData.ToList().ForEach(o =>
            {
                if (o.Value.capture != null)
                    o.Value.capture.Dispose();
            });
            this.TrainData.Clear();
            this.ValidData.Clear();

            this.Model.Dispose();
        }

        public override Module<Tensor, Tensor> LoadModel(bool loadBestModel = false)
        {

            Model = ImageTrain.torchvision.models.resnet34(num_classes: this.Labels.Count, device: device);
            if (loadBestModel && File.Exists(bestModelPath))
            {
                Model.load(bestModelPath);
            }

            return Model;
        }
    }



    public class ImageData
    {
        public string ImageFile { get; set; }
        //public List<(int, float, float, float, float)> bbox { get; set; }
        public List<ImageLabel> bbox { get; set; }
    }

    public class ImageLabel
    {
        public int LabelId { get; set; }
        public double CentX { get; set; }
        public double CentY { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public class Image_DataBase : BaseData<ImageData>
    {

        string trainpath = string.Empty;
        string validpath = string.Empty;
        string trainimagepath = string.Empty;
        string trainlabelpath = string.Empty;
        string validimagepath = string.Empty;
        string validlabelpath = string.Empty;

        private Image_DataBase()
        {

        }
        public Image_DataBase(string rootpath)
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
                        Labels.AddOrGetKey<int, string>(name);
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
                        TrainData.AddOrGetKey<int, ImageData>(image);
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
                        ValidData.AddOrGetKey<int, ImageData>(image);
                    }
                }
            }


        }

        public List<ImageLabel> ReadLabelFile(string filePath)
        {
            var result = new List<ImageLabel>();
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

                    result.Add(new ImageLabel() { LabelId = labelId, CentX = centerX, CentY = centerY, Width = width, Height = height });
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
            this.TrainData = yaml.TrainData;
            this.ValidData = yaml.ValidData;
            this._epochs = yaml._epochs;
            this._epochs_current = yaml._epochs_current;
            this._trainBatchSize = yaml._trainBatchSize;
            this._testBatchSize = yaml._testBatchSize;
            this._logInterval = yaml._logInterval;
            //this._numClasses = yaml._numClasses;
            this._timeout = yaml._timeout;
        }

        static public Image_DataBase LoadYAML(string yamlFile)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();

            using (var reader = new StreamReader(yamlFile))
            {

                var result = new Image_DataBase();
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
                        var labelId = int.Parse(label.Key.ToString());
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



                result.TrainData = ParseImageYAML("TrainImage", yaml);
                result.ValidData = ParseImageYAML("ValidImage", yaml);

                return result;
            }
        }

        static private Dictionary<int, ImageData> ParseImageYAML(string ImageType, Dictionary<string, object> yaml)
        {
            Dictionary<int, ImageData> result = new Dictionary<int, ImageData>();
            if (yaml.TryGetValue(ImageType, out var trainImageValue))
            {
                var ImageDict = trainImageValue as Dictionary<object, object>;
                foreach (var Image in ImageDict)
                {
                    var imageId = int.Parse(Image.Key.ToString());
                    var imageDataDict = (Dictionary<object, object>)Image.Value;
                    var imageData = new ImageData
                    {
                        ImageFile = imageDataDict["ImageFile"].ToString(),
                        bbox = new List<ImageLabel>()
                    };
                    if (imageDataDict.TryGetValue("bbox", out var bboxValue))
                    {
                        var bboxList = (List<object>)bboxValue;
                        foreach (var bboxObj in bboxList)
                        {
                            var bboxArray = (bboxObj as Dictionary<object, object>).ToArray();

                            var bbox = new ImageLabel()
                            {
                                LabelId = int.Parse(bboxArray[0].Value.ToString()),
                                CentX = double.Parse(bboxArray[1].Value.ToString()),
                                CentY = double.Parse(bboxArray[2].Value.ToString()),
                                Width = double.Parse(bboxArray[3].Value.ToString()),
                                Height = double.Parse(bboxArray[4].Value.ToString())
                            };
                            imageData.bbox.Add(bbox);
                        }
                    }
                    result.Add(imageId, imageData);
                }
            }
            return result;
        }

        public override Module<Tensor, Tensor> LoadModel(bool loadBestModel = false)
        {
            var TrainLabelCount = TrainData.Select(o => o.Value.bbox.Any() ? o.Value.bbox.Select(o => o.LabelId).Max() : 0).Max() + 1;
            var ValidLabelCount = ValidData.Select(o => o.Value.bbox.Any() ? o.Value.bbox.Select(o => o.LabelId).Max() : 0).Max() + 1;
            var LabelCount = Math.Max(TrainLabelCount, ValidLabelCount);

            Model = ImageTrain.torchvision.models.resnet34(num_classes: LabelCount, device: device);
            if (loadBestModel && File.Exists(bestModelPath))
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

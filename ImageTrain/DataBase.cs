using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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

    public class ImageData
    {
        public string ImageFile { get; set; }
        public List<(int, float, float, float, float)> bbox { get; set; }
    }

    public class YAMLdatabase
    {
        public string YAMLfile { get; set; }
        public Dictionary<long, string> Labels { get; set; } = new Dictionary<long, string>();
        public Dictionary<long, ImageData> TrainImage { get; set; } = new Dictionary<long, ImageData>();
        public Dictionary<long, ImageData> ValidImage { get; set; } = new Dictionary<long, ImageData>();
        public int _epochs { get; set; } = 200;
        public int _epochs_current { get; set; } = 0;
        public int _trainBatchSize { get; set; } = 4;

        public int _testBatchSize { get; set; } = 4;
        public int _logInterval { get; set; } = 25;
        public int _numClasses { get; set; } = 0;
        public int _timeout { get; set; } = 3600;    // One hour by default.

        public YAMLdatabase()
        {

        }
        public YAMLdatabase(string rootpath)
        {
            var trainpath = Path.Combine(rootpath, "train");
            var validpath = Path.Combine(rootpath, "valid");
            var trainimagepath = Path.Combine(trainpath, "images");
            var trainlabelpath = Path.Combine(trainpath, "labels");
            var validimagepath = Path.Combine(validpath, "images");
            var validlabelpath = Path.Combine(validpath, "labels");

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
                        StaticLib.Log("Error: YAML file is missing required properties.");
                        continue;
                    }

                    foreach (var name in yaml.names)
                    {
                        Labels.AddOrGetKey<long, string>(name);
                    }
                    _numClasses = Labels.Count();
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
            this._numClasses = yaml._numClasses;
            this._timeout = yaml._timeout;
        }

        public YAMLdatabase LoadYAML(string yamlFile)
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

                result.TrainImage = ParseImageYAML("TrainImage", yaml);
                result.ValidImage = ParseImageYAML("ValidImage", yaml);

                return result;
            }
        }

        private Dictionary<long,ImageData> ParseImageYAML(string ImageType, Dictionary<string, object> yaml)
        {
            Dictionary < long, ImageData> result = new Dictionary<long, ImageData>();
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
            

    }


}

using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Modules;
using static ImageTrain.StaticLib;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;

namespace ImageTrain
{
    internal class Detect
    {
        Image_DataBase _db;

        public Detect(Image_DataBase db)
        {
            _db = db;
            _db.LoadModel(true);
            _db.Model.eval();
        }


        public float[] FeaturesFromTensor(Tensor tensor)
        {
            // 将Tensor转换为模型的输入格式
            //tensor = tensor.view(1, 3, ImageLabel_Dataset.ImageSize.Height, ImageLabel_Dataset.ImageSize.Width);
            tensor = tensor.unsqueeze(0);
            tensor = tensor.to(_db.device);
            using (var noGrad = torch.no_grad())
            {
                // 在此上下文中执行的操作将不会计算梯度

                // 将Tensor输入到模型中并获取特征结果

                var Features = ((ImageTrain.Modules.ResNet)_db.Model).Features(tensor);

                // 返回特征数组        
                return Features.data<float>().ToArray();
            }

        }
        private float[] DetectFromTensor(Tensor tensor)
        {
            // 将Tensor转换为模型的输入格式
            //tensor = tensor.view(1, 3, ImageLabel_Dataset.ImageSize.Height, ImageLabel_Dataset.ImageSize.Width);
            tensor = tensor.unsqueeze(0);
            tensor = tensor.to(_db.device);
            using (var noGrad = torch.no_grad())
            {
                // 在此上下文中执行的操作将不会计算梯度

                // 将Tensor输入到模型中并获取预测结果
                var prediction = _db.Model.call(tensor);

                // 获取预测类别的概率
                var probabilities = prediction.softmax(1);

                //StaticLib.PrintData("probabilities", probabilities);

                // 返回概率数组        
                return probabilities.data<float>().ToArray();
            }

        }

        //public List<DetectTraget> GetDetectLabel(string file)
        //{
        //    List<DetectTraget> r = new List<DetectTraget>();
        //    var probability = DetectFromFile(file);
        //    //检查 probability 的长度是否为 5 的倍数
        //    if (probability.Length % 5 != 0)
        //    {
        //        throw new Exception("probability.Length % 5 != 0");
        //    }
        //    var keypointcount = probability.Length / 5;
        //    for (int i = 0; i < keypointcount; i++)
        //    {
        //        var keypoint = probability.Skip(i * 5).Take(5).ToList();
        //        var id = Convert.ToInt32(keypoint[0]);
        //        var bbox = keypoint.Skip(1).ToList();

        //        r.Add(new DetectTraget() { id = id, name = _db.Labels[id], cx = bbox[0], cy = bbox[1], width = bbox[2], height = bbox[3] });
        //    }
        //    return r;
        //}

        public List<DetectTraget> GetDetectLabel(string file)
        {
            List<DetectTraget> r = new List<DetectTraget>();
            var probability = DetectFromFile(file);

            for (int i = 0; i < _db.Labels.Count; i++)
            {
                if (probability[i] > 0.4)
                {
                    r.Add(new DetectTraget() { id =i, name =  _db.Labels[i] });
                }
            }
            r = r.OrderByDescending(o => o.id).ToList();
            return r;
        }

        public List<(int, double, string)> GetDetectLabel(Mat image)
        {
            List<(int, double, string)> r = new List<(int, double, string)>();
            var probability = DetectFromMat(image);
            for (int i = 0; i < _db.Labels.Count; i++)
            {
                if (probability[i] > 0.4)
                    r.Add((i, probability[i], _db.Labels[i]));
            }
            r = r.OrderByDescending(o => o.Item1).ToList();
            return r;
        }

        private float[] DetectFromFile(string file)
        {

            var tensor = StaticLib.GetTensorFromImageFile(file);
            var r = DetectFromTensor(tensor);

            return r;


        }
        private float[] DetectFromMat(Mat image)
        {
            image= StaticLib.Resize(image, 224, 224);
            var tensor = StaticLib.Mat2Tensor(image);
            var r = DetectFromTensor(tensor);
            return r;
        }

    }
}

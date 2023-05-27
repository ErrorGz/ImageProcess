using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;

namespace ImageTrain
{
    internal class Detect
    {
        YAMLdatabase _db;

        public Detect(YAMLdatabase db)
        {
            _db = db;
            _db.LoadModel();
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

                // 返回概率数组        
                return probabilities.data<float>().ToArray();
            }

        }

        public List<(int, double, string)> GetDetectLabel(string file)
        {
            List<(int, double, string)> r = new List<(int, double, string)>();
            var probability = DetectFromFile(file);
            for (int i = 0; i < _db.Labels.Count; i++)
            {
                if (probability[i] > 0.4)
                    r.Add((i, probability[i], _db.Labels[i]));
            }
            r = r.OrderByDescending(o => o.Item1).ToList();
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

            var tensor = ImageLabel_Dataset.GetTensorFromImageFile(file);
            var r = DetectFromTensor(tensor);
            return r;


        }
        private float[] DetectFromMat(Mat image)
        {
            var tensor = ImageLabel_Dataset.Mat2Tensor(image);
            var r = DetectFromTensor(tensor);
            return r;
        }

    }
}

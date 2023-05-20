using OpenCvSharp;
using TorchSharp;
using static TorchSharp.torch;



namespace ImageTrain
{
    class YOLOv5_Dataset : torch.utils.data.Dataset
    {
        Dictionary<long, Dictionary<string, Tensor>> _Caches = new Dictionary<long, Dictionary<string, Tensor>>();
        OpenCvSharp.Size ImageSize = new OpenCvSharp.Size(224, 224);
        public YOLOv5_Dataset( Dictionary<long, ImageData> Images,int LabelCount)
        {
            foreach (var imagedata in Images)
            {
                var cache = GetTensorData(imagedata.Value,LabelCount);
                _Caches.Add(imagedata.Key, cache);
            }
        }

        public void Dispose()
        {

            foreach (var cache in _Caches)
            {
                foreach (var t in cache.Value.Values)
                {
                    t.Dispose();
                }
                cache.Value.Clear();
            }
            _Caches.Clear();

        }

        public override long Count
        {
            get
            {
                return _Caches.Count();
            }
        }

        public override Dictionary<string, torch.Tensor> GetTensor(long index)
        {
            return _Caches[index];
        }

        private Dictionary<string, Tensor> GetTensorData(ImageData imagedata,int LabelCount)
        {
            var img = Cv2.ImRead(imagedata.ImageFile, ImreadModes.Color);

            // 如果读取失败，尝试使用不同的ImreadModes再次读取
            if (img.Empty())
            {
                img = Cv2.ImRead(imagedata.ImageFile, ImreadModes.Grayscale);
            }
            if (img.Empty())
            {
                img = Cv2.ImRead(imagedata.ImageFile, ImreadModes.AnyColor);
            }
            if (img.Empty())
            {
                // 如果所有的ImreadModes都失败了，抛出异常
                throw new Exception($"Failed to read image file {imagedata.ImageFile}");
            }
      
            img = img.Resize(ImageSize);

            var imgData = new byte[3 * img.Rows * img.Cols];

            Parallel.For(0, img.Rows, i =>
            {
                for (int j = 0; j < img.Cols; j++)
                {
                    int p = (i * img.Cols + j) * 3;
                    // 计算当前像素在imgData数组中的索引

                    imgData[p] = img.At<Vec3b>(i, j)[2];
                    imgData[p + 1] = img.At<Vec3b>(i, j)[1];
                    imgData[p + 2] = img.At<Vec3b>(i, j)[0];
                    // 将当前像素的RGB通道值存储到imgData数组中
                }
            });

            // 将imgData中的数据按照R、G、B的顺序排列
            var imgDataRGB = new byte[3 * img.Rows * img.Cols];
            Parallel.For(0, img.Rows * img.Cols, i =>
            {
                imgDataRGB[i] = imgData[i * 3 + 2]; // R
                imgDataRGB[i + img.Rows * img.Cols] = imgData[i * 3 + 1]; // G
                imgDataRGB[i + 2 * img.Rows * img.Cols] = imgData[i * 3]; // B
            });

            // 创建浮点型张量，并将图像数据转换为张量
            var imageTensor = torch.tensor(imgDataRGB, new long[] { 3, img.Rows, img.Cols })
                .to_type(torch.float32)
                .div(255f);


            // 创建张量
            //var labelTensor = torch.tensor(new float[LabelCount], dtype: ScalarType.Float32);
            ////将labelTensor设置默认0
            //labelTensor.zero_();

            var labelTensor=torch.zeros(LabelCount, dtype: float32);

            for (int i = 0; i < imagedata.bbox.Count; i++)
            {
                // 将bbox列表中的元素添加到张量中
                var (labelID, cx, cy, w, h) = imagedata.bbox[i];
                labelTensor[labelID] = 1;
            }
            var tensordata = new Dictionary<string, torch.Tensor> { { "data", imageTensor }, { "label", labelTensor } };
            return tensordata;
        }


    }


    public static class DictionaryExtensions
    {
        public static long AddOrGetKey<TKey, TValue>(this Dictionary<long, TValue> dict, TValue value)
        {
            long key = dict.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, value)).Key;
            if (key == default(long) && !dict.ContainsValue(value))
            {
                key = dict.Keys.Count > 0 ? dict.Keys.Max() + 1 : 0;  // Change 1 to 0 as the starting key
                dict.Add(key, value);
            }
            return key;
        }
    }

}

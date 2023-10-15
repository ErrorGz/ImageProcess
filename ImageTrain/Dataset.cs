using OpenCvSharp;
using TorchSharp;
using static TorchSharp.torch;
namespace ImageTrain
{

    class VideoLabel_Dataset : torch.utils.data.Dataset
    {
        Dictionary<int, Dictionary<string, Tensor>> _Caches = new Dictionary<int, Dictionary<string, Tensor>>();

        public VideoLabel_Dataset(Dictionary<int, VideoData> Videos)
        {
            var LabelCount = Videos.Select(o => o.Value.frames.Any() ? o.Value.frames.Select(o => o.VideoLabelId).Max() : 0).Max() + 1;
            foreach (var videodata in Videos)
            {
                var caches = GetTensorFromVideoData(videodata.Value, LabelCount);
                caches.ForEach(cache =>
                {
                    _Caches.Add(videodata.Key, cache);
                });

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
            return _Caches[(int)index];
        }

        private List<Dictionary<string, Tensor>> GetTensorFromVideoData(VideoData vds, int LabelCount)
        {
            List<Dictionary<string, Tensor>> r = new List<Dictionary<string, Tensor>>();
            VideoCapture capture = new VideoCapture(vds.VideoURL);

            foreach (var vd in vds.frames)
            {
                List<Tensor> image_list_tensor = new List<Tensor>();
                for (int f = vd.FrameStart; f <= vd.FrameStart + vd.FrameCount * vd.FrameStep; f += vd.FrameStep)
                {
                    capture.Set(VideoCaptureProperties.PosFrames, f);
                    var frame = new Mat();
                    capture.Read(frame);
                    if (frame.Empty() == false)
                    {
                        frame = StaticLib.Resize(frame, 244, 244);
                        var image_tensor = StaticLib.Mat2Tensor(frame);
                        image_list_tensor.Add(image_tensor);
                    }
                    else
                    {
                        //抛出异常
                        throw new Exception($"Failed to read image file {vds.VideoURL}");
                    }
                    frame.Dispose();
                    GC.Collect();
                }
                var imageTensor = torch.stack(image_list_tensor.ToArray(), 0);

                // 创建标签张量
                var labelTensor = torch.zeros(LabelCount, dtype: torch.float32);
                labelTensor[vd.VideoLabelId] = 1;

                var tensordata = new Dictionary<string, torch.Tensor>
                    {
                        { "data", imageTensor },
                        { "label", labelTensor },
                    };
                r.Add(tensordata);
            }
            //释放资源
            capture.Release();
            capture.Dispose();
            return r;
        }
    }

    class ImageLabel_Dataset : torch.utils.data.Dataset
    {
        Dictionary<int, Dictionary<string, Tensor>> _Caches = new Dictionary<int, Dictionary<string, Tensor>>();
        static public OpenCvSharp.Size ImageSize { get; set; } = new OpenCvSharp.Size(224, 224);
        public ImageLabel_Dataset(Dictionary<int, ImageData> Images)
        {
            var labelCount = Images.Select(o => o.Value.bbox.Any() ? o.Value.bbox.Select(p => p.LabelId).Max() : 0).Max() + 1;
            var bboxCount = Images.Select(o => o.Value.bbox.Any() ? o.Value.bbox.Count() : 0).Max() + 1;
            foreach (var imagedata in Images)
            {
                Console.WriteLine($"加载image图像文件：{imagedata.Value.ImageFile}");
                var cache = GetTensorFromImageData(imagedata.Value, labelCount, bboxCount);
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
            return _Caches[(int)index];
        }

        private Dictionary<string, Tensor> GetTensorFromImageData(ImageData imagedata, int LabelCount, int KeypointCount)
        {

            Tensor imageTensor = StaticLib.GetTensorFromImageFile(imagedata.ImageFile);


            // 创建标签张量
            var labelTensor = torch.zeros(LabelCount, dtype: torch.float32);
            //var keypointTensor = torch.zeros(KeypointCount * 5, dtype: torch.float32);

            var labelids = imagedata.bbox.ToList();
            foreach (var labelid in labelids)
            {
                labelTensor[labelid.LabelId] = 1;
            }
            //for (int i = 0; i < labelids.Count; i++)
            //{
            //    keypointTensor[i] = 1;
            //    keypointTensor[i + 1] = labelids[i].CentX;
            //    keypointTensor[i + 2] = labelids[i].CentY;
            //    keypointTensor[i + 3] = labelids[i].Width;
            //    keypointTensor[i + 4] = labelids[i].Height;

            //}

            var tensordata = new Dictionary<string, torch.Tensor>
            {
                { "data", imageTensor },
                { "label", labelTensor },
                //{ "keypoint", keypointTensor }
            };
            return tensordata;
        }



    }


    public static class DictionaryExtensions
    {
        public static int AddOrGetKey<TKey, TValue>(this Dictionary<int, TValue> dict, TValue value)
        {
            int key = dict.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, value)).Key;
            if (key == default(int) && !dict.ContainsValue(value))
            {
                key = dict.Keys.Count > 0 ? dict.Keys.Max() + 1 : 0;  // Change 1 to 0 as the starting key
                dict.Add(key, value);
            }
            return key;
        }
    }

}

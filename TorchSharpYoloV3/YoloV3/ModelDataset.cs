namespace TorchSharpYoloV3.YoloV3
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using static TorchSharp.torchvision;
    using static TorchSharp.torch;
    internal class ModelDataset : utils.data.Dataset
    {
        private static ImageHelper imageHandler = new ImageHelper();
        private ITransform transform;

        private List<Tensor> _data = new();
        private List<Tensor> _labels = new();

        public override long Count => _data.Count;
        public ModelDataset(string rootPath, string datasetName, int square, int channels, int max_objects, ITransform target_transform = null)
            : this(Path.Join(rootPath, datasetName), square, channels, max_objects, target_transform)
        {

        }
        protected ModelDataset(string datasetPath, int square, int channels, int max_objects, ITransform target_transform)
        {
            List<Tensor> images = new List<Tensor>();
            object lockObj = "locked";
            transform = target_transform;
            var dataPath = Path.Combine(datasetPath, "images");
            var labelPath = Path.Combine(datasetPath, "labels");
            dataPath = Path.GetFullPath(dataPath);
            //
            // Image Reader
            //
            // Get all files from the directory
            string[] files = Directory.GetFiles(dataPath);

            int count_log = 0;
            Parallel.ForEach(files,
                new ParallelOptions { MaxDegreeOfParallelism = 20 }
                , filePath =>
            {

                try
                {
                    Bitmap my_image = new Bitmap(filePath);
                    my_image = imageHandler.ResizeBitmap(my_image, square, square);
                    System.Drawing.Imaging.BitmapData bd =
                        my_image.LockBits(new Rectangle(0, 0, my_image.Width, my_image.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                    IntPtr ptr = bd.Scan0;
                    int bytes = Math.Abs(bd.Stride) * my_image.Height;
                    byte[] rgbBytes = new byte[bytes];
                    float[] rgbFloat = new float[square * square * channels];
                    Marshal.Copy(ptr, rgbBytes, 0, bytes);
                    my_image.UnlockBits(bd);
                    Tensor t = empty(square, square, channels);
                    foreach (int h in Enumerable.Range(0, square))
                    {
                        foreach (int w in Enumerable.Range(0, square))
                        {
                            int idx = h * square * channels + w * channels;
                            lock (lockObj)
                            {
                                rgbFloat[idx] = rgbBytes[h * w + w] / 256.0f;
                                if (channels > 1)
                                    rgbFloat[idx + 1] = rgbBytes[h * w + w + 1] / 256.0f;
                                if (channels > 2)
                                    rgbFloat[idx + 2] = rgbBytes[h * w + w + 2] / 256.0f;
                            }
                        }
                    }
                    t = (Tensor)rgbFloat;
                    t = t.reshape(square, square, channels);
                    t = t.permute(2, 0, 1);
                    images.Add(t);
                }
                finally
                {
                    Interlocked.Increment(ref count_log);
                    Console.Write("\r{0}", $"loading images: {count_log.ToString()}/{files.Length} ...");
                }
            });
            //
            // Label reader:
            //
            files = Directory.GetFiles(labelPath);
            List<Tensor> labels = new List<Tensor>();
            foreach (string filePath in files)
            {
                string[] oneImageLabels = File.ReadAllLines(filePath);
                Tensor t = zeros(max_objects, 5);
                foreach (string str in oneImageLabels)
                {
                    var id = Array.IndexOf(oneImageLabels, str);
                    string[] l = str.Trim().Split(' ');
                    t[id, 0] = tensor(float.Parse(l[0]));
                    t[id, 1] = tensor(float.Parse(l[1]));
                    t[id, 2] = tensor(float.Parse(l[2]));
                    t[id, 3] = tensor(float.Parse(l[3]));
                    t[id, 4] = tensor(float.Parse(l[4]));
                }
                labels.Add(t);
            }
            _data = images;
            _labels = labels;

            Console.WriteLine("dataset loaded");
        }


        public override Dictionary<string, Tensor> GetTensor(long index)
        {
            var rdic = new Dictionary<string, Tensor>();
            if (transform is not null)
                rdic.Add("data",
                    transform.forward(_data[(int)index].unsqueeze(0).unsqueeze(0)).squeeze(0));
            else
                rdic.Add("data", _data[(int)index]);

            rdic.Add("label", _labels[(int)index]);
            return rdic;
        }
        public override void Dispose()
        {
            _data.ForEach(d => d.Dispose());
            _labels.ForEach(d => d.Dispose());
        }
    }
}

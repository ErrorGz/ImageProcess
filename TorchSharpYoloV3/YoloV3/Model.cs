namespace TorchSharpYoloV3.YoloV3
{
    using TorchSharp;
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;
    internal partial class Model : Module
    {
        private int num_classes;
        private int num_anchors;
        private bool jump_start = true;
        private Tensor[] samples=new Tensor[] { };
        private Device _device;
        public Model(string path, int num_classes, int channels, int image_size, int anchors_per_scale)
            : this(num_classes, channels, image_size, anchors_per_scale)
        {
            this.load(path);
            this.to(_device);
        }
        public Model(int num_classes, int channels, int image_size, int anchors_per_scale) : base("YoloV3_full")
        {
            this.num_classes = num_classes;
            this.num_anchors = anchors_per_scale;
            RegisterComponents();
            _device = torch.cuda.is_available() ? torch.CUDA : torch.CPU;

            Tensor starter = torch.randn(1, channels, image_size, image_size);
            this.forward(starter);
            jump_start = false;
        }
        public Device device() {
            return _device;
        }
        public int[] GetGrids() {
            List<int> grid = new List<int>();
            foreach (Tensor t in samples) {
                grid.Add((int)t.shape[2]);
            }        
            return grid.ToArray();
        }
    }
}

namespace TorchSharpYoloV3
{
    using TorchSharp;
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;
    using static TorchSharp.torch.utils.data;
    using YoloV3;
    using static Utils;
    using System.Linq;

    public partial class ModelHandler
    {
        private Model _model;
        private static int _num_classes;
        private static int _channels;
        private static int _image_size;
        private static int _anchors_per_scale;
        private static int _max_objects = 50;
        private static string _root_path = "";
        private static string _dataset_name = "";
        public ModelHandler(int num_classes, int channels, int image_size, int anchors_per_scale)
        {
            _num_classes = num_classes;
            _channels = channels;
            _image_size = image_size;
            _anchors_per_scale = anchors_per_scale;
            _model = new Model(_num_classes, _channels, _image_size, anchors_per_scale);
            _model.to(_model.device());
        }
        public void SaveModel(string path)
        {
            _model.save(path);
        }

        public void CreateNewloadedModel(string path)
        {
            _model.Dispose();
            _model = new Model(path, _num_classes, _channels, _image_size, _anchors_per_scale);
        }
        public Module model() {
            return _model;
        }
        public DataLoader data_loader(string root, string dataset_name, int batch_size, bool shuffle = false)
        {
            _root_path = root;
            _dataset_name = dataset_name;

            ModelDataset ds = dataset(root, dataset_name);
            return new DataLoader(
                dataset: ds,
                batchSize: batch_size,
                shuffle: shuffle,
                device: _model.device());
        }
        private static ModelDataset dataset(string root, string dataset_name) {
            return new ModelDataset(
                root
                , dataset_name
                , _image_size
                , _channels
                , _max_objects);
        }
    }
}

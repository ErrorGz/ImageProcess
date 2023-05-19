namespace TorchSharpYoloV3
{
    using System.Drawing;

    using TorchSharp;
    using static TorchSharp.torch.nn;
    using static TorchSharp.torch.utils.data;
    internal class Program
    {
        
        static void Main(string[] args)
        {
            torch.random.manual_seed(4711);
            torch.cuda.manual_seed(4711);

            ModelHandler mh 
                = new ModelHandler(
                    num_classes: 80, 
                    channels: 3, 
                    image_size: 416, 
                    anchors_per_scale: 3);

            // 3 anhors per scale (w,h) and 3 scales
            float[] ANCHORS = new float[] {
                0.28f, 0.22f, 0.38f, 0.48f, 0.9f, 0.78f,
                0.07f, 0.15f, 0.15f, 0.11f, 0.14f, 0.29f,
                0.02f, 0.03f, 0.04f, 0.07f, 0.08f, 0.06f };

            string root_path = "data";
            string data_set_name = "Dataset1";
            string model_weights = "weigths.dat";
            int batch_size = 12;
            var epochs = 5000;


            Console.WriteLine("Loading data...");
            DataLoader dataloader = mh.data_loader(root_path, data_set_name, batch_size, false);

            Console.WriteLine("Loading model");
            mh.CreateNewloadedModel(Path.Join(root_path, data_set_name, model_weights));


            Console.WriteLine("Training...");
            var lr = mh.train(epochs: epochs, data: dataloader, anchors: ANCHORS, base_epoch: 0, learning_rate: 0.000002f);


            Console.WriteLine("Evaluating...");
            mh.evaluate(dataloader, ANCHORS);


            Console.WriteLine("Detection...");
            Image img = mh.detect(Path.Join(root_path, data_set_name, "images", "000000000241.jpg"), ANCHORS);
            img.Save(Path.Join(root_path, data_set_name, "detections", "241_detect.jpg"));


            Console.WriteLine("Saving model");
            mh.SaveModel(Path.Join(root_path, data_set_name, model_weights));


            Console.ReadLine();
        }
    }
}
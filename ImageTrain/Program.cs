using System;
using System.Diagnostics;
using System.Reflection;
using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;
using static TorchSharp.torch.nn.functional;
using DataModels;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;
using System.Threading;
using NumSharp;
using Tensorboard;
using Org.BouncyCastle.Asn1.Tsp;

namespace ImageTrain
{
    internal class Program
    {
        static void Main(string[] args)
        {

            var tensorboard = torch.utils.tensorboard.SummaryWriter();
          
            YAMLdatabase db = new YAMLdatabase(@"DataSet\animals.v2-release.yolov5pytorch");
            db.Save("config.yaml");
            //YAMLdatabase db2 = new YAMLdatabase();
            //db2.Load("config.yaml");


            var device = torch.cuda.is_available() ? torch.CUDA : torch.CPU;
            var model = torchvision.models.resnet34(num_classes: db.Labels.Count, device: device);
            //torch.nn.LSTM()
            if (File.Exists("best.pt"))
                model.load("best.pt");

            using YOLOv5_Dataset train_data = new YOLOv5_Dataset(db.Labels, db.TrainImage);
            using YOLOv5_Dataset test_data = new YOLOv5_Dataset(db.Labels, db.ValidImage);
            using var train = new DataLoader(train_data, db._trainBatchSize, device: device, num_worker: 4, shuffle: true);
            using var test = new DataLoader(test_data, db._testBatchSize, device: device, num_worker: 4, shuffle: false);

            using (var optimizer = torch.optim.Adam(model.parameters(), 0.001))
            {
                var bestAccuracy = 0.0;
                var bestModelPath = "best.pt";
                var lastModelPath = "last.pt";

                Stopwatch totalSW = new Stopwatch();
                totalSW.Start();

                for (var epoch = 1; epoch <= db._epochs; epoch++)
                {
                    Stopwatch epchSW = new Stopwatch();
                    epchSW.Start();


                    StaticLib.Log($"epoch: {epoch}...");
                    var accuracy_train = TrainOrTest(model, optimizer, CrossEntropyLoss(), train, tensorboard, epoch, true);

                    if (epoch % 10 == 0)
                    {
                        var accuracy_test = TrainOrTest(model, optimizer, CrossEntropyLoss(), test, tensorboard, epoch, false);
                    }

                    // Save the model at each epoch
                    model.save(lastModelPath);

                    StaticLib.Log($"Save Last Model:{lastModelPath}");
                    if (accuracy_train > bestAccuracy)
                    {
                        bestAccuracy = accuracy_train;
                        // Save the best model                        
                        model.save(bestModelPath);

                        StaticLib.Log($"Save Best Model:{bestModelPath}");
                    }

                    epchSW.Stop();
                    StaticLib.Log($"Elapsed time for this epoch: {epchSW.Elapsed.TotalSeconds} s.");

                    //if (totalSW.Elapsed.TotalSeconds > db._timeout) break;
                }

                totalSW.Stop();
                StaticLib.Log($"Elapsed training time: {totalSW.Elapsed} s.");
            }


            model.Dispose();

        }


        private static double TrainOrTest(
            Module<torch.Tensor, torch.Tensor> model,
            torch.optim.Optimizer optimizer,
            Loss<torch.Tensor, torch.Tensor, torch.Tensor> loss,
            DataLoader dataLoader,
            SummaryWriter tensorboard,
            int epoch,
            bool isTraining)
        {
            if (isTraining)
            {
                model.train();
            }
            else
            {
                model.eval();
            }

            long total = 0;
            double totalLoss = 0.0f;
            double correct = 0.0f;
            double accuracy = 0.0f;

            string mode = isTraining ? "Train" : "Test";

            using (var d = torch.NewDisposeScope())
            {
                foreach (var data in dataLoader)
                {
                    optimizer.zero_grad();

                    var target = data["label"];
                    var image = data["data"];

                    var prediction = model.call(image);
                    var lsm = log_softmax(prediction, 1);
                    var output = loss.call(lsm, target);

                    if (isTraining)
                    {
                        output.backward();
                        optimizer.step();
                    }
                    var batch_count = target.shape[0];
                    total += batch_count;
                    var 推理张量1维最大值 = prediction.argmax(1);
                    var 目标张量1维最大值 = target.argmax(1);
                    var pb = 推理张量1维最大值.data<long>().ToList();
                    var tb = 目标张量1维最大值.data<long>().ToList();
                    var pt_list = pb.Zip(tb, (p, t) => (p, t)).Select(o => new string($"[{o.p},{o.t}]"));
                    var pt_msg=string.Join(",", pt_list);
                   
                    var batch_correct= 推理张量1维最大值.eq(目标张量1维最大值).sum().ToInt64();
                    correct += batch_correct;
                    var batch_loss = output.ToSingle();
                    totalLoss += batch_loss;
                    
                    StaticLib.Log($"[{mode}] batch_data:{pt_msg}\t batch_loss:{batch_loss}/{batch_count}\t batch_correct:{batch_correct}/{batch_count}");

                    d.DisposeEverything();
                }
            }

            if (total == 0)
            {
                var msg = "DataSet.Count()==0";
                StaticLib.Log(msg);
                throw new Exception(msg);
            }
            accuracy = correct / total;
            var avgLoss = totalLoss / total;

            StaticLib.Log($"\r{mode}: Average loss {avgLoss.ToString("0.0000")} | Accuracy {accuracy.ToString("0.0000")}");
            if (isTraining)
            {
                tensorboard.add_scalar("accuracy", (float)accuracy, epoch);
                tensorboard.add_scalar("lost", (float)avgLoss, epoch);
            }
            else
            {
                tensorboard.add_scalar("val_accuracy", (float)accuracy, epoch);
                tensorboard.add_scalar("val_lost", (float)avgLoss, epoch);
            }

            return accuracy;

        }


    }


}
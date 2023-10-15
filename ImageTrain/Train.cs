using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.utils.data;
using TorchSharp.Modules;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.nn;
using static TorchSharp.torch.nn.functional;
using static TorchSharp.torch.utils;
using static TorchSharp.torch.utils.data;
using System.Globalization;
using System.Net;

namespace ImageTrain
{
    internal class Train
    {

        static public void DoTrain(Video_DataBase db)
        {
            string path = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var log_dir = Path.Combine("runs", path);
            var tensorboard = torch.utils.tensorboard.SummaryWriter(log_dir);
            Console.WriteLine($"新建记录文件夹：{tensorboard.LogDir}");

            using (var model = db.LoadModel(true))
            using (var train_data = new VideoLabel_Dataset(db.TrainData))
            using (var test_data = new VideoLabel_Dataset(db.ValidData))
            using (var train = new DataLoader(train_data, db._trainBatchSize.Value, device: db.device, num_worker: db._numWorker.Value, shuffle: true))
            using (var test = new DataLoader(test_data, db._testBatchSize.Value, device: db.device, num_worker: db._numWorker.Value, shuffle: false))
            using (var optimizer = torch.optim.Adam(model.parameters(), 0.001))
            {
                var bestAccuracy = 0.0;


                Stopwatch totalSW = new Stopwatch();
                totalSW.Start();

                for (var epoch = 1; epoch <= db._epochs; epoch++)
                {
                    Stopwatch epchSW = new Stopwatch();
                    epchSW.Start();


                    StaticLib.Log($"epoch: {epoch}...");
                    var accuracy_train = LoopEpch(model, optimizer, train, tensorboard, epoch, true);

                    if (epoch % 10 == 0)
                    {
                        var accuracy_test = LoopEpch(model, optimizer, test, tensorboard, epoch, false);
                    }

                    // Save the model at each epoch
                    model.save(db.lastModelPath);

                    StaticLib.Log($"Save Last Model:{db.lastModelPath}");
                    if (accuracy_train >= bestAccuracy)
                    {
                        bestAccuracy = accuracy_train;
                        // Save the best model                        
                        model.save(db.bestModelPath);

                        StaticLib.Log($"Save Best Model:{db.bestModelPath}");
                    }

                    epchSW.Stop();
                    StaticLib.Log($"Elapsed time for this epoch: {epchSW.Elapsed.TotalSeconds} s.");

                    //if (totalSW.Elapsed.TotalSeconds > db._timeout) break;
                }

                totalSW.Stop();
                StaticLib.Log($"Elapsed training time: {totalSW.Elapsed} s.");
            }

        }
        static public void DoTrain(Image_DataBase db)
        {

            string path = DateTime.Now.ToString("yyyyMMdd-HHmmss");
            var log_dir = Path.Combine("runs", path);
            var tensorboard = torch.utils.tensorboard.SummaryWriter(log_dir);
            Console.WriteLine($"新建记录文件夹：{tensorboard.LogDir}");       


            using (var model = db.LoadModel(true))
            using (var train_data = new ImageLabel_Dataset(db.TrainData))
            using (var test_data = new ImageLabel_Dataset(db.ValidData))
            using (var train = new DataLoader(train_data, db._trainBatchSize.Value, device: db.device, num_worker: db._numWorker.Value, shuffle: true))
            using (var test = new DataLoader(test_data, db._testBatchSize.Value, device: db.device, num_worker: db._numWorker.Value, shuffle: false))
            using (var optimizer = torch.optim.Adam(model.parameters(), 0.001))
            {
                var bestAccuracy = 0.0;


                Stopwatch totalSW = new Stopwatch();
                totalSW.Start();

                for (var epoch = 1; epoch <= db._epochs; epoch++)
                {
                    Stopwatch epchSW = new Stopwatch();
                    epchSW.Start();


                    StaticLib.Log($"epoch: {epoch}...");
                    var accuracy_train = LoopEpch(model, optimizer, train, tensorboard, epoch, true);

                    if (epoch % 10 == 0)
                    {
                        var accuracy_test = LoopEpch(model, optimizer, test, tensorboard, epoch, false);
                    }

                    // Save the model at each epoch
                    model.save(db.lastModelPath);

                    StaticLib.Log($"Save Last Model:{db.lastModelPath}");
                    if (accuracy_train >= bestAccuracy)
                    {
                        bestAccuracy = accuracy_train;
                        // Save the best model                        
                        model.save(db.bestModelPath);

                        StaticLib.Log($"Save Best Model:{db.bestModelPath}");
                    }

                    epchSW.Stop();
                    StaticLib.Log($"Elapsed time for this epoch: {epchSW.Elapsed.TotalSeconds} s.");

                    //if (totalSW.Elapsed.TotalSeconds > db._timeout) break;
                }

                totalSW.Stop();
                StaticLib.Log($"Elapsed training time: {totalSW.Elapsed} s.");
            }

        }

     
        private static double LoopEpch(
            Module<torch.Tensor, torch.Tensor> model,
            torch.optim.Optimizer optimizer,
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
            double total_loss = 0.0f;
            double total_correct = 0.0f;
            double accuracy = 0.0f;

            string mode = isTraining ? "Train" : "Test";

            using (var d = torch.NewDisposeScope())
            {
                foreach (var data in dataLoader)
                {
                    optimizer.zero_grad();

                    var image = data["data"];
                    var label = data["label"];
                    var keypoint = data["keypoint"];

                    var prediction = model.call(image);
                    //PrintData("prediction", prediction);

                    var lsm = log_softmax(prediction, 1);
                    //PrintData("lsm", lsm);

                    var output_label = cross_entropy(lsm, label);
                    //PrintData("output_label", output_label);

                    //var output_keypoint = mse_loss(lsm, keypoint);
                    //PrintData("output_keypoint", output_keypoint);

                    //var output = output_label + output_keypoint;
                    var output = output_label;
                    if (isTraining)
                    {
                        output.backward();
                        optimizer.step();
                    }
                    var batch_count = label.shape[0];
                    var label_count = label.shape[1];

                    total += batch_count;

                    var batch_loss = output.ToSingle();
                    total_loss += batch_loss;

                    double batch_correct = 0;
                    List<(int, int)> pt_data = new List<(int, int)>();

                    for (int b = 0; b < batch_count; b++)
                    {
                        var batch_prediction = prediction[b];
                        var batch_target = label[b];

                        var t2 = batch_target.data<float>().ToList().Where(o => o >= 1).Count();

                        for (int i = 0; i < label_count; i++)
                        {
                            var label_prediction = batch_prediction[i];
                            var label_target = batch_target[i];

                            (int, int) pt = (0, 0);

                            pt.Item1 = label_prediction.ToSingle() > 0.5 ? 1 : 0;
                            pt.Item2 = label_target.ToSingle() == 1 ? 1 : 0;
                            pt_data.Add(pt);
                            if (pt.Item1 == pt.Item2)
                                batch_correct += 1;

                        }
                    }



                    total_correct += batch_correct / label_count;
                    var pt_msg = string.Join(",", pt_data);
                    //StaticLib.Log($"[{mode}] batch_data:{pt_msg}");
                    StaticLib.Log($"batch_loss:{batch_loss}/{batch_count * label_count}\t batch_correct:{batch_correct}/{batch_count * label_count}");

                    d.DisposeEverything();
                }
            }

            if (total == 0)
            {
                var msg = "DataSet.Count()==0";
                StaticLib.Log(msg);
                throw new Exception(msg);
            }
            accuracy = total_correct / total;
            var avgLoss = total_loss / total;

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

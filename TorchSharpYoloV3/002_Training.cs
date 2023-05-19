namespace TorchSharpYoloV3
{
    using TorchSharp;
    using TorchSharpYoloV3.YoloV3;
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;
    using static TorchSharp.torch.utils.data;
    using static Utils;

    public partial class ModelHandler
    {
        public double train(int epochs, int base_epoch, DataLoader data, float[] anchors, float learning_rate = 0.0001f)
        {
            _model.train();
            Tensor all_anchors = anchors;
            if (all_anchors.shape[0] < 2)
                throw new Exception("invalid number of anchors");
            all_anchors = all_anchors.reshape(-1, 2);
            Tensor[] scaled_anchor
                = GetScaledAnchors(
                    anchors: all_anchors,
                    scales: _model.GetGrids(),
                    device: _model.device());
            var optimizer = torch.optim.SGD(_model.parameters(), learningRate: learning_rate);
            var scheduler = torch.optim.lr_scheduler.StepLR(optimizer, 25, 0.95);
            var iterater = data.GetEnumerator();
            int runs = data.Count();
            foreach (int epoch in Enumerable.Range(base_epoch, epochs))
            {
                foreach (int batch_id in Enumerable.Range(0, runs))
                {
                    iterater.MoveNext();
                    var x = iterater.Current["data"];
                    var y = iterater.Current["label"];
                    var batch_size = (int)(x.shape[0]);

                    using (var d = torch.NewDisposeScope())
                    {
                        optimizer.zero_grad();
                        // ========================================
                        // Predict
                        // ========================================
                        Tensor output = _model.forward(x);
                        // unpack prediction
                        Tensor[] predictions
                            = GetUnpackPredictions(
                                output: output,
                                scaled_anchor: scaled_anchor,
                                num_scales: _model.GetGrids().Length,
                                batch_size: batch_size,
                                grid_size: _model.GetGrids(),
                                num_classes: _num_classes);
                        // construct target per scale
                        Tensor[] target
                            = GetTransformedTargets(
                                target: y,
                                unscaled_anchors: all_anchors,
                                scales: _model.GetGrids(),
                                device: _model.device());
                        // ========================================
                        // Caltulate Loss
                        // ========================================
                        // calculate first scale to initialize losses
                        var losses = ModelLoss.GetLossPerScale(predictions[0], target[0], scaled_anchor[0]);
                        // calculate the rest of scales losses
                        foreach (int i in Enumerable.Range(1, _model.GetGrids().Length - 1))
                        {
                            losses = losses + ModelLoss.GetLossPerScale(predictions[i], target[i], scaled_anchor[i]);
                        }
                        // ========================================
                        // Run Optimization
                        // ========================================
                        losses.backward();
                        optimizer.step();
                        // ========================================
                        // Report intermediate results
                        // and maybe save model
                        // ========================================
                        if (batch_id == runs - 1)
                        {
                            if ((epoch % 100) == 0)
                            {
                                Console.WriteLine($"epoch: {epoch}: tensors: {torch.Tensor.TotalCount} loss: {(float)losses}");
                            }
                        }
                    }
                }
                scheduler.step();
                iterater.Reset();
            }
            return optimizer.ParamGroups.First().LearningRate;
        }
    }
}

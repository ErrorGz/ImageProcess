namespace TorchSharpYoloV3.YoloV3
{
    using TorchSharp;
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;
    internal class ModelLoss
    {
        static Loss loss_bce = nn.functional.binary_cross_entropy_with_logits_loss();
        static Loss loss_mse = nn.functional.mse_loss(Reduction.Mean);
        static Loss loss_ce = nn.functional.cross_entropy_loss();
        public static Tensor GetLossPerScale(Tensor p, Tensor t, Tensor anchors) {
            var num_of_anchors = anchors.shape[0];
            var ancs = anchors.reshape(1, num_of_anchors, 1, 1, 2);
            var obj = t[TensorIndex.Ellipsis, 0] == 1;
            var noobj = t[TensorIndex.Ellipsis, 0] == 0;
            // -------------------------------------------------------------------
            // No object loss
            // -------------------------------------------------------------------
            using (var no_obj_loss = loss_bce(
                    p[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)][noobj], 
                    t[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)][noobj]
                )) {
                var box_pred
                    = torch.cat(new List<Tensor>()
                    {
                        sigmoid(p[TensorIndex.Ellipsis, TensorIndex.Slice(1, 3)]),
                        exp(p[TensorIndex.Ellipsis, TensorIndex.Slice(3, 5)]) * ancs
                    }, -1);

                // find IOU
                var ious = Utils.intersection_over_union(
                    box_pred[obj], 
                    t[TensorIndex.Ellipsis, TensorIndex.Slice(1, 5)][obj], 
                    Utils.BoxFormat.midpoint);
                // ---------------------------------------------------------------
                // Object Loss
                // ---------------------------------------------------------------
                using (var obj_loss = loss_mse(
                        p[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)][obj],
                        (ious * t[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)][obj])
                    )) {

                    p[TensorIndex.Ellipsis, TensorIndex.Slice(1, 3)]
                        = sigmoid(p[TensorIndex.Ellipsis, TensorIndex.Slice(1, 3)]);

                    t[TensorIndex.Ellipsis, TensorIndex.Slice(3, 5)]
                        = log(t[TensorIndex.Ellipsis, TensorIndex.Slice(3, 5)] / ancs + 1e-16);
                    // -----------------------------------------------------------
                    // Box Loss
                    // -----------------------------------------------------------
                    using (var box_loss = loss_mse(
                        p[TensorIndex.Ellipsis, TensorIndex.Slice(1, 5)][obj],
                        t[TensorIndex.Ellipsis, TensorIndex.Slice(1, 5)][obj]
                        )) {
                        // -------------------------------------------------------
                        // Class Loss
                        // -------------------------------------------------------
                        using (var class_loss = loss_ce(
                            p[TensorIndex.Ellipsis, TensorIndex.Slice(5, null)][obj],
                            (t[TensorIndex.Ellipsis, 5][obj]).to_type(ScalarType.Int64)
                            )) {
                            return box_loss * 10 + obj_loss * 1 + no_obj_loss * 10 + class_loss * 1;
                        }
                    }
                }
            }
        }
    }
}

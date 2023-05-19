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

        public void evaluate(DataLoader dataLoader, float[] anchors)
        {
            _model.eval();
            int[] scales = _model.GetGrids();
            int num_of_scales = scales.Length;
            float nms_iou_threshold = 0.45f;
            float map_iou_threshold = 0.5f;
            float conf_threshold = 0.05f;

            Tensor all_anchors = anchors;
            if (all_anchors.shape[0] < 2)
                throw new Exception("invalid number of anchors");
            all_anchors = all_anchors.reshape(-1, 2);
            Tensor[] scaled_anchor
                = GetScaledAnchors(
                    anchors: all_anchors,
                    scales: scales,
                    device: _model.device());
            var iterater = dataLoader.GetEnumerator();
            var train_idx = 0;
            //-----------------------------------------------------------------------------
            // Calculate Evaluation boxes (all_preds,all_labls)
            //-----------------------------------------------------------------------------
            List<Tensor> all_preds = new List<Tensor>();
            List<Tensor> all_labls = new List<Tensor>();
            //-----------------------------------------------------------------------------
            // Calculate Class accuracy
            //-----------------------------------------------------------------------------
            int total_predicted_classes = 0;
            int correct_classes = 0;
            int total_predicted_no_obj = 0;
            int correct_no_obj = 0;
            int total_predicted_obj = 0;
            int correct_obj = 0;



            #region Evaluation Boxes, and class accuracy
            int transaction_id = 0;
            foreach (int batch_id in Enumerable.Range(0, dataLoader.Count()))
            {
                using (var d = torch.NewDisposeScope())
                {
                    iterater.MoveNext();
                    var x = iterater.Current["data"];
                    var y = iterater.Current["label"];
                    var batch_size = (int)(x.shape[0]);
                    // ========================================
                    // Predict
                    // ========================================
                    torch.no_grad();
                    Tensor output = _model.forward(x);
                    // unpack prediction
                    Tensor[] predictions
                            = GetUnpackPredictions(
                                output: output,
                                scaled_anchor: scaled_anchor,
                                num_scales: num_of_scales,
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
                    //=========================
                    // Evaluation boxes
                    //=========================
                    List<Tensor> bboxes_all_scales = new List<Tensor>();
                    long[] total_boxes = new long[num_of_scales];
                    foreach (int scale_id in Enumerable.Range(0, num_of_scales))
                    {
                        var S = scales[scale_id];
                        var boxes_of_scale
                            = GetPredBoxesPerScale(
                                predictions[scale_id],
                                scales[scale_id],
                                scaled_anchor[scale_id]);
                        total_boxes[scale_id] = boxes_of_scale.shape[1];
                        bboxes_all_scales.Add(boxes_of_scale);
                    }
                    Tensor bboxes = torch.cat(bboxes_all_scales, 1);
                    Tensor tbboxes
                        = GetTargetBoxesPerScale(
                            target[num_of_scales - 1],
                            scales[num_of_scales - 1]);
                    List<Tensor> nmx_boxes = new List<Tensor>();
                    List<Tensor> true_boxes = new List<Tensor>();
                    foreach (int batch_line in Enumerable.Range(0, batch_size))
                    {
                        // Prediction(nmx) boxes
                        Tensor nms = NonMaxSuppression(
                                        bounding_boxes: bboxes[batch_line],
                                        prob_threshold: conf_threshold,
                                        iou_threshold: nms_iou_threshold,
                                        box_format: BoxFormat.midpoint);
                        Tensor tsample_id = torch.full(new long[] { nms.shape[0], 1 }, transaction_id).to(bboxes.device);

                        nmx_boxes.Add(cat(new List<Tensor>() { tsample_id, nms }, 1));
                        
                        // Target with only true boxes
                        Tensor boxes_of_batch_line = tbboxes[batch_line];
                        Tensor true_mask = boxes_of_batch_line[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)] == 1;
                        true_mask = true_mask.repeat(new long[] { 1, 6 });
                        Tensor true_boxes_in_batch_line = (boxes_of_batch_line[true_mask]).reshape(-1, 6);
                        Tensor tsample_id_1 = torch.full(new long[] { true_boxes_in_batch_line.shape[0], 1 }, transaction_id).to(tbboxes.device);
                        true_boxes.Add(cat(new List<Tensor>() { tsample_id_1, true_boxes_in_batch_line }, 1));
                        transaction_id++;
                    }
                    all_preds.Add(d.MoveToOuter(cat(nmx_boxes, 0)));
                    all_labls.Add(d.MoveToOuter(cat(true_boxes, 0)));
                    //=========================
                    // Class accuracy
                    //=========================
                    foreach (int scale_id in Enumerable.Range(0, num_of_scales))
                    {
                        var obj = target[scale_id][TensorIndex.Ellipsis, 0] == 1;
                        var no_obj = target[scale_id][TensorIndex.Ellipsis, 0] == 1;
                        correct_classes +=
                            (int)sum(
                                    argmax(predictions[scale_id][TensorIndex.Ellipsis, TensorIndex.Slice(5, null)][obj], dim:-1) 
                                    == target[scale_id][TensorIndex.Ellipsis, 5][obj]);

                        total_predicted_classes += (int)sum(obj);

                        var obj_threshold = sigmoid(predictions[scale_id][TensorIndex.Ellipsis, 0]) > conf_threshold;

                        correct_obj +=
                            (int)sum(obj_threshold[obj] == target[scale_id][TensorIndex.Ellipsis, 0][obj]);

                        total_predicted_obj += (int)sum(obj);


                        correct_no_obj +=
                            (int)sum(obj_threshold[no_obj] == target[scale_id][TensorIndex.Ellipsis, 0][no_obj]);

                        total_predicted_no_obj += (int)sum(no_obj);
                    }
                }
            }
            #endregion
            //-----------------------------------------------------------------------------
            //-----------------------------------------------------------------------------
            //-----------------------------------------------------------------------------
            var ca = (correct_classes / (total_predicted_classes + 1e16)) * 100f;
            var noba = (correct_no_obj / (total_predicted_no_obj + 1e16)) * 100f;
            var obja = (correct_obj / (total_predicted_obj + 1e16)) * 100f;

            float mAP =
            MeanAveragePrecision(
                cat(all_preds, 0)
                , cat(all_labls, 0)
                , _num_classes
                , map_iou_threshold, BoxFormat.midpoint);
            Console.WriteLine($"Class accuracy: {ca} No obj accuracy: {noba} Object accuracy: {obja} mAP: {mAP}");
        }
    }
}

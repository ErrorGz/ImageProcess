namespace TorchSharpYoloV3
{
    using TorchSharp;
    using static TorchSharp.torch;
    using static TorchSharp.torch.utils;
    internal static class Utils
    {
        // Intersection Over Union
        public enum BoxFormat
        {
            midpoint,
            corners
        }
        /// <summary>
        /// Intersection over width and height union
        /// </summary>
        /// <param name="box1">[N,w,h]</param>
        /// <param name="box2">[N,w,h]</param>
        /// <returns>Tensor with iou values</returns>
        public static Tensor iou_width_height(Tensor box1, Tensor box2)
        {
            Tensor intersect;

            intersect =
                torch.minimum(
                    box1[TensorIndex.Ellipsis,0],
                    box2[TensorIndex.Ellipsis, 0])
                *
                torch.minimum(
                    box1[TensorIndex.Ellipsis, 1],
                    box2[TensorIndex.Ellipsis, 1]);


            Tensor union = (
                box1[TensorIndex.Ellipsis, 0] * box1[TensorIndex.Ellipsis, 1]
                +
                box2[TensorIndex.Ellipsis, 0] * box2[TensorIndex.Ellipsis, 1]
                -
                intersect
                );

            return intersect / union;

        }
        /// <summary>
        /// Intersection over union
        /// </summary>
        /// <param name="box1">[N,x,y,w,h]</param>
        /// <param name="box2">[N,x,y,w,h]</param>
        /// <param name="box_format">midpoint/corners if boxes (x,y,w,h) or (x1,y1,x2,y2)</param>
        /// <returns>Tensor with iou values</returns>
        public static Tensor intersection_over_union(Tensor box1, Tensor box2, BoxFormat box_format = BoxFormat.midpoint)
        {
            Tensor box1_x1, box1_y1, box1_x2, box1_y2, box2_x1, box2_y1, box2_x2, box2_y2;
            if (box_format == BoxFormat.midpoint)
            {   
                box1_x1 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)]
                                - box1[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)] / 2;
                box1_y1 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)]
                                - box1[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)] / 2;
                box1_x2 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)]
                                + box1[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)] / 2;
                box1_y2 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)]
                                + box1[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)] / 2;
                box2_x1 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)]
                                - box2[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)] / 2;
                box2_y1 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)]
                                - box2[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)] / 2;
                box2_x2 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)]
                                + box2[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)] / 2;
                box2_y2 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)]
                                + box2[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)] / 2;
            }
            else // corners
            {
                box1_x1 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)];
                box1_y1 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)];
                box1_x2 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)];
                box1_y2 = box1[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)];

                box2_x1 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)];
                box2_y1 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)];
                box2_x2 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(2, 3)];
                box2_y2 = box2[TensorIndex.Ellipsis, TensorIndex.Slice(3, 4)];
            }
            Tensor x1 = torch.maximum(box1_x1, box2_x1);
            Tensor y1 = torch.maximum(box1_y1, box2_y1);
            Tensor x2 = torch.minimum(box1_x2, box2_x2);
            Tensor y2 = torch.minimum(box1_y2, box2_y2);
            Tensor intersection = (x2 - x1).clamp(0) * (y2 - y1).clamp(0);
            Tensor box1_area = torch.abs((box1_x2 - box1_x1) * (box1_y2 - box1_y1));
            Tensor box2_area = torch.abs((box2_x2 - box2_x1) * (box2_y2 - box2_y1));
            return intersection / (box1_area + box2_area - intersection + 1e-6);
        }
        /// <summary>
        /// Unpack flatten output of a model
        /// </summary>
        /// <param name="output">flatten Tensor</param>
        /// <param name="num_scales"></param>
        /// <param name="batch_size"></param>
        /// <param name="num_anchors"></param>
        /// <param name="grid_size"></param>
        /// <param name="num_classes"></param>
        /// <returns>[N,a,s,s,classes + 5]</returns>
        public static Tensor[] GetUnpackPredictions(Tensor output, Tensor[] scaled_anchor, int num_scales, int batch_size, int[] grid_size, int num_classes) {
            List<Tensor> pred_list = new List<Tensor>();
            int sliced = 0;
            foreach (int scale_id in Enumerable.Range(0, num_scales)) {
                int num_anch_per_scale = (int)scaled_anchor[scale_id].shape[0];
                Tensor ancs = scaled_anchor[scale_id].reshape(1, num_anch_per_scale, 1, 1, 2);

                int slice_size =
                    batch_size
                    * num_anch_per_scale
                    * grid_size[scale_id]
                    * grid_size[scale_id]
                    * (num_classes + 5);

                Tensor prediction 
                    = output[TensorIndex.Slice(sliced, sliced + slice_size)]
                    .reshape(
                        batch_size
                        , num_anch_per_scale
                        , grid_size[scale_id]
                        , grid_size[scale_id]
                        , (num_classes + 5));

                pred_list.Add(prediction);
                sliced += slice_size;
            }
            Tensor[] predictions = pred_list.ToArray<Tensor>();

            return predictions;
        }
        /// <summary>
        /// Construct targets for each scale 
        /// to match prediction format
        /// </summary>
        /// <param name="target">[N,c,x,y,w,h]</param>
        /// <param name="anchors">[num_anchors,w,h]</param>
        /// <param name="scales"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Tensor[] GetTransformedTargets(Tensor target, Tensor unscaled_anchors, int[] scales, Device device)
        {
            target = target.to(CPU);
            unscaled_anchors = unscaled_anchors.reshape(-1,2);
            int num_of_anchors = (int)unscaled_anchors.shape[0];
            int num_of_anchors_per_scale = num_of_anchors / scales.Length;

            int batch_size = (int)(target.shape[0]);

            List<Tensor> target_yolo_list = new List<Tensor>();
            foreach (int i in Enumerable.Range(0, scales.Length)) {

                target_yolo_list.Add(torch.zeros(new long[] { batch_size, num_of_anchors_per_scale, scales[i], scales[i], 6 }));
            }
            Tensor[] targets_yolo = target_yolo_list.ToArray<Tensor>();
            foreach (int batch_idx in Enumerable.Range(0, batch_size))
            {
                Tensor ts = target[batch_idx];
                int number_of_targets = (int)(ts.shape[0]);
                foreach (int id in Enumerable.Range(0, number_of_targets))
                {
                    int any_class = (int)ts[id, 0].item<float>();
                    if (any_class == 0)
                        continue;
                    Tensor t = ts[id];
                    t = t.roll(4, 0);
                    Tensor b = t[TensorIndex.Slice(2, 4)];
                    Tensor iou_anchors = iou_width_height(b, unscaled_anchors);
                    Tensor anchor_indices = iou_anchors.argsort(0, descending: true);
                    float x = (float)t[0];
                    float y = (float)t[1];
                    float width = (float)t[2];
                    float height = (float)t[3];
                    float class_label = (float)t[4];
                    bool[] has_anchors = new bool[] { false, false, false };
                    foreach (int anch_idx in Enumerable.Range(0, (int)(anchor_indices.shape[0])))
                    {
                        int scale_idx = anch_idx / num_of_anchors_per_scale;
                        int anchors_on_scale = anch_idx % num_of_anchors_per_scale;
                        int s = scales[scale_idx];
                        (int i, int j) = ((int)(s * y), (int)(s * x));
                        bool anchor_taken = (bool)targets_yolo[scale_idx][batch_idx, anchors_on_scale, i, j, 0];
                        if (anchor_taken == false && has_anchors[scale_idx] == false)
                        {
                            targets_yolo[scale_idx][batch_idx, anchors_on_scale, i, j, 0] = 1;
                            (float x_cell, float y_cell) = (s * x - j, s * y - i);
                            (float width_cell, float height_cell) = (width * s, height * s);
                            Tensor box_coordinates = torch.tensor(new float[] {
                            x_cell,y_cell, width_cell, height_cell
                            });
                            targets_yolo[scale_idx][batch_idx, anchors_on_scale, i, j, TensorIndex.Slice(1, 5)] = box_coordinates;
                            targets_yolo[scale_idx][batch_idx, anchors_on_scale, i, j, 5] = (int)class_label;
                            has_anchors[scale_idx] = true;
                        }
                    }
                }
            }
            foreach (int t in Enumerable.Range(0, scales.Length))
            {
                targets_yolo[t] = targets_yolo[t].to(device);
            }
            return targets_yolo;
        }
        /// <summary>
        /// Scale anchors
        /// </summary>
        /// <param name="anchors">[num_anchors,w,h]</param>
        /// <param name="scales"></param>
        /// <param name="device"></param>
        /// <returns></returns>
        public static Tensor[] GetScaledAnchors(Tensor anchors, int[] scales, Device device) {
            List<Tensor> anchor_boxes = new List<Tensor>();
            int anchors_per_scale = (int)anchors.shape[0] / scales.Length;
                // create anchors array
            foreach (int anc_id in Enumerable.Range(0, scales.Length))
            {
                int start = anc_id * anchors_per_scale;
                int end = anchors_per_scale * (anc_id + 1);
                Tensor scaled_anchors = anchors[TensorIndex.Slice(start, end)];
                scaled_anchors = scaled_anchors * scales[anc_id];
                anchor_boxes.Add(scaled_anchors.to(device));
            }
            return anchor_boxes.ToArray<Tensor>();
        }
        /// <summary>
        /// Get all prediction boxes of individual scale
        /// </summary>
        /// <param name="scale_data">[N,a,s,s,classes + 5]</param>
        /// <param name="scale"></param>
        /// <param name="anchors"></param>
        /// <returns>[N,all boxes of prediction,6]</returns>
        public static Tensor GetPredBoxesPerScale(Tensor scale_data, int scale, Tensor anchors)
        {
            int batch_size = (int)scale_data.shape[0];
            int num_anchors = (int)scale_data.shape[1];


            Tensor ancs = anchors.reshape(1, anchors.shape[0], 1, 1, 2);
            Tensor box_prediction = scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(1, 5)];
            box_prediction[TensorIndex.Ellipsis, TensorIndex.Slice(0, 2)]
                = sigmoid(box_prediction[TensorIndex.Ellipsis, TensorIndex.Slice(0, 2)]);
            box_prediction[TensorIndex.Ellipsis, TensorIndex.Slice(2, null)]
                = exp(box_prediction[TensorIndex.Ellipsis, TensorIndex.Slice(2, null)]) * ancs;
            Tensor scores = sigmoid(scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)]);
            Tensor best_class = scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(5, null)].argmax(-1).unsqueeze(-1);

            Tensor cell_indices
                = torch.arange(scale).repeat(scale_data.shape[0], num_anchors, scale, 1)
                .unsqueeze(-1).to(scale_data.device);



            Tensor x = 1.0f / (float)scale
                        * (box_prediction[
                            TensorIndex.Ellipsis,
                            TensorIndex.Slice(0, 1)]
                        + cell_indices);
            Tensor y = 1.0f / (float)scale
                        * (box_prediction[
                            TensorIndex.Ellipsis,
                            TensorIndex.Slice(1, 2)]
                        + cell_indices.permute(0, 1, 3, 2, 4));

            Tensor w_h = 1.0f / (float)scale 
                        * box_prediction[
                            TensorIndex.Ellipsis, 
                            TensorIndex.Slice(2, 4)];


            Tensor converted_boxes = torch.cat(new List<Tensor>()
                                            {
                                                best_class,
                                                scores,
                                                x,
                                                y,
                                                w_h
                                            }, -1)
                                            .reshape(batch_size, num_anchors * scale * scale, 6);

            return converted_boxes;



        }
        /// <summary>
        /// Get true boxes
        /// </summary>
        /// <param name="scale_data"></param>
        /// <param name="scale"></param>
        /// <returns>[N,all boxes of target,6]</returns>
        public static Tensor GetTargetBoxesPerScale(Tensor scale_data, int scale)
        {
            int batch_size = (int)scale_data.shape[0];
            int num_anchors = (int)scale_data.shape[1];
            Tensor box_prediction = scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(1, 5)];
            Tensor scores = scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)];
            Tensor best_class = scale_data[TensorIndex.Ellipsis, TensorIndex.Slice(5, 6)];

            Tensor cell_indices
                = torch.arange(scale).repeat(scale_data.shape[0], num_anchors, scale, 1)
                .unsqueeze(-1).to(scale_data.device);

            Tensor x = 1.0f / (float)scale
                        * (box_prediction[
                            TensorIndex.Ellipsis,
                            TensorIndex.Slice(0, 1)]
                        + cell_indices);
            Tensor y = 1.0f / (float)scale
                        * (box_prediction[
                            TensorIndex.Ellipsis,
                            TensorIndex.Slice(1, 2)]
                        + cell_indices.permute(0, 1, 3, 2, 4));

            Tensor w_h = 1.0f / (float)scale 
                        * box_prediction[
                            TensorIndex.Ellipsis, 
                            TensorIndex.Slice(2, 4)];

            Tensor converted_boxes
                = torch.cat(new List<Tensor>()
                {
                    best_class,
                    scores,
                    x,
                    y,
                    w_h
                }, -1).reshape(batch_size, num_anchors * scale * scale, 6);

            return converted_boxes;

        }
        /// <summary>
        /// nmx boxes
        /// </summary>
        /// <param name="bounding_boxes"></param>
        /// <param name="iou_threshold"></param>
        /// <param name="prob_threshold"></param>
        /// <param name="box_format"></param>
        /// <returns></returns>
        public static Tensor NonMaxSuppression(Tensor bounding_boxes, float prob_threshold, float iou_threshold, BoxFormat box_format = BoxFormat.corners)
        {            
            var prob_mask = bounding_boxes[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)] > prob_threshold;
            prob_mask = prob_mask.repeat(new long[] { 1, 6 });
            var fltr_box = bounding_boxes[prob_mask].reshape(-1, 6);
            var sort_box = fltr_box[fltr_box[TensorIndex.Ellipsis, 1].argsort(-1, descending: true)];
            List<Tensor> nms_boxes = new List<Tensor>();
            while (sort_box.shape[0] > 0)
            { 
                var chosen_box = sort_box[0].clone();
                sort_box = sort_box[TensorIndex.Slice(1, null)].clone();
                // get other classes for later
                var not_our_class_mask 
                    = sort_box[TensorIndex.Ellipsis,TensorIndex.Slice(0,1)] != chosen_box[0];
                not_our_class_mask = not_our_class_mask.repeat(new long[] { 1, 6 });
                // get the rest of our class
                var our_class_mask
                    = sort_box[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)] == chosen_box[0];
                our_class_mask = our_class_mask.repeat(new long[] { 1, 6 });
                var our_class = sort_box[our_class_mask].reshape(-1, 6); ;                
                // get ious of classes that higher than our threshold
                var our_class_iou_mask
                    = intersection_over_union(
                        our_class[TensorIndex.Ellipsis, TensorIndex.Slice(2, null)],
                        chosen_box[TensorIndex.Slice(2, null)]
                        ) < iou_threshold;
                our_class_iou_mask = our_class_iou_mask.repeat(new long[] { 1, 6 });
                chosen_box = chosen_box.reshape(-1, 6);
                our_class = our_class[our_class_iou_mask].reshape(-1, 6);
                chosen_box = torch.cat(new List<Tensor>() {
                    chosen_box,
                    our_class
                    }, 0);
                sort_box = sort_box[not_our_class_mask].reshape(-1, 6);
                nms_boxes.Add(chosen_box);
            }
            //return nms_boxes.ToArray();
            return cat(nms_boxes, 0);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pred_boxes"></param>
        /// <param name="true_boxes"></param>
        /// <param name="num_class_know"></param>
        /// <param name="iou_threshold"></param>
        /// <param name="box_format"></param>
        public static float MeanAveragePrecision(
            Tensor pred_boxes,
            Tensor true_boxes,
            int num_class_know,
            float iou_threshold,
            BoxFormat box_format = BoxFormat.midpoint)
        {
            var epsilon = 1e-6;

            List<float> average_precisions = new List<float>();
            foreach (int c in Enumerable.Range(0, num_class_know))
            {
                // detections of c prediction
                Tensor c_mask = pred_boxes[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)] == c;
                c_mask = c_mask.repeat(new long[] { 1, 7 });
                Tensor detections_of_c = pred_boxes[c_mask].reshape(-1, 7);

                // detections of c target
                c_mask = true_boxes[TensorIndex.Ellipsis, TensorIndex.Slice(1, 2)] == c;
                c_mask = c_mask.repeat(new long[] { 1, 7 });
                Tensor ground_truths_of_c = true_boxes[c_mask].reshape(-1, 7);

                // sort by best probability
                detections_of_c = detections_of_c[
                    detections_of_c[TensorIndex.Ellipsis, 2].argsort(-1, descending: true)];


                Tensor TruePositive = torch.zeros(detections_of_c.shape[0]);
                Tensor FalsePositive = torch.zeros(detections_of_c.shape[0]);

                int total_TRUE_boxes = (int)ground_truths_of_c.shape[0];

                if (total_TRUE_boxes == 0)
                    continue;

                Dictionary<int, List<(int transact_id, int box_no)>> true_box_picked = new Dictionary<int, List<(int transact_id, int box_no)>>();


                foreach (int dIdx in Enumerable.Range(0, (int)detections_of_c.shape[0]))
                {
                    int transaction_id = (int)detections_of_c[dIdx][0];

                    Tensor transaction_mask = ground_truths_of_c[TensorIndex.Ellipsis, TensorIndex.Slice(0, 1)] == transaction_id;
                    transaction_mask = transaction_mask.repeat(new long[] { 1, 7 });
                    Tensor ground_truths_of_transaction = ground_truths_of_c[transaction_mask].reshape(-1, 7);

                    int no_gts = (int)ground_truths_of_transaction.shape[0];
                    float best_iou = 0;
                    int best_index_of_iou = -1;
                    foreach (int gt in Enumerable.Range(0, no_gts)) {
                        Tensor iou
                            = intersection_over_union(
                                    detections_of_c[dIdx][TensorIndex.Slice(3, null)],
                                    ground_truths_of_transaction[gt][TensorIndex.Slice(3, null)],
                                    box_format);

                        if ((float)iou > best_iou) {
                            best_iou = (float)iou;
                            best_index_of_iou = gt;
                        }
                    }
                    if (best_iou > iou_threshold)
                    {
                        if (true_box_picked.get_List_Of_Picked(c).Contains((transaction_id, best_index_of_iou)))
                        {
                            FalsePositive[dIdx] = 1;
                        }
                        else {
                            TruePositive[dIdx] = 1;
                            true_box_picked.get_List_Of_Picked(c).Add((transaction_id, best_index_of_iou));
                        }
                    }
                    else {
                        FalsePositive[dIdx] = 1;
                    }
                }

                Tensor TP_cumsum = torch.cumsum(TruePositive, dimension: 0);
                Tensor FP_cumsum = torch.cumsum(FalsePositive, dimension: 0);
                Tensor recalls = TP_cumsum / (total_TRUE_boxes + epsilon);
                Tensor precisions = TP_cumsum / (TP_cumsum + FP_cumsum + epsilon);
                // calculate trapizoid ###########################################
                //
                //    n-1
                //   _____
                //   \         (  Xi - X(i-1) )  x ( Yi + Y(i-1) )
                //    \       __________________________________________ 
                //    /                          2
                //   /____      
                //
                //    i=1
                //################################################################
                Tensor y = precisions;
                Tensor x = recalls;

                Tensor x_i = x[TensorIndex.Slice(1,null)];
                Tensor x_i_1 = x[TensorIndex.Slice(0, x_i.size(0))];

                Tensor y_i = y[TensorIndex.Slice(1, null)];
                Tensor y_i_1 = y[TensorIndex.Slice(0, x_i.size(0))];

                Tensor xy = ((x_i - x_i_1) * (y_i + y_i_1)) / 2;

                float sum_all = (float)xy.sum();

                average_precisions.Add(sum_all);
                
            }
            return average_precisions.Sum() / average_precisions.Count();
        }



        private static List<(int transact_id, int box_no)> get_List_Of_Picked(this Dictionary<int, List<(int transact_id, int box_no)>> true_box_picked, int class_id)
        {
            true_box_picked.TryGetValue(class_id, out var value);
            if (value is null)
                return new List<(int transact_id, int box_no)>();
            if (value.ToString() is null)
                return new List<(int transact_id, int box_no)>();
            return value;
        }

    }
}

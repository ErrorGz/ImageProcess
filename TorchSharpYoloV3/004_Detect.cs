namespace TorchSharpYoloV3
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Threading.Channels;
    using TorchSharp;
    using TorchSharpYoloV3.YoloV3;
    using static TorchSharp.torch;
    using static TorchSharp.torch.nn;
    using static TorchSharp.torch.utils.data;
    using static Utils;

    public partial class ModelHandler
    {
        private static ImageHelper imageHandler = new ImageHelper();
        public Image detect(string image_file, float[] anchors)
        {
            Tensor input = empty(_image_size, _image_size, _channels);

            #region Read image bytes into Tensor
            try
            {
                Bitmap my_image = new Bitmap(image_file);
                my_image = imageHandler.ResizeBitmap(my_image, _image_size, _image_size);
                System.Drawing.Imaging.BitmapData bd =
                    my_image.LockBits(new Rectangle(0, 0, my_image.Width, my_image.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                IntPtr ptr = bd.Scan0;
                int bytes = Math.Abs(bd.Stride) * my_image.Height;
                byte[] rgbBytes = new byte[bytes];
                float[] rgbFloat = new float[_image_size * _image_size * _channels];
                Marshal.Copy(ptr, rgbBytes, 0, bytes);
                my_image.UnlockBits(bd);

                foreach (int h in Enumerable.Range(0, _image_size))
                {
                    foreach (int w in Enumerable.Range(0, _image_size))
                    {
                        int idx = h * _image_size * _channels + w * _channels;

                        rgbFloat[idx] = rgbBytes[h * w + w] / 256.0f;
                        if (_channels > 1)
                            rgbFloat[idx + 1] = rgbBytes[h * w + w + 1] / 256.0f;
                        if (_channels > 2)
                            rgbFloat[idx + 2] = rgbBytes[h * w + w + 2] / 256.0f;
                    }
                }
                input = (Tensor)rgbFloat;
            }
            finally {
                input = input.reshape(_image_size, _image_size, _channels);
                input = input.permute(2, 0, 1);
                input = input.unsqueeze(0).to(_model.device());
            }
            #endregion


            #region Run prediction
            _model.eval();
            int[] scales = _model.GetGrids();
            int num_of_scales = scales.Length;

            float nms_iou_threshold = 0.45f;
            float conf_threshold = 0.2f;
            Tensor all_anchors = anchors;
            if (all_anchors.shape[0] < 2)
                throw new Exception("invalid number of anchors");
            all_anchors = all_anchors.reshape(-1, 2);
            Tensor[] scaled_anchor
                = GetScaledAnchors(
                    anchors: all_anchors,
                    scales: scales,
                    device: _model.device());


            Tensor output = _model.forward(input);
            // unpack prediction
            Tensor[] predictions
                    = GetUnpackPredictions(
                        output: output,
                        scaled_anchor: scaled_anchor,
                        num_scales: num_of_scales,
                        batch_size: 1,
                        grid_size: _model.GetGrids(),
                        num_classes: _num_classes);


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
            List<Tensor> nmx_boxes = new List<Tensor>();


            Tensor nms = NonMaxSuppression(
                            bounding_boxes: bboxes[0],
                            prob_threshold: conf_threshold,
                            iou_threshold: nms_iou_threshold,
                            box_format: BoxFormat.midpoint);

            #endregion

            return plot_image(image_file, nms);
        }

        public Image plot_image(string image_file, Tensor boxes)
        {
            int num_boxes = (int)boxes.shape[0];


            Image image = Image.FromFile(image_file);

            using Graphics graphics = Graphics.FromImage(image);
            using SolidBrush sb = new SolidBrush(Color.Yellow);
            using Pen pen = new Pen(sb,2);
            using Font fn = new Font(FontFamily.GenericSansSerif, 16);
            foreach (int i in Enumerable.Range(0, num_boxes)) {

                var class_id = (int)boxes[i][0];
                var upper_x = (int)((float)(boxes[i][2] - boxes[i][4] / 2) * image.Width);
                var upper_y = (int)((float)(boxes[i][3] - boxes[i][5] / 2) * image.Height);
                var width = (int)((float)boxes[i][4] * image.Width);
                var height = (int)((float)boxes[i][5] * image.Height);
                Rectangle rect = new Rectangle(upper_x, upper_y, width, height);
                graphics.DrawRectangle(pen, rect);
                graphics.DrawString(class_id.ToString(), fn, sb, upper_x, upper_y);
            }
            return image;
        }



    }
}

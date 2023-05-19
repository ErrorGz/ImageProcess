using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using OpenCvSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Yolo5.NetCore.Extensions;
using Yolo5.NetCore.Models;


namespace Yolo5.NetCore
{
    public class YOLOv5<T> : IDisposable where T : YoloModel
    {
        private readonly InferenceSession _inferenceSession;
        private readonly T _model;
        Stopwatch stopwatch = new Stopwatch();
        public YOLOv5()
        {
            _model = Activator.CreateInstance<T>();
        }

        public YOLOv5(string model, SessionOptions opts = null) : this()
        {
            var binary = File.ReadAllBytes(model);
            _inferenceSession = new InferenceSession(binary, opts ?? new SessionOptions());
            InitModelName();

        }

        public YOLOv5(Stream model, SessionOptions opts = null) : this()
        {
            using var reader = new BinaryReader(model);
            _inferenceSession = new InferenceSession(reader.ReadBytes((int)model.Length), opts ?? new SessionOptions());
            InitModelName();

        }

        public YOLOv5(byte[] model, SessionOptions opts = null) : this()
        {
            _inferenceSession = new InferenceSession(model, opts ?? new SessionOptions());
            InitModelName();
        }
        private void InitModelName()
        {
            var inputnames = _inferenceSession.InputMetadata.Select(o => o.Key).ToArray();
            _model.Inputs = inputnames;
            var outputnames = _inferenceSession.OutputMetadata.Select(o => o.Key).ToArray();
            _model.Outputs = outputnames;

            var d = _inferenceSession.InputMetadata.FirstOrDefault().Value.Dimensions;
            var o = _inferenceSession.OutputMetadata.FirstOrDefault().Value.Dimensions;
            _model.Dimensions = o[2];
            if (d.Length == 3)
            {
                _model.Height = d[1];
                _model.Width = d[2];
            }
            else if (d.Length == 4)
            {
                if (d[3] == 3)
                {
                    _model.Height = d[1];
                    _model.Width = d[2];
                }
                else if (d[1] == 3)
                {
                    _model.Height = d[2];
                    _model.Width = d[3];
                }

            }


        }
        public void Dispose()
        {
            _inferenceSession.Dispose();
        }
        private static float Sigmoid(float value)
        {
            return 1 / (1 + (float)Math.Exp(-value));
        }
        private float[] Xywh2xyxy(float[] source)
        {
            var result = new float[4];

            result[0] = source[0] - source[2] / 2f;
            result[1] = source[1] - source[3] / 2f;
            result[2] = source[0] + source[2] / 2f;
            result[3] = source[1] + source[3] / 2f;

            return result;
        }
        public float Clamp(float value, float min, float max)
        {
            return value < min ? min : value > max ? max : value;
        }
        public Tensor<float> ToTensor(Mat image)
        {

            OpenCvSharp.Size dnnsInputSize = new OpenCvSharp.Size(image.Width, image.Height);
            OpenCvSharp.Scalar mean = new Scalar(0.408, 0.694, 0.482);
            using Mat blob = OpenCvSharp.Dnn.CvDnn.BlobFromImage(image, 1.0f / 255f, dnnsInputSize, default, true, false);

            float[] data_bytes = new float[blob.Total()];
            Marshal.Copy(blob.Data, data_bytes, 0, data_bytes.Length);
            Tensor<float> tensor = new DenseTensor<float>(data_bytes, new int[] { 1, 3, _model.Height, _model.Width });

            return tensor;
        }
        private DenseTensor<float>[] Inference(Mat image)
        {
            //Mat resized = null;
            stopwatch.Restart();
            if (image.Width != _model.Width || image.Height != _model.Height)
            {
                image = image.Resize(new OpenCvSharp.Size(_model.Width, _model.Height));
            }
            //Debug.WriteLine($"ResizeImage:{stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor(_model.Inputs[0], ToTensor(image)),
            };
            //Debug.WriteLine($"CreateFromTensor:{stopwatch.ElapsedMilliseconds}ms");
            stopwatch.Restart();
            var result = _inferenceSession.Run(inputs);

            //Debug.WriteLine($"_inferenceSession.Run:{stopwatch.ElapsedMilliseconds}ms");
            var tensor = _model.Outputs.Select(item => result.First(x => x.Name == item).Value as DenseTensor<float>).ToArray();
            return tensor;
        }

        private List<YoloPrediction> ParseDetect(DenseTensor<float> output, OpenCvSharp.Mat image)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (image.Width, image.Height); // image w and h

            var (xGain, yGain) = (_model.Width / (float)w, _model.Height / (float)h); // x, y gains
            var gain = Math.Max(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((_model.Width - w * xGain) / 2, (_model.Height - h * yGain) / 2); // left, right pads
            Parallel.For(0, (int)output.Length / _model.Dimensions, (i) =>
            {
                if (output[0, i, 4] <= _model.Confidence) return; // skip low obj_conf results

                Parallel.For(5, _model.Dimensions, (j) =>
                {
                    output[0, i, j] = output[0, i, j] * output[0, i, 4]; // mul_conf = obj_conf * cls_conf

                    if (output[0, i, j] <= _model.MulConfidence) return; // skip low mul_conf results

                    float xMin = ((output[0, i, 0] - output[0, i, 2] / 2) - xPad) / xGain; // unpad bbox tlx to original
                    float yMin = ((output[0, i, 1] - output[0, i, 3] / 2) - yPad) / yGain; // unpad bbox tly to original
                    float xMax = ((output[0, i, 0] + output[0, i, 2] / 2) - xPad) / xGain; // unpad bbox brx to original
                    float yMax = ((output[0, i, 1] + output[0, i, 3] / 2) - yPad) / yGain; // unpad bbox bry to original

                    xMin = Clamp(xMin, 0, w - 0); // clip bbox tlx to boundaries
                    yMin = Clamp(yMin, 0, h - 0); // clip bbox tly to boundaries
                    xMax = Clamp(xMax, 0, w - 1); // clip bbox brx to boundaries
                    yMax = Clamp(yMax, 0, h - 1); // clip bbox bry to boundaries

                    //YoloLabel label = _model.Labels[j - 5];

                    var prediction = new YoloPrediction(j - 5, output[0, i, j])
                    {
                        //Rectangle = new Rect((int)xMin, (int)yMin, (int)(xMax - xMin), (int)(yMax - yMin))
                        Rectangle = new Rect2f(xMin, yMin, (xMax - xMin), (yMax - yMin))
                    };

                    result.Add(prediction);
                });

            });

            return result.ToList();
        }

        private List<YoloPrediction> ParseSigmoid(DenseTensor<float>[] output, OpenCvSharp.Mat image)
        {
            var result = new ConcurrentBag<YoloPrediction>();

            var (w, h) = (image.Width, image.Height); // image w and h
            var (xGain, yGain) = (_model.Width / (float)w, _model.Height / (float)h); // x, y gains
            var gain = Math.Min(xGain, yGain); // gain = resized / original

            var (xPad, yPad) = ((_model.Width - w * gain) / 2, (_model.Height - h * gain) / 2); // left, right pads

            Parallel.For(0, output.Length, (i) => // iterate model outputs
            {
                int shapes = _model.Shapes[i]; // shapes per output

                for (int a = 0; a < _model.Anchors[0].Length; a++)
                {
                    for (int y = 0; y < shapes; y++)
                    {
                        for (int x = 0; x < shapes; x++)
                        {
                            int offset = (shapes * shapes * a + shapes * y + x) * _model.Dimensions;

                            float[] buffer = output[i].Skip(offset).Take(_model.Dimensions).Select(Sigmoid).ToArray();

                            if (buffer[4] <= _model.Confidence) return; // skip low obj_conf results


                            List<float> scores = buffer.Skip(5).Select(b => b * buffer[4]).ToList(); // mul_conf = obj_conf * cls_conf

                            float mulConfidence = scores.Max(); // max confidence score

                            if (mulConfidence <= _model.MulConfidence) return; // skip low mul_conf results

                            float rawX = (buffer[0] * 2 - 0.5f + x) * _model.Strides[i]; // predicted bbox x (center)
                            float rawY = (buffer[1] * 2 - 0.5f + y) * _model.Strides[i]; // predicted bbox y (center)

                            float rawW = (float)Math.Pow(buffer[2] * 2, 2) * _model.Anchors[i][a][0]; // predicted bbox w
                            float rawH = (float)Math.Pow(buffer[3] * 2, 2) * _model.Anchors[i][a][1]; // predicted bbox h

                            float[] xyxy = Xywh2xyxy(new float[] { rawX, rawY, rawW, rawH });

                            float xMin = Clamp((xyxy[0] - xPad) / xGain, 0, w - 0); // unpad, clip tlx
                            float yMin = Clamp((xyxy[1] - yPad) / yGain, 0, h - 0); // unpad, clip tly
                            float xMax = Clamp((xyxy[2] - xPad) / xGain, 0, w - 1); // unpad, clip brx
                            float yMax = Clamp((xyxy[3] - yPad) / yGain, 0, h - 1); // unpad, clip bry

                            //YoloLabel label = _model.Labels[scores.IndexOf(mulConfidence)];

                            var prediction = new YoloPrediction(scores.IndexOf(mulConfidence), mulConfidence)
                            {
                                //Rectangle = new Rect((int)xMin, (int)yMin, (int)(xMax - xMin), (int)(yMax - yMin))
                                Rectangle = new Rect2f(xMin, yMin, (xMax - xMin), (yMax - yMin))
                            };

                            result.Add(prediction);
                        }
                    }
                }
            });

            return result.ToList();
        }

        private List<YoloPrediction> ParseOutput(DenseTensor<float>[] output, OpenCvSharp.Mat image)
        {
            stopwatch.Restart();
            if (output == null) throw new ArgumentNullException(nameof(output));
            var r = _model.UseDetect ? Clean(ParseDetect(output[0], image)) : Clean(ParseSigmoid(output, image));
            //Debug.WriteLine($"ParseOutput:{stopwatch.ElapsedMilliseconds}ms");
            return r;
        }

        private List<YoloPrediction> Clean(IReadOnlyCollection<YoloPrediction> items)
        {
            var result = new List<YoloPrediction>(items);

            foreach (var item in items) // iterate every prediction
            {
                foreach (var current in result.ToList()) // make a copy for each iteration
                {
                    if (current == item) continue;

                    var (rect1, rect2) = (item.Rectangle, current.Rectangle);

                    Rect2f intersection = Rect2f.Intersect(rect1, rect2);

                    float intArea = intersection.Area(); // intersection area
                    float unionArea = rect1.Area() + rect2.Area() - intArea; // union area
                    float overlap = intArea / unionArea; // overlap ratio

                    if (overlap >= _model.Overlap)
                    {
                        if (item.Score >= current.Score)
                        {
                            result.Remove(current);
                        }
                    }
                }
            }

            return result;
        }


        //private List<YoloPrediction> Clean(IReadOnlyCollection<YoloPrediction> items)
        //{
        //    var result = new List<YoloPrediction>();

        //    var maxscoreitemlist = items.GroupBy(o => o.LabelId).SelectMany(o => o.Where(p => p.Score == o.Max(q => q.Score))).ToList();
        //    return maxscoreitemlist;


        //}

        public List<YoloPrediction> Predict(OpenCvSharp.Mat image)
        {
            var inference = Inference(image);
            return ParseOutput(inference, image);
        }
    }




}
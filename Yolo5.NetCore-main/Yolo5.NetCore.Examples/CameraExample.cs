using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Yolo5.NetCore.Models;

namespace Yolo5.NetCore.Examples
{
    public static class CameraExample
    {
        public static void Run()
        {
            var capture = new VideoCapture(0);
            using var window = new Window("Camera");
            using var image = new Mat();

            while (true)
            {
                capture.Read(image);
                if (image.Empty())
                    break;

                using var png = image.ToMemoryStream(".png");
                using var frame = Image.FromStream(png);

                using var yolo = new Yolo<YoloCocoModel>("Models/yolov5n6.onnx");
                var predictions = yolo.Predict(frame);

                using var graphics = Graphics.FromImage(frame);
                foreach (var prediction in predictions)
                {
                    var score = Math.Round(prediction.Score, 2);
                    graphics.DrawRectangles(new Pen(Color.Blue, 1),
                        new[] { prediction.Rectangle });

                    var (x, y) = (prediction.Rectangle.X - 3, prediction.Rectangle.Y - 23);

                    graphics.DrawString($"{prediction.Label.Name} ({score})",
                        new Font("Consolas", 16, GraphicsUnit.Pixel), new SolidBrush(Color.White),
                        new PointF(x, y));
                }

                using var stream = new MemoryStream();
                frame.Save(stream, ImageFormat.Png);
                var binary = stream.ToArray();

                window.ShowImage(Mat.FromImageData(binary, ImreadModes.Color));
                Cv2.WaitKey(30);
            }
        }
    }
}

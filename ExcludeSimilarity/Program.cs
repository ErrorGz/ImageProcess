

using SkiaSharp;
using System;
using System.Security.Cryptography;

namespace ExcludeSimilarity
{
    internal class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("请输入需要分析相似似的图片文件夹：");
            var Inputfolder = Console.ReadLine();
            if (Directory.Exists(Inputfolder) == false)
            {
                Console.WriteLine($"\"{Inputfolder}\"不是一个有效文件夹。");
                return;
            }

            Console.WriteLine("请输入生成图片的输出文件夹：");
            var Outputfolder = Console.ReadLine();
            if (Directory.Exists(Outputfolder) == false)
            {
                Console.WriteLine($"{Outputfolder}不是一个有效文件夹。");
                return;
            }

            //列举所有图片文件
            string[] ImageFileFormats = { ".jpg", ".jpeg", ".png", ".gif" };
            string[] allImageFiles = Directory.GetFiles(Inputfolder, "*.*", SearchOption.AllDirectories)
                .Where(file => ImageFileFormats.Contains(Path.GetExtension(file).ToLower()))
                .ToArray();
            var blank = PreprocessBlankImage(412, 412);

            List<(string, double)> result = new List<(string, double)>();
            for (int i = 0; i < allImageFiles.Length; i++)
            {
                string imageFile = allImageFiles[i];
                var distance = CompareWithBlankImage(imageFile, blank);
                result.Add((imageFile, distance));
                Console.WriteLine($"空白对比：{imageFile}={distance}");
            }

            result = result.OrderBy(o => o.Item2).ToList();

            for (int i = 0; i < result.Count - 1; i++)
            {
                for (var j = i + 1; j < result.Count; j++)
                {
                    string imageFile1 = result[i].Item1;
                    string imageFile2 = result[j].Item1;
                    if (File.Exists(imageFile1) == false || File.Exists(imageFile2) == false)
                        continue;
                    // 计算图像之间的巴氏距离
                    double distance = ComputeBhattacharyyaDistance(imageFile1, imageFile2);
                    Console.WriteLine($"文件对比：{imageFile1} : {imageFile2} = {distance}");
                    if (distance < 0.2)
                    {
                        FileInfo fi = new FileInfo(imageFile2);
                        var folder = Path.Combine(Outputfolder, i.ToString("D3"));
                        if (Directory.Exists(folder) == false)
                        {
                            Directory.CreateDirectory(folder);
                        }
                        var destFileName = Path.Join(folder, fi.Name);
                        File.Move(imageFile2, destFileName);
                        Console.WriteLine($"移动文件：{imageFile2} 到 {destFileName}");
                    }
                    else
                    {
                        break;
                    }
                }

            }



        }


        static Dictionary<string, List<string>> FindSimilarImages(string directory)
        {
            Dictionary<string, List<string>> imageHashes = new Dictionary<string, List<string>>();


            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif" };

            // 遍历目录中的所有图片文件
            foreach (string file in Directory.GetFiles(directory, "*", SearchOption.AllDirectories))
            {
                if (Array.Exists(imageExtensions, ext => ext.Equals(Path.GetExtension(file), StringComparison.OrdinalIgnoreCase)))
                {
                    string imageHash = ComputeImageHash(file);

                    Console.WriteLine($"{file}:{imageHash}");
                    // 将图像的路径添加到哈希值的列表中
                    if (imageHashes.ContainsKey(imageHash))
                    {
                        imageHashes[imageHash].Add(file);
                    }
                    else
                    {
                        imageHashes.Add(imageHash, new List<string> { file });
                    }
                }
            }


            return imageHashes;
        }
        static string ComputeImageHash(string imagePath)
        {
            using (var stream = new SKFileStream(imagePath))
            using (var bitmap = SKBitmap.Decode(stream))
            using (var image = SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
            {
                byte[] imageBytes = data.ToArray();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(imageBytes);
                    return Convert.ToBase64String(hashBytes);
                }
            }
        }



        static double ComputeBhattacharyyaDistance(string imagePath1, string imagePath2)
        {
            using (SKBitmap bitmap1 = SKBitmap.Decode(imagePath1))
            using (SKBitmap bitmap2 = SKBitmap.Decode(imagePath2))
            {
                // 转换为灰度图像
                SKBitmap grayBitmap1 = ConvertToGrayscale(bitmap1);
                SKBitmap grayBitmap2 = ConvertToGrayscale(bitmap2);

                // 计算颜色直方图
                int[] hist1 = ComputeHistogram(grayBitmap1);
                int[] hist2 = ComputeHistogram(grayBitmap2);

                // 计算巴氏系数
                double bCoefficient = ComputeBhattacharyyaCoefficient(hist1, hist2);

                // 计算巴氏距离
                double bDistance = Math.Sqrt(1 - bCoefficient);

                return bDistance;
            }
        }


        static double ComputeBhattacharyyaCoefficient(int[] hist1, int[] hist2)
        {
            // 计算直方图的总和
            int sum1 = 0, sum2 = 0;
            for (int i = 0; i < 256; i++)
            {
                sum1 += hist1[i];
                sum2 += hist2[i];
            }

            // 计算巴氏系数
            double coefficient = 0;
            for (int i = 0; i < 256; i++)
            {
                double p1 = hist1[i] / (double)sum1;
                double p2 = hist2[i] / (double)sum2;
                coefficient += Math.Sqrt(p1 * p2);
            }

            return coefficient;
        }
        static SKBitmap ConvertToGrayscale(SKBitmap bitmap)
        {
            SKBitmap grayscaleBitmap = new SKBitmap(bitmap.Width, bitmap.Height);

            using (SKCanvas canvas = new SKCanvas(grayscaleBitmap))
            {
                SKPaint paint = new SKPaint
                {
                    ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                    {
                0.2989f, 0.587f, 0.114f, 0, 0,
                0.2989f, 0.587f, 0.114f, 0, 0,
                0.2989f, 0.587f, 0.114f, 0, 0,
                0, 0, 0, 1, 0
                    }),
                };
                canvas.DrawBitmap(bitmap, SKRect.Create(bitmap.Width, bitmap.Height), paint);
            }

            return grayscaleBitmap;
        }



        static int[] ComputeHistogram(SKBitmap bitmap)
        {
            int[] histogram = new int[256];

            using (SKPixmap pixmap = bitmap.PeekPixels())
            {
                for (int y = 0; y < pixmap.Height; y++)
                {
                    for (int x = 0; x < pixmap.Width; x++)
                    {
                        SKColor color = pixmap.GetPixelColor(x, y);
                        byte pixelValue = color.Red;
                        histogram[pixelValue]++;
                    }
                }
            }

            return histogram;
        }


        static int[] PreprocessBlankImage(int width, int height)
        {
            // 创建空白图像
            using (SKBitmap blankBitmap = new SKBitmap(width, height))
            {
                // 填充随机彩色噪点
                using (SKCanvas canvas = new SKCanvas(blankBitmap))
                {
                    Random random = new Random();
                    SKPaint paint = new SKPaint();

                    for (int x = 0; x < width; x++)
                    {
                        for (int y = 0; y < height; y++)
                        {
                            byte r = (byte)random.Next(256);
                            byte g = (byte)random.Next(256);
                            byte b = (byte)random.Next(256);

                            paint.Color = new SKColor(r, g, b);
                            canvas.DrawPoint(x, y, paint);
                        }
                    }
                }

                // 转换为灰度图像
                SKBitmap grayBitmap = ConvertToGrayscale(blankBitmap);

                // 计算颜色直方图
                int[] blankHist = ComputeHistogram(grayBitmap);

                return blankHist;
            }
        }


        // 比较输入图像与空白图像
        static double CompareWithBlankImage(string imagePath, int[] blankHist)
        {

            using (SKBitmap bitmap = SKBitmap.Decode(imagePath))
            {
                // 转换为灰度图像
                SKBitmap grayBitmap = ConvertToGrayscale(bitmap);

                // 计算颜色直方图
                int[] hist = ComputeHistogram(grayBitmap);

                // 计算巴氏系数
                double coefficient = ComputeBhattacharyyaCoefficient(hist, blankHist);

                // 计算巴氏距离
                double distance = Math.Sqrt(1 - coefficient);

                return distance;
            }
        }
    }
}
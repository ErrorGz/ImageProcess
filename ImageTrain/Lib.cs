using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TorchSharp;
using YamlDotNet.RepresentationModel;
using static TorchSharp.torchvision;
using static TorchSharp.torch;
using static TorchSharp.torch.distributions.transforms;
using static TorchSharp.torch.utils.data;
using TorchSharp.Modules;
using OpenCvSharp;
using System.Diagnostics;

namespace ImageTrain
{
    public class StaticLib
    {
        static Process process;
        static public void RunTensorBoard()
        {
            // 创建唯一的互斥锁名称
            string mutexName = "PowerShellTensorBoard";

            // 尝试获取互斥锁
            bool createdNew;
            Mutex mutex = new Mutex(true, mutexName, out createdNew);

            if (!createdNew)
            {
                // 已经有一个实例在运行
                Console.WriteLine("已经有一个实例在运行。");
                return;
            }

            try
            {
                // 获取当前程序的目录
                string currentDirectory = Directory.GetCurrentDirectory();

                // 启动PowerShell进程
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false, // 显示控制台窗口
                    UseShellExecute = false,
                    WorkingDirectory = currentDirectory
                };

                process = new Process();
                process.StartInfo = psi;
                process.Start();

                // 打开Conda环境
                string condaCommand = "conda activate py38";
                process.StandardInput.WriteLine(condaCommand);

                // 执行TensorBoard程序命令
                string tensorboardCommand = "tensorboard --logdir=runs";
                process.StandardInput.WriteLine(tensorboardCommand);

                // 创建新线程来等待进程退出
                Thread processThread = new Thread(() =>
                {
                    // 等待进程退出
                    process.WaitForExit();

                    // 获取进程输出
                    string output = process.StandardOutput.ReadToEnd();
                    Console.WriteLine(output);

                    // 释放互斥锁
                    mutex.ReleaseMutex();
                    mutex.Close();
                });

                // 启动线程
                processThread.Start();

                // 不等待退出
                process.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine("发生异常：" + ex.Message);
            }

        }
        static public Tensor Mat2Tensor(Mat img)
        {
            //img = img.Resize(ImageSize, 0, 0, InterpolationFlags.Area);

            var imgData = new byte[3 * img.Rows * img.Cols];

            Parallel.For(0, img.Rows, i =>
            {
                for (int j = 0; j < img.Cols; j++)
                {
                    int p = (i * img.Cols + j) * 3;
                    // 计算当前像素在imgData数组中的索引

                    imgData[p] = img.At<Vec3b>(i, j)[2];
                    imgData[p + 1] = img.At<Vec3b>(i, j)[1];
                    imgData[p + 2] = img.At<Vec3b>(i, j)[0];
                    // 将当前像素的RGB通道值存储到imgData数组中
                }
            });

            // 将imgData中的数据按照R、G、B的顺序排列
            var imgDataRGB = new byte[3 * img.Rows * img.Cols];
            Parallel.For(0, img.Rows * img.Cols, i =>
            {
                imgDataRGB[i] = imgData[i * 3 + 2]; // R
                imgDataRGB[i + img.Rows * img.Cols] = imgData[i * 3 + 1]; // G
                imgDataRGB[i + 2 * img.Rows * img.Cols] = imgData[i * 3]; // B
            });

            // 创建图像张量
            var imageTensor = torch.tensor(imgDataRGB, new long[] { 3, img.Rows, img.Cols })
                .to_type(torch.float32)
                .div(255f);
            return imageTensor;
        }
        static public Tensor GetTensorFromImageFile(string file)
        {

            var img = Cv2.ImRead(file, ImreadModes.Color);

            // 如果读取失败，尝试使用不同的ImreadModes再次读取
            if (img.Empty())
            {
                img = Cv2.ImRead(file, ImreadModes.Grayscale);
            }
            if (img.Empty())
            {
                img = Cv2.ImRead(file, ImreadModes.AnyColor);
            }
            if (img.Empty())
            {
                // 如果所有的ImreadModes都失败了，抛出异常
                throw new Exception($"Failed to read image file {file}");
            }

            // 创建图像张量
            var imageTensor = Mat2Tensor(img);
            return imageTensor;
        }
        static public OpenCvSharp.Mat Resize(Mat img_mat, int w_new, int h_new)
        {
            //如果img_mat没有图像，则退出
            if (img_mat.Empty())
                return img_mat;

            Mat output = null;
            var 原图比例 = img_mat.Width / img_mat.Height;
            var 新图比例 = w_new / h_new;


            if (w_new > h_new)
            {
                //宽图片，处理高度
                var temp_height = img_mat.Height * ((float)w_new / img_mat.Width);
                var size = new OpenCvSharp.Size(w_new, temp_height);
                output = img_mat.Resize(size);

            }
            else
            {
                //高图片，处理宽度
                var temp_width = img_mat.Width * ((float)h_new / img_mat.Height);
                var size = new OpenCvSharp.Size(temp_width, h_new);
                output = img_mat.Resize(size);


            }
            var top_bottom = 0;
            var left_right = 0;
            if (h_new > output.Height)
                top_bottom = (h_new - output.Height) / 2;
            if (w_new > output.Width)
                left_right = (w_new - output.Width) / 2;
            if (top_bottom > 0 || left_right > 0)
                output = output.CopyMakeBorder(top_bottom, top_bottom, left_right, left_right, BorderTypes.Constant, OpenCvSharp.Scalar.Black);


            return output;
        }
        public class DetectTraget
        {
            public int id;
            public string name;
            public double cx;
            public double cy;
            public double width;
            public double height;
        }

        static public void Log(string msg)
        {
            Console.WriteLine(msg);
        }

        static public void PrintData(string mode, Tensor tensordata)
        {
            // 获取tensordata的维度
            var shape = tensordata.shape;

            // 获取tensordata的数据
            var data = tensordata.data<float>();
            // 将数据转换为List
            var data_list = data.ToList();
            // 将List转换为string，并按维度数据进行换行显示
            var data_format = "";

            if (shape.Length == 0)
            {
                data_format = $"[{mode}]: {data_list[0].ToString()}";
                Console.WriteLine(data_format);
            }
            else
            {
                var currentIndex = 0;
                for (int i = 0; i < shape[0]; i++)
                {
                    for (int j = 0; j < shape[1]; j++)
                    {
                        var index = i * shape[1] + j;
                        data_format += $"[{index}]: {data_list[currentIndex].ToString()}  ";
                        currentIndex++;
                    }
                    data_format += "\n";
                }

                Console.WriteLine($"[{mode}]:\n{data_format}");
            }


        }
    }
}

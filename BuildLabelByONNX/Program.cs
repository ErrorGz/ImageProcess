using System.Linq;
using System.IO;
using SixLabors.ImageSharp;
using Compunet.YoloV8;
using System.Text;
using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;

string ONNXFile = "yolov8n-pose.onnx";
Console.WriteLine($"加载{ONNXFile}模型");


YoloV8 model = new YoloV8(ONNXFile);
Console.WriteLine("加载完毕！");

string ImageFilePathRoot = args.FirstOrDefault(Directory.Exists);

while (ImageFilePathRoot == null)
{
    Console.Write("请输入图片文件夹：");
    ImageFilePathRoot = Console.ReadLine();
    if (Directory.Exists(ImageFilePathRoot))
        break;
    else
    {
        ImageFilePathRoot = null;
        Console.WriteLine("图片文件夹不存在,请重新输入");
    }
}

Console.WriteLine($"你输入的图片文件夹为：{ImageFilePathRoot}");



string[] AllFile = Directory.GetFiles(ImageFilePathRoot, "*.*", SearchOption.AllDirectories);
string[] ImageFileFormat = { ".jpg", ".png" };
string[] ImageFilePaths = AllFile.Where(x => ImageFileFormat.Contains(Path.GetExtension(x))).ToArray();

int 已处理Count = 0;
Stopwatch sw1 = new Stopwatch();
Stopwatch sw2 = new Stopwatch();

foreach (string imagefilepath in ImageFilePaths)
{
    Console.WriteLine($"检测：{imagefilepath}");
    sw1.Restart();
    var image = Image.Load(imagefilepath);
    sw1.Stop();

    sw2.Restart();
    var result = model.Pose(image);
    sw2.Stop();

    Console.WriteLine($"加载图片用时：{sw1.ElapsedMilliseconds}ms  检测模型用时：{sw2.ElapsedMilliseconds}ms");

    StringBuilder textBuilder = new StringBuilder();
    foreach (var box in result.Boxes)
    {
        var imagewidth = result.Image.Width;
        var imageheight = result.Image.Height;

        textBuilder.Append(box.Class.Id);
        textBuilder.Append(" ");
        textBuilder.Append(((double)box.Bounds.X + box.Bounds.Width / 2) / imagewidth);
        textBuilder.Append(" ");
        textBuilder.Append(((double)box.Bounds.Y + box.Bounds.Height / 2) / imageheight);
        textBuilder.Append(" ");
        textBuilder.Append((double)box.Bounds.Width / imagewidth);
        textBuilder.Append(" ");
        textBuilder.Append((double)box.Bounds.Height / imageheight);
        textBuilder.Append(" ");
        int[] keypintindex = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        for (int i = 0; i < 10; i++)
        {
            textBuilder.Append((double)box.Keypoints[i].Point.X / imagewidth);
            textBuilder.Append(" ");
            textBuilder.Append((double)box.Keypoints[i].Point.Y / imageheight);
            textBuilder.Append(" ");
        }
        textBuilder.AppendLine();
    }
    已处理Count++;
    if (textBuilder.Length == 0)
    {
        Console.WriteLine("检测结果空白。");
        continue;
    }
    string resultText = textBuilder.ToString();
    Console.WriteLine($"检测结果：{resultText}");
    string path = Path.GetDirectoryName(imagefilepath);
    string TextFile = Path.Combine(path, Path.GetFileNameWithoutExtension(imagefilepath) + ".txt");
    Console.WriteLine($"保存结果到：{TextFile}");
    File.WriteAllText(TextFile, resultText);

    Console.WriteLine($"已处理{已处理Count}/{ImageFilePaths.Length}张图片");

}


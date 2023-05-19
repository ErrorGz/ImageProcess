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

namespace ImageTrain
{
     public class StaticLib
    {
        static public void Log(string msg)
        {
            Console.WriteLine(msg);    
        }
    }
}

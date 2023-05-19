using System;
using System.Collections.Generic;
using System.Drawing;
using Yolo5.NetCore.Models;

namespace Yolo5.NetCore.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            // Single frame / image example.
            ImageExample.Run();

            // Web camera example.
            CameraExample.Run();
        }
    }
}

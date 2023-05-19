using OpenCvSharp;
using System.Drawing;

namespace Yolo5.NetCore.Extensions
{
    public static class RectangleExtensions
    {
        static public float Area(this Rect2f r)
        {
            return r.Width * r.Height;
        }
    }
}

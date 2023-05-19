using OpenCvSharp;
using System.Drawing;

namespace Yolo5.NetCore.Models
{
    public class YoloPrediction
    {
        //public YoloLabelModel Label { get; set; }
        public int LabelId { get; set; }
        //public RectangleF Rectangle { get; set; }
        public Rect2f Rectangle { get; set; }        
        public float Score { get; set; }

        public YoloPrediction(int label, float confidence) : this(label)
        {
            Score = confidence;
        }

        public YoloPrediction(int label)
        {
            LabelId = label;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VideoLabel
{
    internal class Lib
    {
    }

    public class VideoProject
    {
        public List<VideoData> VideoDatas { get; set; } = new List<VideoData>();
    }
    public class VideoData
    {
        public string VideoFile { get; set; }
        public List<VideoLabel> frames { get; set; } = new List<VideoLabel>();
    }
    public class VideoLabel
    {
        public int VideoLabelId { get; set; }
        public int FrameStart { get; set; }
        public int FrameCount { get; set; } = 10;
        public int FrameStep { get; set; } 

    }


    static public class StaticLib
    {
        static public string[] VideoFile = { ".mp4", ".avi", ".mkv", ".wmv", ".flv", ".mov", ".rmvb", ".rm", ".3gp", ".dat", ".ts", ".mts", ".vob" };

    }
}

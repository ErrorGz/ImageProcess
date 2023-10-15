using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace VideoLabel
{


    public class VideoProject
    {
        public List<VideoData> VideoDatas { get; set; } = new List<VideoData>();
        public Dictionary<int, string> Labels { get; set; } = new Dictionary<int, string>();
    }
    public class VideoData
    {
        public string VideoURL { get; set; }
        public List<VideoLabel> frames { get; set; } = new List<VideoLabel>();

        public bool isRTSP { get; set; }
        public bool isURB { get; set; }

        [YamlIgnore]
        public VideoCapture capture { get; set; }


    }
    public class VideoLabel
    {
        public int VideoLabelId { get; set; }
        public int FrameStart { get; set; }
        public int FrameCount { get; set; } = 10;
        public int FrameStep { get; set; }

    }


    public static class DictionaryExtensions
    {
        public static int AddOrGetKey<TKey, TValue>(this Dictionary<int, TValue> dict, TValue value)
        {
            int key = dict.FirstOrDefault(x => EqualityComparer<TValue>.Default.Equals(x.Value, value)).Key;
            if (key == default(int) && !dict.ContainsValue(value))
            {
                key = dict.Keys.Count > 0 ? dict.Keys.Max() + 1 : 0;  // Change 1 to 0 as the starting key
                dict.Add(key, value);
            }
            return key;
        }
    }

    static public class StaticLib
    {

        static public string[] VideoFile = { ".mp4", ".avi", ".mkv", ".wmv", ".flv", ".mov", ".rmvb", ".rm", ".3gp", ".dat", ".ts", ".mts", ".vob" };
        static public void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
    }
}

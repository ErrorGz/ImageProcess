using Compunet.YoloV8.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;


namespace ImageLabel
{
    public class TrackData
    {
        public IDetectionResult Last_Prediction { get; set; }
        public IDetectionResult Current_Prediction { get; set; }

        public double Current_FrameSecond { get; set; }
        public bool? Flag_Move { get; set; }
    }
    //public class Track
    //{
    //    public Action<TrackData> On出现;
    //    public Action<TrackData> On消失;
    //    public Action<TrackData> On移动;
    //    public Action<TrackData> On停止;

    //    Threshold Threshold;

    //    List<TrackData> tracks = new List<TrackData>();
    //    public void Update(double fs, IDetectionResult prediction)
    //    {
    //        //var last_prediction = tracks.Select(o => o.Current_Prediction).ToList();
    //        var current_prediction = prediction.Select(o => new TrackData { Current_Prediction = o });
    //        var join_trackdata = MoreLinq.Extensions.FullJoinExtension.FullJoin<TrackData, int, TrackData>(
    //            tracks,
    //            current_prediction,
    //            o => o.Last_Prediction != null ? o.Last_Prediction.LabelId : o.Current_Prediction != null ? o.Current_Prediction.LabelId : 0,
    //            p => new TrackData
    //            {
    //                Last_Prediction = p.Current_Prediction,
    //                Current_Prediction = (YoloPrediction)null,
    //                Current_FrameSecond = fs,
    //                Flag_Move = p.Flag_Move
    //            },
    //            q => new TrackData
    //            {
    //                Last_Prediction = (YoloPrediction)null,
    //                Current_Prediction = q.Current_Prediction,
    //                Current_FrameSecond = fs,
    //                Flag_Move = q.Flag_Move
    //            },
    //            (p, q) => new TrackData
    //            {
    //                Last_Prediction = p.Current_Prediction,
    //                Current_Prediction = q.Current_Prediction,
    //                Current_FrameSecond = fs,
    //                Flag_Move = p.Flag_Move

    //            }).ToList();

    //        //foreach(var td in join_trackdata)
    //        //{
    //        //    var msg = $"time:{td.Current_FrameSecond:n2} label ID:{(td.Current_Prediction??td.Last_Prediction).LabelId},flag_move:{td.Flag_Move}";
    //        //    Debug.WriteLine(msg);
    //        //}
    //        //Debug.WriteLine("");

    //        foreach (var td in join_trackdata)
    //        {
    //            var LabelId = td.Last_Prediction != null ? td.Last_Prediction.LabelId : td.Current_Prediction != null ? td.Current_Prediction.LabelId : -1;
    //            if (On出现 != null && td.Last_Prediction == null && td.Current_Prediction != null)
    //            {
    //                On出现(td);
    //            }
    //            if (On消失 != null && td.Last_Prediction != null && td.Current_Prediction == null)
    //            {
    //                On消失(td);
    //            }
    //            if (On移动 != null && On停止 != null && td.Last_Prediction != null && td.Current_Prediction != null)
    //            {
    //                (var last_cx, var last_cy) = (td.Last_Prediction.Rectangle.X + td.Last_Prediction.Rectangle.Width / 2, td.Last_Prediction.Rectangle.Y + td.Last_Prediction.Rectangle.Height / 2);
    //                (var curr_cx, var curr_cy) = (td.Current_Prediction.Rectangle.X + td.Current_Prediction.Rectangle.Width / 2, td.Current_Prediction.Rectangle.Y + td.Current_Prediction.Rectangle.Height / 2);
    //                (var len_cx, var len_cy) = (Math.Abs(curr_cx - last_cx), Math.Abs(curr_cy - last_cy));
    //                (var scale_cx, var scale_cy) = (len_cx / td.Last_Prediction.Rectangle.Width, len_cy / td.Last_Prediction.Rectangle.Height);
    //                (var scale_w, var scale_h) = (td.Current_Prediction.Rectangle.Width / td.Last_Prediction.Rectangle.Width,
    //                                             td.Current_Prediction.Rectangle.Height / td.Last_Prediction.Rectangle.Height);

    //                Debug.Write($"{fs:N2} {LabelId}:");
    //                Debug.Write($"len_cx:[{len_cx}]>[{Threshold.移动阈值宽度}]=[{len_cx > Threshold.移动阈值宽度}]  ");
    //                Debug.Write($"len_cy:[{len_cy}]>[{Threshold.移动阈值高度}]=[{len_cy > Threshold.移动阈值高度}]  ");
    //                Debug.Write($"scale_w:[{scale_w}]>[{Threshold.放大阈值宽度 / 100f}]=[{scale_w > Threshold.放大阈值宽度 / 100f}]  ");
    //                Debug.Write($"scale_h:[{scale_h}]>[{Threshold.放大阈值高度 / 100f}]=[{scale_h > Threshold.放大阈值高度 / 100f}]  ");
    //                Debug.Write($"scale_w:[{scale_w}]<[{Threshold.缩小阈值宽度 / 100f}]=[{scale_w < Threshold.缩小阈值宽度 / 100f}]  ");
    //                Debug.Write($"scale_h:[{scale_h}]<[{Threshold.缩小阈值高度 / 100f}]=[{scale_h < Threshold.缩小阈值高度 / 100f}]  ");
    //                Debug.WriteLine("");
    //                if (len_cx > Threshold.移动阈值宽度 || len_cy > Threshold.移动阈值高度 || scale_w > Threshold.放大阈值宽度 / 100f || scale_h > Threshold.放大阈值高度 / 100f || scale_w < Threshold.缩小阈值宽度 / 100f || scale_h < Threshold.缩小阈值高度 / 100f)
    //                {

    //                    if (td.Flag_Move != true)
    //                    {
    //                        On移动(td);
    //                        td.Flag_Move = true;

    //                    }
    //                }
    //                else
    //                {
    //                    if (td.Flag_Move != false)
    //                    {
    //                        On停止(td);
    //                        td.Flag_Move = false;
    //                    }
    //                }
    //            }
    //        }
    //        tracks = join_trackdata;

    //    }


    //    public Track(Threshold t)
    //    {
    //        Threshold = t;
    //    }


    //}


    //public class TrackModel
    //{

    //}
}

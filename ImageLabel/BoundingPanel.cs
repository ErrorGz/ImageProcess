using DevExpress.CodeParser;
using DevExpress.DataAccess.Wizard;
using DXApplication;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using static ImageLabel.BoundingPanelContainer;
using Color = System.Drawing.Color;

namespace ImageLabel
{
    public class BoundingPanel : Panel
    {
        public delegate void BoundingPanelEventHandler(object sender, BoxClass e);
        public event BoundingPanelEventHandler OnDataChanging;
        public event BoundingPanelEventHandler OnDataChanged;

        public delegate void BoundingPanel_SelectObject(object sender, object o);
        public event BoundingPanel_SelectObject OnSelectObject;

        public event EventHandler DeleteClick;  //声明一个事件 

        private bool Drawing = false;

        public OperatingModes OperatingMode { get; set; }


        public ImageDataClass idc { get; set; }
        public BoxClass box { get; set; }


        private double _scalefactor;

        public double ScaleFactor
        {
            get
            {
                return _scalefactor;
            }
            set
            {
                _scalefactor = value;
                if (idc != null)
                {
                    this.Invalidate();
                    //Debug.WriteLine($"x/y:({_scalefactor * box.x * idc.ImageWidth},{_scalefactor * box.y * idc.ImageHeight})  w/h:({_scalefactor * box.width * idc.ImageWidth},{_scalefactor * box.height * idc.ImageHeight})");
                }
            }
        }




        public BoundingPanel()
        {
            InitializeComponent();
            this.MouseDown += MyMouseDown;
            this.MouseLeave += MyMouseLeave;
            this.MouseMove += MyMouseMove;
            this.MouseUp += MyMouseUp;
            this.Paint += BoundingPanel_Paint;

            this.ClientSizeChanged += new EventHandler((s, e) => { this.Invalidate(); });
            this.ContextMenuStrip = new ContextMenuStrip();
            this.ContextMenuStrip.Items.Add("删除").Click += new EventHandler((s, e) =>
            {
                idc.ObjectBox.Remove(box);
                if (DeleteClick != null)
                {
                    DeleteClick(this, null);
                }
            });

        }


        private void BoundingPanel_Paint(object sender, PaintEventArgs e)
        {
            //base.OnPaint(e);
            this.Drawing = true;
            if (Parent != null)
            {
                using (var parentBmp = new Bitmap((int)(Parent.Width * ScaleFactor), (int)(Parent.Height * ScaleFactor)))
                {
                    using (var parentGraphics = Graphics.FromImage(parentBmp))
                    {
                        parentGraphics.DrawImage(parentBmp, 0, 0);
                    }


                    e.Graphics.DrawImage(parentBmp, new Rectangle(Location, Size));
                    //Debug.WriteLine($"Location:{Location}  Size:{Size}");
                }
            }

            e.Graphics.SetClip(e.ClipRectangle); // 设置绘制区域为无效区域
            e.Graphics.DrawRectangle(new System.Drawing.Pen(lib.Colors[box.Id % 32], 4), e.ClipRectangle);
            //Debug.WriteLine($"box_pos.Y:{box_pos.Y},e.ClipRectangle.Top:{e.ClipRectangle.Top}");
            for (int i = 0; i < box.points.Count; i++)
            {
                var point = box.points[i];
                const int r = 4;
                (var x, var y, var w, var h) = ((int)(this.ScaleFactor * point.x * idc.ImageWidth - this.Left - r / 2),
                                                (int)(this.ScaleFactor * point.y * idc.ImageHeight - this.Top - r / 2),
                                                r, r);

                var brush = new SolidBrush(lib.Colors[box.Id % 32]);
                Font font = new Font("微软雅黑", 8f, FontStyle.Regular);
                e.Graphics.FillEllipse(brush, x, y, w, h);
                string msg = point.c == 0 ? $"{i} 无效" : point.c == 1 ? $"{i} 不可见" : $"{i}";
                SizeF textSize = e.Graphics.MeasureString(msg, font);
                e.Graphics.DrawString(msg, font, brush, x - textSize.Width / 2, y + textSize.Height/2);

            }


            if (box.points.Count == 17)
            {
                e.Graphics.DrawLine(new Pen(lib.Colors[box.Id % 32]), GetImagePoint(box.points[0]), GetImagePoint(box.points[1]));
                e.Graphics.DrawLine(new Pen(lib.Colors[box.Id % 32]), GetImagePoint(box.points[0]), GetImagePoint(box.points[2]));
                e.Graphics.DrawLine(new Pen(lib.Colors[box.Id % 32]), GetImagePoint(box.points[1]), GetImagePoint(box.points[3]));
                e.Graphics.DrawLine(new Pen(lib.Colors[box.Id % 32]), GetImagePoint(box.points[2]), GetImagePoint(box.points[4]));

                e.Graphics.DrawLines(new Pen(lib.Colors[box.Id % 32]), new PointF[] { GetImagePoint(box.points[5]), GetImagePoint(box.points[7]), GetImagePoint(box.points[9]) });
                e.Graphics.DrawLines(new Pen(lib.Colors[box.Id % 32]), new PointF[] { GetImagePoint(box.points[6]), GetImagePoint(box.points[8]), GetImagePoint(box.points[10]) });
                e.Graphics.DrawLines(new Pen(lib.Colors[box.Id % 32]), new PointF[] { GetImagePoint(box.points[11]), GetImagePoint(box.points[13]), GetImagePoint(box.points[15]) });
                e.Graphics.DrawLines(new Pen(lib.Colors[box.Id % 32]), new PointF[] { GetImagePoint(box.points[12]), GetImagePoint(box.points[14]), GetImagePoint(box.points[16]) });

            }
            Drawing = false;
        }

        private PointF GetImagePoint(PointClass point)
        {
            var p = new PointF((float)(this.ScaleFactor * point.x * idc.ImageWidth - this.Left), (float)(this.ScaleFactor * point.y * idc.ImageHeight - this.Top));
            return p;
        }

        public void SetBox(ImageDataClass idc, BoxClass boxclass)
        {
            this.idc = idc;
            this.box = boxclass;
            this.SetBounds(
                (int)(this.ScaleFactor * box.x * idc.ImageWidth),
                (int)(this.ScaleFactor * box.y * idc.ImageHeight),
                (int)(this.ScaleFactor * box.width * idc.ImageWidth),
                (int)(this.ScaleFactor * box.height * idc.ImageHeight));
        }

        //1、定义一个枚举类型，描述光标状态
        public enum EnumMousePointPosition
        {
            MouseSizeNone = 0, //'无
            MouseSizeRight = 1, //'拉伸右边框
            MouseSizeLeft = 2, //'拉伸左边框
            MouseSizeBottom = 3, //'拉伸下边框
            MouseSizeTop = 4, //'拉伸上边框
            MouseSizeTopLeft = 5, //'拉伸左上角
            MouseSizeTopRight = 6, //'拉伸右上角
            MouseSizeBottomLeft = 7, //'拉伸左下角
            MouseSizeBottomRight = 8, //'拉伸右下角
            MouseDrag = 9, // '鼠标拖动
            MouseDragPoint = 10

        }

        //2、定义几个变量
        public const int Band = 5;
        public const int MinWidth = 10;
        public const int MinHeight = 10;
        private EnumMousePointPosition m_MousePointPosition;
        private ImageLabel.PointClass m_Point;


        private Point p原点, p移动点;
        //3、定义自己的MyMouseDown事件
        private void MyMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Debug.WriteLine($"BoundingPanel_MouseDown:{OperatingMode.ToString()}");

            p原点.X = e.X;
            p原点.Y = e.Y;
            p移动点.X = e.X;
            p移动点.Y = e.Y;
            if (this.OperatingMode == OperatingModes.MoveBounding)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (m_MousePointPosition == EnumMousePointPosition.MouseDrag)
                    {
                        OnSelectObject?.Invoke(this, box);
                    }
                    else if (m_MousePointPosition == EnumMousePointPosition.MouseDragPoint)
                    {
                        OnSelectObject?.Invoke(this, m_Point);
                    }
                }
            }

            if (this.OperatingMode == OperatingModes.AddBounding)
            {
                if (e.Button == MouseButtons.Left)
                {

                }
            }

            if (this.OperatingMode == OperatingModes.AddPoint)
            {
                if (e.Button == MouseButtons.Left)
                {

                }
            }

        }
        //4、定义自己的MyMouseLeave事件
        private void MyMouseLeave(object sender, System.EventArgs e)
        {
            m_MousePointPosition = EnumMousePointPosition.MouseSizeNone;
            this.Cursor = Cursors.Arrow;
            OnDataChanged?.Invoke(this, box);
        }
        //5、设计一个函数，确定光标在控件不同位置的样式
        private (EnumMousePointPosition, object) MousePointPosition(Size size, System.Windows.Forms.MouseEventArgs e)
        {
            if (this.OperatingMode == OperatingModes.MoveBounding)
            {
                foreach (var point in this.box.points)
                {
                    var p_rect = new RectangleF(
                             (float)(this.ScaleFactor * point.x * this.idc.ImageWidth - this.Left - Band),
                             (float)(this.ScaleFactor * point.y * this.idc.ImageHeight - this.Top - Band),
                             (float)Band * 2,
                             (float)Band * 2);

                    if (p_rect.Contains(e.Location))
                    {
                        return (EnumMousePointPosition.MouseDragPoint, point);
                    }
                }
                if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
                {
                    if (e.X < Band)
                    {
                        if (e.Y < Band)
                        {
                            return (EnumMousePointPosition.MouseSizeTopLeft, null);
                        }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return (EnumMousePointPosition.MouseSizeBottomLeft, null); }
                            else
                            { return (EnumMousePointPosition.MouseSizeLeft, null); }
                        }
                    }
                    else
                    {
                        if (e.X > -1 * Band + size.Width)
                        {
                            if (e.Y < Band)
                            { return (EnumMousePointPosition.MouseSizeTopRight, null); }
                            else
                            {
                                if (e.Y > -1 * Band + size.Height)
                                { return (EnumMousePointPosition.MouseSizeBottomRight, null); }
                                else
                                { return (EnumMousePointPosition.MouseSizeRight, null); }
                            }
                        }
                        else
                        {
                            if (e.Y < Band)
                            { return (EnumMousePointPosition.MouseSizeTop, null); }
                            else
                            {
                                if (e.Y > -1 * Band + size.Height)
                                { return (EnumMousePointPosition.MouseSizeBottom, null); }
                                else
                                { return (EnumMousePointPosition.MouseDrag, null); }
                            }
                        }
                    }
                }
                else
                { return (EnumMousePointPosition.MouseSizeNone, null); }
            }
            else if (this.OperatingMode == OperatingModes.AddBounding)
            {
                foreach (var point in this.box.points)
                {
                    var p_rect = new RectangleF(
                             (float)(this.ScaleFactor * point.x * this.idc.ImageWidth - this.Left - Band),
                             (float)(this.ScaleFactor * point.y * this.idc.ImageHeight - this.Top - Band),
                             (float)Band * 2,
                             (float)Band * 2);

                    if (p_rect.Contains(e.Location))
                    {
                        return (EnumMousePointPosition.MouseDragPoint, point);
                    }
                }

                if ((e.X >= -1 * Band) | (e.X <= size.Width) | (e.Y >= -1 * Band) | (e.Y <= size.Height))
                {
                    if (e.X < Band)
                    {
                        if (e.Y < Band)
                        {
                            return (EnumMousePointPosition.MouseSizeTopLeft, null);
                        }
                        else
                        {
                            if (e.Y > -1 * Band + size.Height)
                            { return (EnumMousePointPosition.MouseSizeBottomLeft, null); }
                            else
                            { return (EnumMousePointPosition.MouseSizeLeft, null); }
                        }
                    }
                    else
                    {
                        if (e.X > -1 * Band + size.Width)
                        {
                            if (e.Y < Band)
                            { return (EnumMousePointPosition.MouseSizeTopRight, null); }
                            else
                            {
                                if (e.Y > -1 * Band + size.Height)
                                { return (EnumMousePointPosition.MouseSizeBottomRight, null); }
                                else
                                { return (EnumMousePointPosition.MouseSizeRight, null); }
                            }
                        }
                        else
                        {
                            if (e.Y < Band)
                            { return (EnumMousePointPosition.MouseSizeTop, null); }
                            else
                            {
                                if (e.Y > -1 * Band + size.Height)
                                { return (EnumMousePointPosition.MouseSizeBottom, null); }
                                else
                                { return (EnumMousePointPosition.MouseDrag, null); }
                            }
                        }
                    }
                }
                else
                { return (EnumMousePointPosition.MouseSizeNone, null); }
            }
            else if (this.OperatingMode == OperatingModes.AddPoint)
            {

                return (EnumMousePointPosition.MouseSizeNone, null);
            }
            return (EnumMousePointPosition.MouseSizeNone, null);
        }

        //6、定义自己的MyMouseMove事件，在这个事件里，会使用上面设计的函数
        private void MyMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Control ctrl = (sender as Control);
            if (e.Button == MouseButtons.None)
            {
                (m_MousePointPosition, var object_point) = MousePointPosition(ctrl.Size, e); //'判断光标的位置状态
                switch (m_MousePointPosition) //'改变光标
                {
                    case EnumMousePointPosition.MouseSizeNone:
                        this.Cursor = Cursors.Arrow;       //'箭头
                        break;
                    case EnumMousePointPosition.MouseDragPoint:
                        m_Point = object_point as PointClass;
                        this.Cursor = Cursors.Arrow;
                        break;
                    case EnumMousePointPosition.MouseDrag:
                        this.Cursor = Cursors.SizeAll;     //'四方向
                        break;
                    case EnumMousePointPosition.MouseSizeBottom:
                        this.Cursor = Cursors.SizeNS;      //'南北
                        break;
                    case EnumMousePointPosition.MouseSizeTop:
                        this.Cursor = Cursors.SizeNS;      //'南北
                        break;
                    case EnumMousePointPosition.MouseSizeLeft:
                        this.Cursor = Cursors.SizeWE;      //'东西
                        break;
                    case EnumMousePointPosition.MouseSizeRight:
                        this.Cursor = Cursors.SizeWE;      //'东西
                        break;
                    case EnumMousePointPosition.MouseSizeBottomLeft:
                        this.Cursor = Cursors.SizeNESW;    //'东北到南西
                        break;
                    case EnumMousePointPosition.MouseSizeBottomRight:
                        this.Cursor = Cursors.SizeNWSE;    //'东南到西北
                        break;
                    case EnumMousePointPosition.MouseSizeTopLeft:
                        this.Cursor = Cursors.SizeNWSE;    //'东南到西北
                        break;
                    case EnumMousePointPosition.MouseSizeTopRight:
                        this.Cursor = Cursors.SizeNESW;    //'东北到南西
                        break;
                    default:
                        break;
                }
            }

            if (this.OperatingMode == OperatingModes.MoveBounding)
            {
                if (e.Button == MouseButtons.Left)
                {
                    switch (m_MousePointPosition)
                    {
                        case EnumMousePointPosition.MouseDragPoint:
                            m_Point.x = (double)(e.X + this.Left) / this.ScaleFactor / (this.idc.ImageWidth);
                            m_Point.y = (double)(e.Y + this.Top) / this.ScaleFactor / (this.idc.ImageHeight);
                            if (this.Drawing == false)
                                this.Invalidate();
                            OnSelectObject?.Invoke(this, m_Point);
                            OnDataChanging?.Invoke(this, box);

                            break;
                        case EnumMousePointPosition.MouseDrag:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Top = ctrl.Top + e.Y - p原点.Y;
                            OnSelectObject?.Invoke(this, box);
                            break;
                        case EnumMousePointPosition.MouseSizeBottom:
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeBottomRight:
                            ctrl.Width = ctrl.Width + e.X - p移动点.X;
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeRight:
                            ctrl.Width = ctrl.Width + e.X - p移动点.X;
                            //      lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTop:
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            break;
                        case EnumMousePointPosition.MouseSizeLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            break;
                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTopRight:
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Width = ctrl.Width + (e.X - p移动点.X);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTopLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            break;
                        default:
                            break;
                    }
                    if (ctrl.Width < MinWidth) ctrl.Width = MinWidth;
                    if (ctrl.Height < MinHeight) ctrl.Height = MinHeight;

                    lib.NormalizeRectangle(this.ScaleFactor, ctrl.Bounds, new Size((int)idc.ImageWidth, (int)idc.ImageHeight), box);
                    OnDataChanging?.Invoke(this, box);
                }
            }
            else if (this.OperatingMode == OperatingModes.AddBounding)
            {
                if (e.Button == MouseButtons.Left)
                {
                    switch (m_MousePointPosition)
                    {
                        case EnumMousePointPosition.MouseDragPoint:
                            m_Point.x = (double)(e.X + this.Left) / this.ScaleFactor / (this.idc.ImageWidth);
                            m_Point.y = (double)(e.Y + this.Top) / this.ScaleFactor / (this.idc.ImageHeight);
                            if (this.Drawing == false)
                                this.Invalidate();
                            OnDataChanging?.Invoke(this, box);
                            break;
                        case EnumMousePointPosition.MouseDrag:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Top = ctrl.Top + e.Y - p原点.Y;
                            break;
                        case EnumMousePointPosition.MouseSizeBottom:
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeBottomRight:
                            ctrl.Width = ctrl.Width + e.X - p移动点.X;
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeRight:
                            ctrl.Width = ctrl.Width + e.X - p移动点.X;
                            //      lCtrl.Height = lCtrl.Height + e.Y - p1.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTop:
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            break;
                        case EnumMousePointPosition.MouseSizeLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            break;
                        case EnumMousePointPosition.MouseSizeBottomLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            ctrl.Height = ctrl.Height + e.Y - p移动点.Y;
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTopRight:
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Width = ctrl.Width + (e.X - p移动点.X);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            p移动点.X = e.X;
                            p移动点.Y = e.Y; //'记录光标拖动的当前点
                            break;
                        case EnumMousePointPosition.MouseSizeTopLeft:
                            ctrl.Left = ctrl.Left + e.X - p原点.X;
                            ctrl.Top = ctrl.Top + (e.Y - p原点.Y);
                            ctrl.Width = ctrl.Width - (e.X - p原点.X);
                            ctrl.Height = ctrl.Height - (e.Y - p原点.Y);
                            break;
                        default:
                            break;
                    }
                    if (ctrl.Width < MinWidth) ctrl.Width = MinWidth;
                    if (ctrl.Height < MinHeight) ctrl.Height = MinHeight;

                    lib.NormalizeRectangle(this.ScaleFactor, ctrl.Bounds, new Size((int)idc.ImageWidth, (int)idc.ImageHeight), box);
                    OnDataChanging?.Invoke(this, box);
                }
            }
            else if (this.OperatingMode == OperatingModes.AddPoint)
            {
                if (e.Button == MouseButtons.Left)
                {

                }
            }





        }


        private void MyMouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine($"BoundingPanel_MouseUp:{OperatingMode.ToString()}");

            if (this.OperatingMode == OperatingModes.MoveBounding)
            {
                if (e.Button == MouseButtons.Left)
                {
                    OnDataChanged?.Invoke(this, box);
                }
            }
            else if (this.OperatingMode == OperatingModes.AddBounding)
            {
                if (e.Button == MouseButtons.Left)
                {
                    OnDataChanged?.Invoke(this, box);
                }
            }
            else if (this.OperatingMode == OperatingModes.AddPoint)
            {
                if (e.Button == MouseButtons.Left)
                {
                    this.box.AddPoint(new PointClass() { x = (double)(e.X + this.Left) / this.ScaleFactor / (this.idc.ImageWidth), y = (double)(e.Y + this.Top) / this.ScaleFactor / (this.idc.ImageHeight), c = 2 });
                    this.Invalidate();
                    OnDataChanged?.Invoke(this, box);
                }
            }
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;

            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ResumeLayout(false);

        }


    }
}

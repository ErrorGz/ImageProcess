using DevExpress.Mvvm.Native;
using DevExpress.Pdf.Native;
using DevExpress.XtraRichEdit.Export;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Windows.Forms;

namespace ImageLabel
{
    public partial class BoundingPanelContainer : Panel
    {
        public ImageDataClass idc;
        private BoundingPanel tempBoundingPanel = null;
        public int LabelId { get; set; }
        public string LabelName { get; set; }
        //private bool _readonly = false;

        bool MouseDownFlag = false;
        System.Drawing.Point MouseDownLoc = new System.Drawing.Point();
        System.Drawing.Point MouseMoveLoc = new System.Drawing.Point();

        public delegate void BoundingPanelContainerEventHandler(object sender, ImageDataClass e);
        public event BoundingPanelContainerEventHandler OnDataChanging;
        public event BoundingPanelContainerEventHandler OnDataChanged;

        public delegate void BoundingPanelContainer_SelectObject(object sender, object o);
        public event BoundingPanelContainer_SelectObject OnSelectObject;

        private OperatingModes _operatingmode = OperatingModes.MoveBounding;

        public OperatingModes OperatingMode
        {
            get { return _operatingmode; }
            set
            {
                _operatingmode = value;
                foreach (var ctrl in Controls.Cast<BoundingPanel>())
                {
                    ctrl.OperatingMode = _operatingmode;
                }
            }
        }

        private Image CurrentImage;
        private float ImageScalefactor = 1.0f;

        public BoundingPanelContainer()
        {
            InitializeComponent();
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.ResizeRedraw, true);
            this.DoubleBuffered = true;
            this.BackColor = Color.Transparent;


            this.AutoScroll = true;

            this.MouseDown += BoundingPanelContainer_MouseDown;
            this.MouseMove += BoundingPanelContainer_MouseMove;
            this.MouseUp += BoundingPanelContainer_MouseUp;



            this.ClientSizeChanged += (s, e) =>
            {
                if (idc != null)
                {
                    SetScaleFactor();
                    this.Invalidate();
                }
            };


        }

        public void SetImageDataClass(ImageDataClass idc)
        {
            this.idc = idc;
            this.CurrentImage = Bitmap.FromFile(idc.ImageFileName);
            (this.Parent as DevExpress.XtraEditors.SplitGroupPanel).VerticalScroll.Value = 0;
            (this.Parent as DevExpress.XtraEditors.SplitGroupPanel).HorizontalScroll.Value = 0;
            this.Location = new Point(0, 0);
            this.Size = new Size((int)idc.ImageWidth, (int)idc.ImageHeight);

            SetScaleFactor();
            idc.ObjectBox = idc.ObjectBox.OrderBy(o => o.width * idc.ImageWidth * o.height * idc.ImageHeight).ToList();
            this.Controls.Clear();
            foreach (var box in idc.ObjectBox)
            {
                var bp = AddBoundingPanel(box, this.ImageScalefactor);
                bp.OperatingMode = _operatingmode;
                bp.OnDataChanging += (s, e) => { OnDataChanging?.Invoke(this, idc); };
                bp.OnDataChanged += (s, e) => { OnDataChanged?.Invoke(this, idc); };
                bp.OnSelectObject+=(s,e)=> OnSelectObject?.Invoke(this, e);
            }

            this.Invalidate();
        }

        private void SetScaleFactor()
        {
            if (CurrentImage != null)
            {
                float imageWidth = CurrentImage.Width;
                float imageHeight = CurrentImage.Height;

                float controlWidth = Width;
                float controlHeight = Height;

                // 根据控件大小和原图大小计算缩放因子
                float widthScaleFactor = controlWidth / imageWidth;
                float heightScaleFactor = controlHeight / imageHeight;

                // 取较小的缩放因子，保持纵横比
                ImageScalefactor = Math.Min(widthScaleFactor, heightScaleFactor);
                this.Controls.Cast<BoundingPanel>().ToList().ForEach(o =>
                {
                    o.ScaleFactor = ImageScalefactor;
                    o.SetBounds((int)(o.ScaleFactor * o.box.x * o.idc.ImageWidth), (int)(o.ScaleFactor * o.box.y * o.idc.ImageHeight), (int)(o.ScaleFactor * o.box.width * o.idc.ImageWidth), (int)(o.ScaleFactor * o.box.height * o.idc.ImageHeight));
                    //Debug.WriteLine($"ImageScalefactor:{ImageScalefactor}  Bounds:{o.Bounds}");
                });

            }

        }


        private void BoundingPanelContainer_MouseUp(object sender, MouseEventArgs e)
        {
            Debug.WriteLine($"BoundingPanelContainer_MouseUp:{OperatingMode.ToString()}");
            if (idc != null && MouseDownFlag == true && OperatingMode == OperatingModes.AddBounding)
            {
                if (tempBoundingPanel.Bounds.Width > 0.01 && tempBoundingPanel.Bounds.Height > 0.01)
                    tempBoundingPanel.box.CalBox_xywh(new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), tempBoundingPanel.Bounds, this.ImageScalefactor);
                else
                    idc.ObjectBox.Remove(idc.ObjectBox.Last());


                OnDataChanged(this, idc);
                MouseDownFlag = false;
            }
        }

        private void BoundingPanelContainer_MouseMove(object sender, MouseEventArgs e)
        {
            //Debug.WriteLine($"BoundingPanelContainer_MouseMove:{idc}");
            if (tempBoundingPanel != null && OperatingMode == OperatingModes.AddBounding && MouseDownFlag == true)
            {

                MouseMoveLoc.X = e.X;
                MouseMoveLoc.Y = e.Y;
                (var x, var y, var w, var h) = CalBounds(MouseDownLoc, MouseMoveLoc);
                //tempBoundingPanel.SetBounds((int)(ImageScalefactor * x), (int)(ImageScalefactor * y), (int)(ImageScalefactor * w), (int)(ImageScalefactor * h));
                tempBoundingPanel.SetBounds((int)(x), (int)(y), (int)(w), (int)(h));
                tempBoundingPanel.box.CalBox_xywh(new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), tempBoundingPanel.Bounds, this.ImageScalefactor);

                OnDataChanging(this, idc);
                //Debug.WriteLine($"{x},{y}-{w},{h}");
            }
        }


        private void BoundingPanelContainer_MouseDown(object sender, MouseEventArgs e)
        {
            Debug.WriteLine($"BoundingPanelContainer_MouseDown:{OperatingMode.ToString()}");
            var pb = sender as PictureBox;
            if (idc != null && MouseDownFlag == false && OperatingMode == OperatingModes.AddBounding)
            {
                var LabelId = this.LabelId;
                var LableName = LabelName;
                MouseDownFlag = true;

                MouseDownLoc.X = e.X;
                MouseDownLoc.Y = e.Y;
                var box = new BoxClass(LabelId, LableName, new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), new System.Drawing.Rectangle());

                tempBoundingPanel = AddBoundingPanel(box, this.ImageScalefactor);
                //tempBoundingPanel.SetBounds((int)(e.X), (int)(e.Y), 0, 0);
                tempBoundingPanel.box.CalBox_xywh(new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), tempBoundingPanel.Bounds, this.ImageScalefactor);

                idc.ObjectBox.Add(tempBoundingPanel.box);
                OnDataChanging(this, idc);
            }

        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            //将CurrentImage绘制在本窗体
            if (CurrentImage != null)
            {
                var targetWidth = ImageScalefactor * CurrentImage.Width;
                var targetHeight = ImageScalefactor * CurrentImage.Height;
                var targetX = (CurrentImage.Width - targetWidth) / 2;
                var targetY = (CurrentImage.Height - targetHeight) / 2;
                var targetRect = new RectangleF(targetX, targetY, targetWidth, targetHeight);
                pe.Graphics.Clear(Color.DarkGray);
                pe.Graphics.SetClip(pe.ClipRectangle); // 设置绘制区域为无效区域
                pe.Graphics.DrawImage(CurrentImage, 0, 0, targetWidth, targetHeight);
                //this.Controls.Cast<BoundingPanel>().ToList().ForEach(o => { o.Invalidate(); });
            }

        }

        private (int x, int y, int w, int h) CalBounds(System.Drawing.Point xy1, System.Drawing.Point xy2)
        {
            int x = Math.Min(xy1.X, xy2.X);
            int y = Math.Min(xy1.Y, xy2.Y);
            int w = Math.Abs(xy2.X - xy1.X);
            int h = Math.Abs(xy2.Y - xy1.Y);
            return (x, y, w, h);
        }


        private BoundingPanel AddBoundingPanel(BoxClass box, double ScaleFactor)
        {

            BoundingPanel t = new BoundingPanel();
            t.ScaleFactor = ScaleFactor;
            t.SetBox(idc, box);
            t.OnDataChanging += (s, e) => { OnDataChanging?.Invoke(this, idc); };
            t.OnDataChanged += (s, e) => { OnDataChanged?.Invoke(this, idc); };
            t.OnSelectObject += (s, e) => OnSelectObject?.Invoke(this, e);
            t.DeleteClick += new EventHandler((s, e) =>
            {
                this.Controls.Remove(t);
                idc.ObjectBox.Remove(box);
                idc.Saved = false;
                OnDataChanged(this, idc);

            });

            this.Controls.Add(t);
            return t;
        }
    }
}

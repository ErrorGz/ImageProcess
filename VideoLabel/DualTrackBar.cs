using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar;


namespace VideoLabel
{
    public partial class DualTrackBar : Control
    {

        private int _minimum = 0;
        private int _maximum = 100;
        private int _value1 = 0;
        private int _value2 = 100;
        private int _thumbSize = 16;
        private int _tickFrequency = 10;

        private bool _dragging1 = false;
        private bool _dragging2 = false;
        private Point _dragStart1;
        private Point _dragStart2;
        private int LastTick = Environment.TickCount;


        public event EventHandler OnValue1Changed;
        public event EventHandler OnValue2Changed;
        public DualTrackBar()
        {

            SetStyle(ControlStyles.AllPaintingInWmPaint |
             ControlStyles.OptimizedDoubleBuffer |
             ControlStyles.ResizeRedraw |
             ControlStyles.UserPaint, true);
            // SetStyle(ControlStyles.AllPaintingInWmPaint |
            //ControlStyles.OptimizedDoubleBuffer |
            //ControlStyles.ResizeRedraw, true);
            // DoubleBuffered = true;

        }

        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum != value)
                {
                    _minimum = value;
                    Invalidate();
                }
            }
        }

        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum != value)
                {
                    _maximum = value;
                    Invalidate();
                }
            }
        }

        public int Value1
        {
            get { return _value1; }
            set
            {
                if (_value1 != value)
                {
                    _value1 = value;
                    Invalidate();
                }
            }
        }

        public int Value2
        {
            get { return _value2; }
            set
            {
                if (_value2 != value)
                {
                    _value2 = value;
                    Invalidate();
                }
            }
        }

        public int ThumbSize
        {
            get { return _thumbSize; }
            set
            {
                if (_thumbSize != value)
                {
                    _thumbSize = value;
                    Invalidate();
                }
            }
        }

        public int TickFrequency
        {
            get { return _tickFrequency; }
            set
            {
                if (_tickFrequency != value)
                {
                    _tickFrequency = value;
                    Invalidate();
                }
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (_minimum == 0 && _maximum == 0)
                return;
            int thumbHalf = _thumbSize / 2;
            int trackWidth = Width - _thumbSize;
            int trackHeight = Height / 3;
            int trackTop = (Height - trackHeight) / 2;

            // Draw tick marks
            using (Pen pen = new Pen(Color.Gray))
            {
                for (int i = _minimum; i <= _maximum; i += _tickFrequency)
                {
                    int tickX = (int)((float)(i - _minimum) / (_maximum - _minimum) * trackWidth) + thumbHalf;
                    int tickY = trackTop + trackHeight + thumbHalf;
                    e.Graphics.DrawLine(pen, tickX, trackTop, tickX, tickY);
                }
            }

            // Draw track
            using (SolidBrush brush = new SolidBrush(Color.LightGray))
            {
                e.Graphics.FillRectangle(brush, thumbHalf, trackTop, trackWidth, trackHeight);
            }

            // Draw thumbs
            DrawThumb(e.Graphics, _value1, thumbHalf, trackTop, trackWidth, trackHeight, _dragging1, 1);
            DrawThumb(e.Graphics, _value2, thumbHalf, trackTop, trackWidth, trackHeight, _dragging2, 2);

            // Draw values
            using (Font font = new Font(FontFamily.GenericSansSerif, 8))
            {
                string value1Text = _value1.ToString();
                string value2Text = _value2.ToString();
                SizeF value1Size = e.Graphics.MeasureString(value1Text, font);
                SizeF value2Size = e.Graphics.MeasureString(value2Text, font);
                float value1X = (float)(_value1 - _minimum) / (_maximum - _minimum) * trackWidth + thumbHalf - value1Size.Width / 2;
                float value2X = (float)(_value2 - _minimum) / (_maximum - _minimum) * trackWidth + thumbHalf - value2Size.Width / 2;
                float valueY = trackTop + trackHeight + thumbHalf + 5;
                e.Graphics.DrawString(value1Text, font, Brushes.Black, value1X, valueY);
                e.Graphics.DrawString(value2Text, font, Brushes.Black, value2X, valueY);
            }
            GC.Collect();
        }

        private void DrawThumb(Graphics g, int value, int thumbHalf, int trackTop, int trackWidth, int trackHeight, bool dragging, int index)
        {
            float thumbX = (float)(value - _minimum) / (_maximum - _minimum) * trackWidth + thumbHalf;
            float thumbY = trackTop + trackHeight / 2;
            int triangleHeight = thumbHalf;

            Image img = null;
            if (index == 1)
            {
                if (dragging)
                    img = Resource.Start_p;
                else
                    img = Resource.Start;
            }
            else if (index == 2)
            {
                if (dragging)
                    img = Resource.End_p;
                else
                    img = Resource.End;
            }
            if (img != null)
            {
                g.DrawImage(img, thumbX - thumbHalf, thumbY - thumbHalf, thumbHalf * 2, thumbHalf * 2);
            }


        }
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                int thumbHalf = _thumbSize / 2;
                int trackWidth = Width - _thumbSize;
                int trackHeight = Height / 3;
                int trackTop = (Height - trackHeight) / 2;

                // Check if first thumb is being dragged
                float thumb1X = (float)(_value1 - _minimum) / (_maximum - _minimum) * trackWidth + thumbHalf;
                float thumb1Y = trackTop + trackHeight / 2;
                RectangleF thumb1Rect = new RectangleF(thumb1X - thumbHalf, thumb1Y - thumbHalf, _thumbSize, _thumbSize);
                if (thumb1Rect.Contains(e.Location))
                {
                    _dragging1 = true;
                    _dragStart1 = e.Location;
                    return;
                }

                // Check if second thumb is being dragged
                float thumb2X = (float)(_value2 - _minimum) / (_maximum - _minimum) * trackWidth + thumbHalf;
                float thumb2Y = trackTop + trackHeight / 2;
                RectangleF thumb2Rect = new RectangleF(thumb2X - thumbHalf, thumb2Y - thumbHalf, _thumbSize, _thumbSize);
                if (thumb2Rect.Contains(e.Location))
                {
                    _dragging2 = true;
                    _dragStart2 = e.Location;
                    return;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_dragging1)
            {
                int thumbHalf = _thumbSize / 2;
                int trackWidth = Width - _thumbSize;
                int newValue = (int)((float)(e.X - thumbHalf) / trackWidth * (_maximum - _minimum) + _minimum);
                if (newValue < _minimum)
                {
                    newValue = _minimum;
                }
                if (newValue > _value2)
                {
                    newValue = _value2;
                }
                if (_value1 != newValue)
                {
                    _value1 = newValue;
                    Invalidate();
                    if (Environment.TickCount - LastTick > 100)
                    {
                        LastTick = Environment.TickCount;
                        OnValue1Changed?.Invoke(this, EventArgs.Empty);
                    }

                }
            }

            if (_dragging2)
            {
                int thumbHalf = _thumbSize / 2;
                int trackWidth = Width - _thumbSize;
                int newValue = (int)((float)(e.X - thumbHalf) / trackWidth * (_maximum - _minimum) + _minimum);
                if (newValue > _maximum)
                {
                    newValue = _maximum;
                }
                if (newValue < _value1)
                {
                    newValue = _value1;
                }
                if (_value2 != newValue)
                {
                    _value2 = newValue;
                    Invalidate();
                    if (Environment.TickCount - LastTick > 100)
                    {
                        LastTick = Environment.TickCount;
                        OnValue2Changed?.Invoke(this, EventArgs.Empty);
                    }

                }
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            //if (_dragging1 || _dragging2)
            //{
            //    _dragging1 = false;
            //    _dragging2 = false;
            //}
            if (_dragging1)
            {
                _dragging1 = false;
                OnValue1Changed?.Invoke(this, EventArgs.Empty);
            }
            if (_dragging2)
            {
                _dragging2 = false;
                OnValue2Changed?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}

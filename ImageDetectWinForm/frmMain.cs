using Compunet.YoloV8;
using System.Security.Policy;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using RectangleF = SixLabors.ImageSharp.RectangleF;

namespace ImageDetectWinForm
{
    public partial class frmMain : Form
    {
        CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        string url = @"P:\BaiduSyncdisk\PY\DataSet\��Ƶ\9��14��.mp4";
        //var url = @"rtsp://guest:123456@10.103.8.236:554/avstream/channel=1/stream=1.sdp";
        string model = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.Environment.ProcessPath), @".\onnx\yolov8n-pose.onnx");
        //YoloV8 yolov8_model;
        //int fps = 30;
        ImageDetect id = null;


        RectangleF people_rect_float = new RectangleF() { X = 230f / 962, Y = 95f / 752, Width = 300f / 962, Height = 600f / 752 };
        RectangleF train_rect_float = new RectangleF() { X = 190f / 962, Y = 0f, Width = 470f / 962, Height = 630f / 752 };
        public frmMain()
        {
            InitializeComponent();
            id = new ImageDetect(cancellationTokenSource.Token);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            id.Start(url, model, people_rect_float, train_rect_float, ActionShowBitmap);

        }

        public void ActionShowBitmap(Bitmap bitmap)
        {
            //pictureBox1.Image = bitmap;
            // ���� PictureBox �Ŀ�������Ŀ�Ⱥ͸߶�
            int pictureBoxWidth = pictureBox1.ClientSize.Width;
            int pictureBoxHeight = pictureBox1.ClientSize.Height;

            // ��ȡͼƬ�Ŀ�Ⱥ͸߶�
            int imageWidth = bitmap.Width;
            int imageHeight = bitmap.Height;

            // ����ͼƬ�Ŀ�߱���
            float widthRatio = (float)pictureBoxWidth / imageWidth;
            float heightRatio = (float)pictureBoxHeight / imageHeight;

            // ѡ���С�ı�����Ϊ���ű�������ȷ��ͼƬ��ȫ��ʾ�� PictureBox ��
            float ratio = Math.Min(widthRatio, heightRatio);

            if (ratio > 0)
            {
                // �������ź��ͼƬ��Ⱥ͸߶�
                int scaledWidth = (int)(imageWidth * ratio);
                int scaledHeight = (int)(imageHeight * ratio);

                // �������ź��ͼƬ
                Bitmap scaledBitmap = new Bitmap(bitmap, scaledWidth, scaledHeight);

                // �� PictureBox ����ʾ���ź��ͼƬ
                try
                {
                    if (cancellationTokenSource.IsCancellationRequested == false) pictureBox1.Invoke((MethodInvoker)(() => { pictureBox1.Image = scaledBitmap; }));
                }
                catch
                {

                }

            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            cancellationTokenSource.Cancel();
        }

        private void toolStripButtonReverse_Click(object sender, EventArgs e)
        {
            id.SetReverse();
        }

        private void toolStripButtonAdvance_Click(object sender, EventArgs e)
        {
            id.SetAdvance();
        }

        private void toolStripButtonPlayPause_Click(object sender, EventArgs e)
        {
            var ctrl = sender as ToolStripButton;
            var r = id.SetPause();
            ctrl.Image = r ? Properties.Resources.icons8_play_64 : Properties.Resources.icons8_pause_64;
        }

        private void toolStripButtonRecord_Click(object sender, EventArgs e)
        {
            var ctrl = sender as ToolStripButton;
            id.SetRecord();
            new Task(() =>
            {
                Thread.Sleep(200);
                var r = id.GetRecording();
                this.Invoke(() => { ctrl.Image = r ? Properties.Resources.icons8_pause_button_64 : Properties.Resources.icons8_record_64; });

            }).Start();

        }

        private void toolStripButtonLiveDetect_Click(object sender, EventArgs e)
        {
            var ctrl = sender as ToolStripButton;
            var r = id.SetLiveDetect();
            ctrl.Image = r ? Properties.Resources.icons8_rfid_signal_64 : Properties.Resources.icons8_offline_64;
        }
    }
}
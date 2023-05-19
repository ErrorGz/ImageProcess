using DevExpress.Utils.DirectXPaint;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace ImageLabel
{
    public partial class frm设置运动阈值 : Form
    {
        public Threshold threshold { get; set; }
        public frm设置运动阈值(Threshold t)
        {
            InitializeComponent();
            threshold = t;
            spinEdit移动阈值宽度.Value = t.移动阈值宽度;
            spinEdit移动阈值高度.Value = t.移动阈值高度;
            spinEdit放大阈值宽度.Value = t.放大阈值宽度;
            spinEdit放大阈值高度.Value = t.放大阈值高度;
            spinEdit缩小阈值宽度.Value = t.缩小阈值宽度;
            spinEdit缩小阈值高度.Value = t.缩小阈值高度;
        }

        private void button确定_Click(object sender, EventArgs e)
        {
            threshold.移动阈值宽度 = (int)spinEdit移动阈值宽度.Value;
            threshold.移动阈值高度 = (int)spinEdit移动阈值高度.Value;
            threshold.放大阈值宽度 = (int)spinEdit放大阈值宽度.Value;
            threshold.放大阈值高度 = (int)spinEdit放大阈值高度.Value;
            threshold.缩小阈值宽度 = (int)spinEdit缩小阈值宽度.Value;
            threshold.缩小阈值高度 = (int)spinEdit缩小阈值高度.Value;
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button取消_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}

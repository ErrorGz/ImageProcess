using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VideoLabel
{
    public partial class frmSelect : Form
    {
        public string InputString { get; set; }
        public frmSelect(string title, string label, List<string> msg)
        {
            InitializeComponent();
            this.Text = title;
            label1.Text = label;
            foreach (var item in msg)
            {
                comboBox1.Items.Add(item);
            }
            if (comboBox1.Items.Count > 0)
                comboBox1.SelectedIndex = 0;

        }

        private void button2_Click(object sender, EventArgs e)
        {
            InputString = comboBox1.SelectedItem as string;
        }
    }
}

using DevExpress.Mvvm.Native;
using DXApplication;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;


namespace ImageLabel
{
    public partial class frmLabel : Form
    {
        List<WorkSpaceClass> _listworkspace;
        Dictionary<int, string> _listlabels;
        public BindingList<DoubleInt> listLabelId { get; set; }
        public BindingList<DoubleString> listLabelName { get; set; }


        public frmLabel(List<WorkSpaceClass> listworkspace, Dictionary<int, string> listlabels)
        {
            InitializeComponent();
            _listworkspace = listworkspace;
            _listlabels = listlabels;

        }

        private void frmLabelIds_Load(object sender, EventArgs e)
        {
            listLabelName = new BindingList<DoubleString>();
            listLabelId = new BindingList<DoubleInt>();

            var listname = _listlabels.Select(o => new DoubleString { Name = o.Value, NewName = o.Value });
            listname.ForEach(o =>
            {
                listLabelName.Add(o);

            });
            gridControl1.DataSource = listLabelName;



            var PathItems = _listworkspace.OrderBy(o => o.WorkSpacePath).Select(o => o.WorkSpacePath).ToList();
            comboBoxEdit2.Properties.Items.AddRange(PathItems);
            if (PathItems.Count > 0)
                comboBoxEdit2.SelectedIndex = 0;

        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {


        }

        private void comboBoxEdit2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEdit2.SelectedIndex != -1)
            {
                var item = comboBoxEdit2.SelectedItem as string;
                bindGridControl2Datasource(item);
            }
        }

        private void bindGridControl2Datasource(string item)
        {
            var workspace = _listworkspace.Where(o => o.WorkSpacePath == item).FirstOrDefault();
            if (workspace != null)
            {
                listLabelId.Clear();
                workspace.ImageData.ForEach(imagedata =>
                {
                    var listobjectbox = imagedata.ObjectBox;
                    listobjectbox.ForEach(objectbox =>
                    {
                        var any = listLabelId.Where(o => o.Id == objectbox.Id).Any();
                        if (any == false)
                            listLabelId.Add(new DoubleInt { Id = objectbox.Id, NewId = objectbox.Id });
                    });
                });
                gridControl2.DataSource = listLabelId;
            }
        }

        private void buttonApply_Click(object sender, EventArgs e)
        {
            var listLabelNameChange = listLabelName.Where(o => o.Name != o.NewName && string.IsNullOrEmpty(o.NewName) == false);
            listLabelNameChange.ForEach(o =>
            {
                var label = _listlabels.FirstOrDefault(p => p.Value == o.Name);
                if (!label.Equals(default(KeyValuePair<int, string>)))
                {
                    _listlabels[label.Key] = o.NewName;
                }

            });


            var listLabelIdChanged = listLabelId.Where(o => o.Id != o.NewId);

            if (comboBoxEdit2.SelectedIndex != -1)
            {
                List<DoubleInt> listid = new List<DoubleInt>();
                var item = comboBoxEdit2.SelectedItem as string;
                _listworkspace.Where(o => o.WorkSpacePath == item).FirstOrDefault().ImageData.ForEach(imagedata =>
                {
                    bool flagchanged = false;
                    var listObjectBox = imagedata.ObjectBox;
                    listLabelIdChanged.ForEach(p =>
                    {
                        var ids = listObjectBox.Where(q => q.Id == p.Id).FirstOrDefault();
                        if (ids != null)
                        {
                            ids.Id = p.NewId;
                            flagchanged = true;
                        }
                    });
                    if (flagchanged)
                        imagedata.Saved = false;

                });
                bindGridControl2Datasource(item);

            }
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            this.Close();


        }
        private void 添加ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void 删除ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
        private void 添加ToolStripMenuItem1_Click(object sender, EventArgs e)
        {

        }
        private void 删除ToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var doubleint = gridView2.GetFocusedRow() as DoubleInt;

            var item = comboBoxEdit2.SelectedItem as string;
            _listworkspace.Where(o => o.WorkSpacePath == item).FirstOrDefault().ImageData.ForEach(imagedata =>
            {
                var listObjectBox = imagedata.ObjectBox;
                var count = listObjectBox.RemoveAll(p => p.Id == doubleint.Id);

                if (count > 0)
                    imagedata.Saved = false;

            });
            bindGridControl2Datasource(item);


        }




    }


}

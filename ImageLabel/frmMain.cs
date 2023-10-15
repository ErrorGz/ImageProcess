using Compunet.YoloV8;
using DevExpress.Utils;
using DevExpress.XtraBars.Ribbon.ViewInfo;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraSpreadsheet.DocumentFormats.Xlsb;
using Microsoft.ML.OnnxRuntime;
using Microsoft.Win32;
using MoreLinq;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using static ImageLabel.BoundingPanel;


namespace ImageLabel
{
    public partial class frmMain : Form
    {
        List<Classifier> CurrentClassifierList = new List<Classifier>();
        List<WorkSpaceClass> CurrentWorkSpace = new List<WorkSpaceClass>();
        Dictionary<int, string> CurrentLabels = new Dictionary<int, string>();
        List<string> CurrentYoloModelFiles = new List<string> { };


        YoloV8 yolo;


        ImageDataClass CurrentImageData;
        //BoxClass CurrentBox;
        //PointClass CurrentPoint;
        object CurrentObject = null;
        BoundingPanel CurrentMaskPanel = null;

        bool MouseDownFlag = false;
        System.Drawing.Point MouseDownLoc = new System.Drawing.Point();
        System.Drawing.Point MouseMoveLoc = new System.Drawing.Point();



        public frmMain()
        {
            InitializeComponent();
            boundingPanelContainer1.OnDataChanging += (s, e) =>
            {
                e.Saved = false;
                ShowImageDataInTextBox(textBoxImageData, e);
            };
            boundingPanelContainer1.OnDataChanged += (s, e) =>
            {
                e.Saved = false;
                ShowImageDataInTextBox(textBoxImageData, e);
            };
            boundingPanelContainer1.OnSelectObject += (s, e) =>
            {
                ShowObjectDataInTextBox(maskedTextBox, e);
            };
            splitContainerControl2.Panel1.SizeChanged += (s, e) => { boundingPanelContainer1.Invalidate(); };
            SettoolStripButtonIcon();

        }

        private void frmWinMain_Load(object sender, EventArgs e)
        {
            CurrentLabels = StaticLib.YoloLabels;
            var labels = CurrentLabels.Select(o => new RadioGroupItem { Value = o.Key, Description = $"{o.Key}:{o.Value}", Tag = o.Value });
            radioGroupLabel.Properties.Items.AddRange(labels.ToArray());

            string[] defaultWeightFile = { "./onnx/yolov8n-pose.onnx", "./onnx/yolov8n.onnx", "./onnx/yolov8n-seg.onnx" };
            CurrentYoloModelFiles.AddRange(defaultWeightFile);
            var weights = defaultWeightFile.Select(o => new RadioGroupItem { Value = o, Description = o, Tag = o }).ToArray();

            radioGroupWeight.Properties.Items.AddRange(weights);
            radioGroupWeight.SelectedIndex = 0;

            var t = Task.Run(() =>
            {
                FileInfo onnxfile = new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, defaultWeightFile[0]));
                yolo = new YoloV8(new ModelSelector(onnxfile.FullName));

            });
            t.ContinueWith((o) =>
            {
                ShowMessageDelayClose("模型加载完毕");
            });

        }
        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            Task.Run(() => { if (yolo != null) yolo.Dispose(); });
        }
        async private Task<WorkSpaceClass> GetWorkSpace(string CurrentDirectory)
        {
            WorkSpaceClass myws = null;


            Stopwatch sw = new Stopwatch();
            await Task.Run(() =>
            {
                sw.Start();
                myws = new WorkSpaceClass(CurrentDirectory, CurrentLabels, (m) => { this.Invoke(() => { ShowMessage(m); }); });
                myws.WorkSpacePath = CurrentDirectory;
                sw.Stop();
            });

            Debug.WriteLine($"读取文件夹完成，用时{sw.ElapsedMilliseconds / 1000f}秒");


            return myws;
        }



        private void galleryControl1_Gallery_CustomDrawItemImage(object sender, DevExpress.XtraBars.Ribbon.GalleryItemCustomDrawEventArgs e)
        {
            GalleryItemViewInfo vInfo = e.ItemInfo as GalleryItemViewInfo;

            var item = e.Item.Value as ImageDataClass;
            if (!vInfo.ImageInfo.IsLoaded)
            {
                Debug.WriteLine($"No Cache.");
                using (var image = System.Drawing.Image.FromFile(item.ImageFileName))
                {
                    if (image != null)
                    {
                        var cloneimage = (System.Drawing.Image)ImageSharpExtensions.FixedSize(image, 416, 416);
                        var filename = Path.GetFileName(item.ImageFileName);
                        e.Cache.DrawString(filename, System.Drawing.SystemFonts.SmallCaptionFont, Brushes.LightPink, e.Bounds);
                        vInfo.ImageInfo.Image = cloneimage;
                        vInfo.ImageInfo.ThumbImage = cloneimage;
                        vInfo.ImageInfo.IsLoaded = true;
                    }
                    e.Handled = true;
                }
            }
            else
            {
                Debug.WriteLine($"Cache.");
                ImageCollection.DrawImageListImage(e.Cache, vInfo.ImageInfo.Image, vInfo.ImageInfo.ThumbImage, vInfo.Item.ImageIndex, vInfo.ImageContentBounds, vInfo.IsEnabled);
                var filename = Path.GetFileName(item.ImageFileName);
                e.Cache.DrawString(filename, SystemFonts.SmallCaptionFont, Brushes.LightPink, e.Bounds);
                e.Handled = true;
            }

        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Link;
            else
                e.Effect = DragDropEffects.None;
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            var s = ((string[])e.Data.GetData(DataFormats.FileDrop, false)).ToList();
            Task.Run(() =>
            {

                s.ForEach(async o =>
                {
                    if (Directory.Exists(o))
                    {
                        var nws = await GetWorkSpace(o);
                        CurrentWorkSpace.Add(nws);


                    }
                    if (File.Exists(o))
                    {
                        var ext = Path.GetExtension(o);
                        if (StaticLib.VideoFormat.Contains(ext))
                        {
                            FileInfo file = new FileInfo(o);
                            var VideoPath = file.DirectoryName;
                            frmInputVideo frm = new frmInputVideo(o);
                            if (frm.ShowDialog() == DialogResult.OK)
                            {
                                var capture = VideoCapture.FromFile(o);
                                var fc = capture.Get(VideoCaptureProperties.FrameCount);
                                var fps = capture.Get(VideoCaptureProperties.Fps);
                                var total = fc / fps;
                                await Task.Run(() =>
                                {
                                    var path = StaticLib.GetNewDirName(VideoPath, DateTime.Now.ToString("yyyyMMdd"));
                                    System.IO.Directory.CreateDirectory(path);
                                    for (int i = frm.StartPos; i < frm.EndPos; i += frm.FPS)
                                    {
                                        capture.Set(VideoCaptureProperties.PosFrames, i);
                                        var mat = capture.RetrieveMat();
                                        if (frm.XFlip)
                                            mat = mat.Flip(OpenCvSharp.FlipMode.X);
                                        if (frm.YFlip)
                                            mat = mat.Flip(OpenCvSharp.FlipMode.Y);

                                        var filename = StaticLib.GetNewFileName(path, DateTime.Now.ToString("yyyyMMdd"), ".jpg");
                                        mat.SaveImage(filename);

                                    }
                                    var myws = new WorkSpaceClass(path, CurrentLabels, (m) => this.Invoke(() => { ShowMessage(m); }));
                                    CurrentWorkSpace.Add(myws);

                                });

                            }

                        }
                        else if (StaticLib.ImageFormat.Contains(ext))
                        {

                        }
                        else if (StaticLib.ONNXFormat.Contains(ext))
                        {

                            var c = radioGroupWeight.Properties.Items.Where(p => p.Value.ToString() == o).Any();
                            if (c == false)
                            {
                                CurrentYoloModelFiles.Add(o);
                                this.Invoke(() => { radioGroupWeight.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(o, o, true, o)); });

                            }
                        }

                    }
                    this.Invoke(() =>
                    {
                        bind(CurrentWorkSpace, CurrentLabels, CurrentYoloModelFiles);

                    });
                });
            });


        }
        public void SelectLastItem(int? selectindex = null)
        {
            if (selectindex != null)
            {
                comboBox1.SelectedIndex = selectindex.Value;
            }
            else
            {
                var lastitem = CurrentClassifierList.Where(o => o.Parameter.type == ShowParameterType.WorkSpace && o.Parameter.tag == ShowParameterTag.显示全部).Select(o => o.Parameter.Text).LastOrDefault();
                if (lastitem != null)
                {
                    var index = comboBox1.FindStringExact(lastitem);
                    if (index != -1)
                        comboBox1.SelectedIndex = index;
                    else
                        comboBox1.SelectedIndex = -1;
                }
                else
                    comboBox1.SelectedIndex = -1;
            }
            GalleryRefresh();
        }
        public void bind(List<WorkSpaceClass> listworkspace, Dictionary<int, string> listlabels, List<string> listModelsFile, int? selectindex = null)
        {
            CurrentClassifierList.Clear();
            Dictionary<int, string> listAvailableLabels = new();
            listworkspace.ForEach(ws =>
            {
                foreach (ShowParameterTag tag in (ShowParameterTag[])Enum.GetValues(typeof(ShowParameterTag)))
                {
                    ItemParameter p = new ItemParameter
                    {
                        type = ShowParameterType.WorkSpace,
                        tag = tag,
                        Name = ws.WorkSpacePath,
                        Text = $"WorkSpace:{ws.WorkSpacePath},{StaticLib.GetShowParameterTagName(tag)}"
                    };
                    CurrentClassifierList.Add(new Classifier { Text = p.Text, Parameter = p });
                }
                ws.ImageData.ForEach(imagedata =>
                {
                    imagedata.ObjectBox.ForEach(objectbox =>
                    {
                        if (listAvailableLabels.ContainsKey(objectbox.Id) == false)
                            listAvailableLabels.Add(objectbox.Id, objectbox.Name);

                    });

                });
            });

            listAvailableLabels = listAvailableLabels.Join(listlabels, x => x.Key, y => y.Key, (x, y) => new KeyValuePair<int, string>(x.Key, y.Value)).ToDictionary<int, string>();
            listAvailableLabels.ForEach(label =>
            {
                var tag = ShowParameterTag.显示已标签;
                ItemParameter p = new ItemParameter
                {
                    type = ShowParameterType.Label,
                    tag = tag,
                    Id = label.Key,
                    Name = label.Value,
                    Text = $"Label:{label.Key}-{label.Value},{StaticLib.GetShowParameterTagName(tag)}"
                };
                CurrentClassifierList.Add(new Classifier { Text = p.Text, Parameter = p });

            });
            comboBox1.Items.Clear();
            CurrentClassifierList.ForEach(o =>
            {
                var index = comboBox1.Items.Add(o.Text);

            });
            SelectLastItem(selectindex);

            var labels = listlabels.Select(o => new RadioGroupItem { Value = o.Key, Description = $"{o.Key}:{o.Value}", Tag = o.Value }).ToArray();
            radioGroupLabel.Properties.Items.Clear();
            radioGroupLabel.Properties.Items.AddRange(labels);

            var weights = listModelsFile.Select(o => new RadioGroupItem { Value = o, Description = o, Tag = o }).ToArray();
            radioGroupWeight.Properties.Items.Clear();
            radioGroupWeight.Properties.Items.AddRange(weights);
        }

        private void GalleryRefresh()
        {
            var ctrl = comboBox1;
            var allitem = CurrentClassifierList.Where(o => o.Text == (string)ctrl.SelectedItem).ToList();

            //var allitem = ctrl.Properties.Items.Where(o => o.CheckState == CheckState.Checked).Select(o => o.Value).Cast<ItemParameter>().ToList();
            galleryControl1.Gallery.Groups.Clear();

            galleryControl1.Gallery.BeginUpdate();
            allitem.ForEach(o =>
            {
                if (o.Parameter.type == ShowParameterType.WorkSpace)
                {
                    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = o.Text });


                    CurrentWorkSpace.ForEach(ws =>
                    {
                        if (o.Parameter.tag == ShowParameterTag.显示全部)
                        {
                            ws.ImageData.ForEach(imagedata =>
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = Path.GetFileName(imagedata.ImageFileName)
                                });
                            });
                        }
                        if (o.Parameter.tag == ShowParameterTag.显示未标签)
                        {
                            ws.ImageData.Where(imagedata => string.IsNullOrEmpty(imagedata.TextFileName) == true).ForEach(imagedata =>
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = Path.GetFileName(imagedata.ImageFileName)
                                });
                            });
                        }
                        if (o.Parameter.tag == ShowParameterTag.显示已标签)
                        {
                            ws.ImageData.Where(imagedata => string.IsNullOrEmpty(imagedata.TextFileName) == false).ForEach(imagedata =>
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = Path.GetFileName(imagedata.ImageFileName)
                                });
                            });
                        }
                    });




                }
                if (o.Parameter.type == ShowParameterType.Label)
                {
                    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = o.Text });
                    CurrentWorkSpace.ForEach(ws =>
                    {
                        ws.ImageData.ForEach(imagedata =>
                        {
                            var flag = imagedata.ObjectBox.Where(objectbox => objectbox.Id == o.Parameter.Id).Any();
                            if (flag)
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = Path.GetFileName(imagedata.ImageFileName)
                                });
                            }
                        });
                    });

                }
            });
            galleryControl1.Gallery.EndUpdate();
        }

        private void galleryControl1_Gallery_ItemClick(object sender, DevExpress.XtraBars.Ribbon.GalleryItemClickEventArgs e)
        {
            CurrentImageData = e.Item.Value as ImageDataClass;
            boundingPanelContainer1.SetImageDataClass(CurrentImageData);
            boundingPanelContainer1.Invalidate();
            ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
            CurrentObject = null;
            ShowObjectDataInTextBox(maskedTextBox, CurrentObject);


        }
        private void toggleSwitch1_Toggled(object sender, EventArgs e)
        {
            SelectWeightsFiles();
        }
        private void radioGroupWeight_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectWeightsFiles();
        }

        private void SelectWeightsFiles()
        {
            if (radioGroupWeight.SelectedIndex == -1)
                return;
            var onnxfilepath = radioGroupWeight.Properties.Items[radioGroupWeight.SelectedIndex].Value as string;
            if (onnxfilepath != null)
                Task.Run(() =>
                {
                    try
                    {
                        this.Invoke(() => ShowMessageDelayClose($"准备加载模型：{onnxfilepath}"));
                        yolo = null;
                        FileInfo onnxfile = new FileInfo(onnxfilepath);
                        SessionOptions options = new SessionOptions();
                        if (toggleSwitch1.IsOn)
                        {
                            options.AppendExecutionProvider_CUDA();
                        }
                        yolo = new YoloV8(onnxfile.FullName, options);
                        this.Invoke(() => ShowMessageDelayClose($"加载完毕"));
                    }
                    catch (Exception ex)
                    {

                        this.Invoke(() => { MessageBox.Show(ex.Message); });
                    }
                });
        }



        private void ShowMessage(string msg)
        {
            toolStripStatusLabelForm.Text = msg;
        }

        private void ShowMessageDelayClose(string msg)
        {
            Task.Run(() =>
            {
                this.Invoke(() => toolStripStatusLabelForm.Text = msg);
                Thread.Sleep(3000);
                this.Invoke(() => toolStripStatusLabelForm.Text = "");
            });
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetScrollPos(IntPtr hWnd, int nBar);


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);


        private void ShowObjectDataInTextBox(System.Windows.Forms.MaskedTextBox tb, object data)
        {
            if (data is BoxClass)
            {
                var box = data as BoxClass;
                CurrentObject = box;
                tb.Text = $"{box.Id} {box.cx} {box.cy} {box.width} {box.height}";
            }

            else if (data is PointClass)
            {
                var point = data as PointClass;
                CurrentObject = point;
                tb.Text = $"{point.x} {point.y} {point.c}";
            }
            else
            {
                tb.Text = "";
            }
        }
        private void ShowImageDataInTextBox(System.Windows.Forms.TextBox tb, ImageDataClass idc)
        {
            if (tb != null)
            {
                StringBuilder sb = new StringBuilder();
                idc.ObjectBox.ForEach(o =>
                {
                    sb.Append($"{o.Id} {o.cx} {o.cy} {o.width} {o.height} ");
                    o.points.ForEach(p => { sb.Append($"{p.x} {p.y} {p.c} "); });
                    sb.Append("\r\n");

                });

                int horizontalScrollPos = GetScrollPos(tb.Handle, 0);
                int verticalScrollPos = GetScrollPos(tb.Handle, 1);
                tb.Text = sb.ToString();

                // 设置水平滚动条位置
                SetScrollPos(tb.Handle, 0, horizontalScrollPos, true);
                // 设置垂直滚动条位置
                SetScrollPos(tb.Handle, 1, verticalScrollPos, true);

            }

        }

        private void 新增标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInput input = new frmInput("添加标签", "请输入标签名：", "");
            if (input.ShowDialog() == DialogResult.OK && string.IsNullOrWhiteSpace(input.InputString) == false)
            {
                int id = 0;
                if (CurrentLabels.Count > 0)
                    id = CurrentLabels.Select(o => o.Key).Max() + 1;

                CurrentLabels.Add(id, input.InputString);
                radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem() { Value = id, Description = $"{id}:{input.InputString}", Tag = input.InputString });

            }
        }

        private void 删除标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("确定删除标签？", "警告", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Yes && radioGroupLabel.SelectedIndex != -1)
            {
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                CurrentLabels.Remove(selectid);
                radioGroupLabel.Properties.Items.RemoveAt(radioGroupLabel.SelectedIndex);
            }
        }




        async private void 归集移动ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                var path = StaticLib.GetNewDirName(folderBrowserDialog.SelectedPath, $"Collect{DateTime.Now.ToString("yyyyMMdd")}");
                try
                {

                    Directory.CreateDirectory(path);
                    foreach (var workspace in CurrentWorkSpace)
                    {
                        foreach (var imagedata in workspace.ImageData)
                        {

                            var shortimagefilename = Path.GetFileName(imagedata.ImageFileName);
                            var longimagefilename = Path.Combine(path, shortimagefilename);
                            File.Move(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = Path.GetFileName(imagedata.TextFileName);
                                var longtextfilename = Path.Combine(path, shorttextfilename);
                                File.Move(imagedata.TextFileName, longtextfilename, true);
                            }

                        }
                    }

                }


                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }


                var nws = await GetWorkSpace(path);


                galleryControl1.Gallery.Groups.Clear();
                var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = path });
                nws.ImageData.ForEach(o =>
                {
                    galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem() { Value = o, Hint = Path.GetFileName(o.ImageFileName) });
                });


                //radioGroupLabel.Properties.Items.Clear();
                //nws.Labels.ForEach(o =>
                //{
                //    radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(o.Key, o.Key.ToString() + ":" + o.Value, true, o.Value));

                //});
                CurrentWorkSpace.Clear();
                CurrentWorkSpace.Add(nws);

                ShowMessageDelayClose($"归集文件夹{path}完毕");
            }
        }




        private void SaveYoloDataYamlFile(string yamlfilename, string trainimagepath, string validimagepath)
        {
            YOLODataClass y = new YOLODataClass();

            y.names = new List<string>();
            y.names.AddRange(CurrentLabels.Select(o => o.Value).ToArray());
            //y.names.AddRange(radioGroupLabel.Properties.Items.Select(o => (string)o.Tag).ToArray());
            y.nc = y.names.Count;
            y.train = trainimagepath;
            y.val = validimagepath;

            var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
            var yaml = serializer.Serialize(y);

            System.IO.File.WriteAllText(yamlfilename, yaml);
        }

        private void 分集移动ToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        async private void toolStripButtonOpenFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            var r = browserDialog.ShowDialog();
            if (r == DialogResult.OK)
            {
                var CurrentDirectory = browserDialog.SelectedPath;

                var nws = await GetWorkSpace(CurrentDirectory);
                CurrentWorkSpace.Add(nws);
                bind(CurrentWorkSpace, CurrentLabels, CurrentYoloModelFiles);

                ShowMessageDelayClose($"打开文件夹{CurrentDirectory}完毕");
            }
        }

        private void toolStripButtonOpenFile_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            var count = 0;
            CurrentWorkSpace.ForEach(project =>
            {

                project.ImageData.Where(o => o.Saved == false).ForEach(idc =>
                {
                    count++;
                    if (idc.ObjectBox != null)
                    {


                        StringBuilder sb = new StringBuilder();
                        idc.ObjectBox.ForEach(box =>
                        {
                            sb.Append(box.Id);
                            sb.Append(" ");
                            sb.Append(box.cx);
                            sb.Append(" ");
                            sb.Append(box.cy);
                            sb.Append(" ");
                            sb.Append(box.width);
                            sb.Append(" ");
                            sb.Append(box.height);
                            sb.Append(" ");
                            box.points.ForEach(point =>
                            {
                                sb.Append(point.x);
                                sb.Append(" ");
                                sb.Append(point.y);
                                sb.Append(" ");
                                sb.Append(point.c);
                                sb.Append(" ");

                            });
                            sb.AppendLine("");
                        });
                        var label_text = sb.ToString();
                        if (label_text.Length > 0)
                        {
                            if (string.IsNullOrEmpty(idc.TextFileName))
                            {
                                var textfilename = Path.ChangeExtension(idc.ImageFileName, ".txt");
                                idc.TextFileName = textfilename;
                            }

                            System.IO.File.WriteAllText(idc.TextFileName, label_text);
                            Debug.WriteLine($"保存数据：{idc.TextFileName}:{label_text}");
                        }
                        else
                        {
                            if (string.IsNullOrEmpty(idc.TextFileName) == false && File.Exists(idc.TextFileName))
                            {
                                var tempfile = idc.TextFileName;

                                File.Delete(idc.TextFileName);
                                idc.TextFileName = null;
                                Debug.WriteLine($"删除数据文件：{idc.TextFileName}");
                            }

                        }

                    }

                    idc.Saved = true;

                });

            });

            ShowMessageDelayClose($"保存完毕，共{count}个数据文件");
        }

        private void toolStripButtonCollectibles_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var path = StaticLib.GetNewDirName(folderBrowserDialog.SelectedPath, $"Collect{DateTime.Now.ToString("yyyyMMdd")}");
                    Directory.CreateDirectory(path);
                    foreach (var workspace in CurrentWorkSpace)
                    {
                        foreach (var imagedata in workspace.ImageData)
                        {

                            var shortimagefilename = Path.GetFileName(imagedata.ImageFileName);
                            var longimagefilename = Path.Combine(path, shortimagefilename);
                            File.Copy(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = Path.GetFileName(imagedata.TextFileName);
                                var longtextfilename = Path.Combine(path, shorttextfilename);
                                File.Copy(imagedata.TextFileName, longtextfilename, true);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

        }

        private void toolStripButtonExport_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var trainpath = Path.Combine(folderBrowserDialog.SelectedPath, "train");
                    var trainimagepath = Path.Combine(trainpath, "images");
                    var trainlabelspath = Path.Combine(trainpath, "labels");
                    var validpath = Path.Combine(folderBrowserDialog.SelectedPath, "valid");
                    var validimagepath = Path.Combine(validpath, "images");
                    var validlabelspath = Path.Combine(validpath, "labels");
                    var yamlfilename = Path.Combine(folderBrowserDialog.SelectedPath, "data.yaml");

                    Directory.CreateDirectory(trainimagepath);
                    Directory.CreateDirectory(trainlabelspath);
                    Directory.CreateDirectory(validimagepath);
                    Directory.CreateDirectory(validlabelspath);


                    foreach (var workspace in CurrentWorkSpace)
                    {
                        Random random = new Random();

                        var tempimagedata = workspace.ImageData.Where(o => o.TextFileName != null).ToList();
                        #region 随机乱序
                        for (int i = 0; i < tempimagedata.Count(); i++)
                        {
                            var pos = random.Next(tempimagedata.Count());
                            var temp = tempimagedata[i];
                            tempimagedata[i] = tempimagedata[pos];
                            tempimagedata[pos] = temp;
                        }
                        #endregion
                        for (int i = 0; i < tempimagedata.Count(); i++)
                        {
                            string imagepath = null;
                            string textpath = null;
                            var imagedata = tempimagedata[i];
                            #region 80%训练，20%检验
                            if ((float)i / tempimagedata.Count() < 0.8)
                            {
                                imagepath = trainimagepath;
                                textpath = trainlabelspath;
                            }
                            else
                            {
                                imagepath = validimagepath;
                                textpath = validlabelspath;
                            }
                            #endregion
                            #region 取得文件名，复制
                            var shortimagefilename = Path.GetFileName(imagedata.ImageFileName);
                            var longimagefilename = Path.Combine(imagepath, shortimagefilename);
                            File.Copy(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = Path.GetFileName(imagedata.TextFileName);
                                var longtextfilename = Path.Combine(textpath, shorttextfilename);
                                File.Copy(imagedata.TextFileName, longtextfilename, true);
                            }
                            #endregion
                        }



                    }

                    SaveYoloDataYamlFile(yamlfilename, trainimagepath, validimagepath);
                    ShowMessageDelayClose($"输入yolo data{folderBrowserDialog.SelectedPath}完毕");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void toolStripButtonDetect_Click(object sender, EventArgs e)
        {
            if (CurrentImageData == null)
            {
                ShowMessageDelayClose("没有指定图片");
                return;
            }
            if (yolo == null)
            {
                ShowMessageDelayClose("没有指定模型");
                return;
            }

            try
            {
                //var image = SixLabors.ImageSharp.Image.Load(CurrentImageData.ImageFileName);
                var image = SixLabors.ImageSharp.Image.Load<Rgb24>(CurrentImageData.ImageFileName);
                var predictions = yolo.Pose(image);
                foreach (var box in predictions.Boxes)
                {
                    var labelid = box.Class.Id;
                    var checkanylabel = CurrentLabels.Where(o => o.Key == labelid).Any();
                    var labelname = checkanylabel ? CurrentLabels[labelid] : $"{box.Class.Name}";
                    var objectbox = new BoxClass(labelid,
                                            labelname,
                                            new System.Drawing.Size(image.Width, image.Height),
                                            new System.Drawing.Rectangle(box.Bounds.X, box.Bounds.Y, box.Bounds.Width, box.Bounds.Height));
                    if (box.Keypoints.Count != 0)
                        box.Keypoints.ForEach(o =>
                        {
                            objectbox.points.Add(new PointClass() { x = (double)o.Point.X / image.Width, y = (double)o.Point.Y / image.Height, c = 2 });
                        });
                    CurrentImageData.ObjectBox.Add(objectbox);
                    boundingPanelContainer1.SetImageDataClass(CurrentImageData);
                    ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
                }
            }
            catch (Exception ex)
            {
                ShowMessageDelayClose(ex.Message);
            }

            boundingPanelContainer1.Invalidate();

        }

        private void toolStripButtonSetLabel_Click(object sender, EventArgs e)
        {
            if (radioGroupLabel.SelectedIndex != -1 && CurrentObject is BoxClass)
            {
                var box = CurrentObject as BoxClass;
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                var selectname = (string)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Tag;
                box.Id = selectid;
                box.Name = selectname;
                boundingPanelContainer1.Invalidate();
                ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
                ShowObjectDataInTextBox(maskedTextBox, box);
            }

        }

        private void toolStripButtonAddTag_Click(object sender, EventArgs e)
        {
            frmInput input = new frmInput("添加标签", "请输入标签名：", "");
            if (input.ShowDialog() == DialogResult.OK && string.IsNullOrWhiteSpace(input.InputString) == false)
            {
                int id = 0;
                if (CurrentLabels.Count > 0)
                    id = CurrentLabels.Select(o => o.Key).Max() + 1;

                CurrentLabels.Add(id, input.InputString);
                radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem() { Value = id, Description = $"{id}:{input.InputString}", Tag = input.InputString });

            }


        }

        private void toolStripButtonRemoveTag_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("确定删除标签？", "警告", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Yes && radioGroupLabel.SelectedIndex != -1)
            {
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                CurrentLabels.Remove(selectid);
                radioGroupLabel.Properties.Items.RemoveAt(radioGroupLabel.SelectedIndex);
            }
        }

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            if (yolo != null)
            {
                //frmPlay frm = new frmPlay(yolo, listyololabel);
                //frm.Show();


            }

        }


        private void toolStripButtonLoadLabel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "yaml文件(*.yaml)|*.yaml|所有文件(*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                var yamltext = File.ReadAllText(openFileDialog.FileName);
                var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                try
                {
                    var yd = deserializer.Deserialize<YOLODataClass>(yamltext);
                    CurrentLabels.Clear();
                    radioGroupLabel.Properties.Items.Clear();
                    for (int i = 0; i < yd.names.Count; i++)
                    {
                        CurrentLabels.Add(i, yd.names[i]);
                        radioGroupLabel.Properties.Items.Add(new RadioGroupItem { Value = i, Description = $"{i}:{yd.names[i]}", Tag = yd.names[i] });
                    }


                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            ShowMessageDelayClose("载入标签完毕");
        }

        private void toolStripButtonSaveLabel_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "yaml文件(*.yaml)|*.yaml|所有文件(*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                SaveYoloDataYamlFile(saveFileDialog.FileName, null, null);

            }
            ShowMessageDelayClose("保存标签完毕");
        }

        private void toolStripButtonLabelManager_Click(object sender, EventArgs e)
        {
            frmLabel frm = new frmLabel(CurrentWorkSpace, CurrentLabels);
            frm.FormClosed += (s, e) =>
            {
                var labels = CurrentLabels.Select(o => new RadioGroupItem { Value = o.Key, Description = $"{o.Key}:{o.Value}", Tag = o.Value });
                radioGroupLabel.Properties.Items.Clear();
                radioGroupLabel.Properties.Items.AddRange(labels.ToArray());

                CurrentWorkSpace.ForEach(o =>
                {
                    o.ImageData.ForEach(p =>
                    {
                        var flagchanged = false;
                        p.ObjectBox.ForEach(q =>
                        {
                            var label = CurrentLabels.FirstOrDefault(x => x.Key == q.Id);
                            if (!label.Equals(default(KeyValuePair<int, string>)))
                            {
                                q.Name = label.Value;
                                flagchanged = true;
                            }

                        });
                        if (flagchanged)
                            p.Saved = false;


                    });
                });
            };
            frm.Show();
        }

        private void toolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            bind(CurrentWorkSpace, CurrentLabels, CurrentYoloModelFiles);

        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                var item = CurrentClassifierList.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
                if (item != null && item.Parameter.type == ShowParameterType.WorkSpace)
                {
                    var workspacepath = item.Parameter.Name;
                    var ws = CurrentWorkSpace.Where(o => o.WorkSpacePath == workspacepath).FirstOrDefault();
                    if (ws != null)
                    {
                        CurrentWorkSpace.Remove(ws);
                        bind(CurrentWorkSpace, CurrentLabels, CurrentYoloModelFiles);


                    }
                }
            }
            ShowMessageDelayClose("关闭文件夹完毕");
            CurrentObject = null;
            ShowObjectDataInTextBox(maskedTextBox, CurrentObject);
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GalleryRefresh();

        }


        private void toolStripButtonMove_Click(object sender, EventArgs e)
        {
            toolStripButtonMove.Checked = true;
            toolStripButtonAddBounding.Checked = false;
            toolStripButtonAddPoint.Checked = false;
            SettoolStripButtonIcon();
            boundingPanelContainer1.OperatingMode = OperatingModes.MoveBounding;
        }

        private void toolStripButtonAddBounding_Click(object sender, EventArgs e)
        {
            toolStripButtonMove.Checked = false;
            toolStripButtonAddBounding.Checked = true;
            toolStripButtonAddPoint.Checked = false;
            SettoolStripButtonIcon();
            boundingPanelContainer1.OperatingMode = OperatingModes.AddBounding;
        }
        private void toolStripButtonAddPoint_Click(object sender, EventArgs e)
        {
            toolStripButtonMove.Checked = false;
            toolStripButtonAddBounding.Checked = false;
            toolStripButtonAddPoint.Checked = true;
            SettoolStripButtonIcon();
            boundingPanelContainer1.OperatingMode = OperatingModes.AddPoint;
        }
        private void SettoolStripButtonIcon()
        {
            if (toolStripButtonMove.Checked)
                toolStripButtonMove.Image = Properties.Resources.icons8_move_50_checked;
            else
                toolStripButtonMove.Image = Properties.Resources.icons8_move_50;

            if (toolStripButtonAddBounding.Checked)
                toolStripButtonAddBounding.Image = Properties.Resources.bounding_box_check;
            else
                toolStripButtonAddBounding.Image = Properties.Resources.bounding_box;

            if (toolStripButtonAddPoint.Checked)
                toolStripButtonAddPoint.Image = Properties.Resources.icons8_collect_48_check;
            else
                toolStripButtonAddPoint.Image = Properties.Resources.icons8_collect_48;
        }
        private void toolStripMenuItemSaveProject_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "yaml文件(*.yaml)|*.yaml|所有文件(*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

                Project p = new Project(CurrentWorkSpace, CurrentLabels, CurrentClassifierList, CurrentYoloModelFiles);
                var yaml = serializer.Serialize(p);
                System.IO.File.WriteAllText(saveFileDialog.FileName, yaml);
            }
        }

        private void toolStripMenuItemLoadProject_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openFileDialog = new System.Windows.Forms.OpenFileDialog();
            openFileDialog.Filter = "yaml文件(*.yaml)|*.yaml|所有文件(*.*)|*.*";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var yamltext = File.ReadAllText(openFileDialog.FileName);
                    var deserializer = new DeserializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();
                    var p = deserializer.Deserialize<Project>(yamltext);
                    CurrentWorkSpace = p.listworkspace ?? new List<WorkSpaceClass>();
                    CurrentLabels = p.listyololabel ?? new Dictionary<int, string>();
                    CurrentClassifierList = p.listItemData ?? new List<Classifier>();
                    CurrentYoloModelFiles = p.listYoloModelFilePath ?? new List<string>();
                    bind(CurrentWorkSpace, CurrentLabels, CurrentYoloModelFiles);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }



        private void toolStripButtonRemove_Click(object sender, EventArgs e)
        {
            CurrentImageData.ObjectBox = new List<BoxClass>();
            if (File.Exists(CurrentImageData.TextFileName))
            {
                File.Delete(CurrentImageData.TextFileName);
                CurrentImageData.TextFileName = null;
            }

            boundingPanelContainer1.Invalidate();
            ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
            CurrentObject = null;
            ShowObjectDataInTextBox(maskedTextBox, CurrentObject);
        }




        //Size为输入单元格的尺寸
        private void CropImage(List<System.Drawing.Image> src, out System.Drawing.Image des, System.Drawing.Size size)
        {
            var count = src.Count();
            int w, h;
            w = h = (int)Math.Sqrt(count) + 1;
            des = new Bitmap(size.Width * w, size.Height * h);
            using (Graphics g = Graphics.FromImage(des))
            {
                int x = 0, y = 0;
                src.ForEach(o =>
                {
                    //var img = FixedSize(o, size.Width, size.Height);
                    g.DrawImage(o, new System.Drawing.Rectangle(x, y, size.Width, size.Height));
                    x += size.Width;
                    if (x >= size.Width * w)
                    {
                        x = 0;
                        y += size.Height;
                    }

                });
            }
        }

        private void toolStripButtonThumbnail_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "PNG文件(*.png)|*.png|所有文件(*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                List<System.Drawing.Image> listImage = GetListImage();
                System.Drawing.Image descimage;
                CropImage(listImage, out descimage, new System.Drawing.Size(100, 100));
                descimage.Save(saveFileDialog.FileName, ImageFormat.Png);

            }
            ShowMessageDelayClose("生成缩略图完毕");


        }

        private List<System.Drawing.Image> GetListImage()
        {
            List<System.Drawing.Image> listImage = new List<System.Drawing.Image>();

            if (comboBox1.SelectedIndex != -1)
            {
                var item = CurrentClassifierList.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
                if (item.Parameter.type == ShowParameterType.WorkSpace)
                {
                    galleryControl1.Gallery.Groups.ForEach(o =>
                    {
                        o.Items.ForEach(p =>
                        {
                            var idc = p.Value as ImageDataClass;
                            var image = (System.Drawing.Image)new Bitmap(idc.ImageFileName);
                            using (var g = Graphics.FromImage(image))
                            {
                                var filename = Path.GetFileName(idc.ImageFileName);
                                g.DrawString(filename, SystemFonts.CaptionFont, Brushes.LightPink, new System.Drawing.Point(0, 0));
                            }
                            listImage.Add(image);
                        });
                    });


                }
                if (item.Parameter.type == ShowParameterType.Label)
                {
                    galleryControl1.Gallery.Groups.ForEach(o =>
                    {
                        o.Items.ForEach(p =>
                        {
                            var idc = p.Value as ImageDataClass;
                            var Oimage = (System.Drawing.Image)new Bitmap(idc.ImageFileName);
                            idc.ObjectBox.Where(o => o.Id == item.Parameter.Id).ForEach(q =>
                            {
                                var x = (int)(q.x * Oimage.Width);
                                var y = (int)(q.y * Oimage.Height);
                                var w = (int)(q.width * Oimage.Width);
                                var h = (int)(q.height * Oimage.Height);
                                var image = new Bitmap(w, h);
                                using (var g = Graphics.FromImage(image))
                                {
                                    g.DrawImage(Oimage, new System.Drawing.Rectangle(0 + 1, 0 + 1, w - 2, h - 2), new System.Drawing.Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                                    var filename = Path.GetFileName(idc.ImageFileName);
                                    g.DrawString(filename, SystemFonts.CaptionFont, Brushes.LightPink, new System.Drawing.Point(0, 0));
                                }
                                listImage.Add((System.Drawing.Image)image);

                            });

                        });
                    });
                }



            }
            else
            {
                CurrentWorkSpace.ForEach(ws =>
                {
                    ws.ImageData.ForEach(idc =>
                    {
                        listImage.Add((System.Drawing.Image)new Bitmap(idc.ImageFileName));
                    });
                });

            }

            return listImage;
        }

        private void SortImage(List<ImageDataClass> listidc)
        {
            //List<KeyValuePair<ImageDataClass, Mat[]>> listHist = new List<KeyValuePair<ImageDataClass, Mat[]>>();
            listidc.ForEach(imagedata =>
            {

                Mat panda = new Mat(imagedata.ImageFileName, ImreadModes.AnyColor);//读取为彩图

                Mat[] mats = Cv2.Split(panda);//一张图片，将panda拆分成3个图片装进mat
                Mat[] mats0 = new Mat[] { mats[0] };//panda的第一个通道，也就是B
                Mat[] mats1 = new Mat[] { mats[1] };//panda的第二个通道，也就是G
                Mat[] mats2 = new Mat[] { mats[2] };//panda的第三个通道，也就是R

                Mat[] hist = new Mat[] { new Mat(), new Mat(), new Mat() };//一个矩阵数组，用来接收直方图,记得全部初始化
                int[] channels = new int[] { 0 };//一个通道,初始化为通道0,这些东西可以共用设置一个就行
                int[] histsize = new int[] { 256 };//一个通道，初始化为256箱子
                Rangef[] range = new Rangef[1] { new Rangef(0.0f, 256f) };
                Mat mask = new Mat();//不做掩码

                Cv2.CalcHist(mats0, channels, mask, hist[0], 1, histsize, range);//对被拆分的图片单独进行计算
                Cv2.CalcHist(mats1, channels, mask, hist[1], 1, histsize, range);//对被拆分的图片单独进行计算
                Cv2.CalcHist(mats2, channels, mask, hist[2], 1, histsize, range);//对被拆分的图片单独进行计算

                //KeyValuePair<ImageDataClass, Mat[]> kv = new KeyValuePair<ImageDataClass, Mat[]>(imagedata, new Mat[] { hist[0], hist[1], hist[3] });
                //listHist.Add(kv);
                imagedata.hist = new Mat[] { hist[0], hist[1], hist[2] };

            });

            for (int x = 0; x < listidc.Count - 1; x++)
            {
                double? score_min = 1;
                int y_min = x + 1;
                for (int y = x + 1; y < listidc.Count; y++)
                {
                    var xhist = listidc[x].hist;
                    var yhist = listidc[y].hist;

                    var score = Cv2.CompareHist(xhist[0], yhist[0], HistCompMethods.Bhattacharyya)
                              + Cv2.CompareHist(xhist[1], yhist[1], HistCompMethods.Bhattacharyya)
                              + Cv2.CompareHist(xhist[2], yhist[2], HistCompMethods.Bhattacharyya);
                    Debug.WriteLine($"{Math.Round(100f * (x + 1) / listidc.Count, 2, MidpointRounding.AwayFromZero)}% {listidc[x].ImageFileName} vs {listidc[y].ImageFileName} : {score}");
                    if (score_min == null)
                        score_min = score;
                    if (score < score_min)
                    {
                        score_min = score;
                        y_min = y;


                    }
                }
                var temp = listidc[x];
                listidc[x] = listidc[y_min];
                listidc[y_min] = temp;
            }



        }

        private void 排序ToolStripMenuItem_Click(object sender, EventArgs e)
        {

            //var listimagedata = GetListImageData();
            //SoftImage(listimagedata);
            //bind(listworkspace, listyololabel, listYoloModelFilePath, comboBox1.SelectedIndex);
            Random radnom = new Random();
            var listimagedata = GetListImageData();
            for (int i = 0; i < listimagedata.Count; i++)
            {
                var index = radnom.Next(listimagedata.Count);
                var temp = listimagedata[i];
                listimagedata[i] = listimagedata[index];
                listimagedata[index] = temp;
            }
            listimagedata = listimagedata.Take(100).ToList();
            var listimage1 = listimagedata.Select(o => (System.Drawing.Image)new Bitmap(o.ImageFileName)).ToList();

            System.Drawing.Image descimage;
            CropImage(listimage1, out descimage, new System.Drawing.Size(100, 100));
            descimage.Save("1.png", ImageFormat.Png);
            SortImage(listimagedata);


            var listimage2 = listimagedata.Select(o => (System.Drawing.Image)new Bitmap(o.ImageFileName)).ToList();
            CropImage(listimage2, out descimage, new System.Drawing.Size(100, 100));
            descimage.Save("2.png", ImageFormat.Png);

            //bind(listworkspace, listyololabel, listYoloModelFilePath, comboBox1.SelectedIndex);
        }

        public List<ImageDataClass> GetListImageData()
        {
            List<ImageDataClass> listidc = new List<ImageDataClass>();
            if (comboBox1.SelectedIndex != -1)
            {
                var item = CurrentClassifierList.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
                if (item.Parameter.type == ShowParameterType.WorkSpace)
                {
                    galleryControl1.Gallery.Groups.ForEach(o =>
                    {
                        o.Items.ForEach(p =>
                        {
                            var idc = p.Value as ImageDataClass;
                            listidc.Add(idc);
                        });
                    });


                }
            }
            return listidc;
        }

        private void toggleSwitch1_EditValueChanged(object sender, EventArgs e)
        {
            SelectWeightsFiles();
        }

        private void 添加ONNX文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog fileDialog = new System.Windows.Forms.OpenFileDialog();
            fileDialog.Filter = "ONNX文件(*.onnx)|*.onnx|所有文件(*.*)|*.*";

            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                var checkfilename = CurrentYoloModelFiles.Where(o => string.Equals(o, fileDialog.FileName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkfilename == false)
                {
                    CurrentYoloModelFiles.Add(fileDialog.FileName);
                    radioGroupWeight.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(fileDialog.FileName, fileDialog.FileName, true, fileDialog.FileName));
                }
            }
        }

        private void 删除ONNX文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (radioGroupWeight.SelectedIndex != -1)
            {
                var r = MessageBox.Show("确定删除权重文件？", "警告", MessageBoxButtons.YesNoCancel);
                if (r == DialogResult.Yes)
                {
                    var ModelFilePath = (string)radioGroupWeight.Properties.Items[radioGroupWeight.SelectedIndex].Value;
                    CurrentYoloModelFiles.Remove(CurrentYoloModelFiles.Where(o => string.Equals(o, ModelFilePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
                    radioGroupWeight.Properties.Items.RemoveAt(radioGroupWeight.SelectedIndex);
                    radioGroupWeight.SelectedIndex = -1;

                }
            }
        }



        private void radioGroupLabel_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (radioGroupLabel.SelectedIndex == -1)
            {
                boundingPanelContainer1.LabelId = -1;
                boundingPanelContainer1.LabelName = null;
            }
            else
            {
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                var selectname = (string)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Tag;
                boundingPanelContainer1.LabelId = selectid;
                boundingPanelContainer1.LabelName = selectname;
            }
        }

        private void toolStripButtonZoomIn_Click(object sender, EventArgs e)
        {
            boundingPanelContainer1.Width = (int)(boundingPanelContainer1.Width * 1.2f);
            boundingPanelContainer1.Height = (int)(boundingPanelContainer1.Height * 1.2f);
        }

        private void toolStripButtonZoomOut_Click(object sender, EventArgs e)
        {
            boundingPanelContainer1.Width = (int)(boundingPanelContainer1.Width * 0.8f);
            boundingPanelContainer1.Height = (int)(boundingPanelContainer1.Height * 0.8f);
        }

        private void maskedTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var ctrl = sender as MaskedTextBox;
            if (CurrentObject is BoxClass)
            {
                var box = CurrentObject as BoxClass;
                string input = ctrl.Text.Trim();

                // 使用正则表达式验证输入格式，并捕获整数和浮点数的数值
                string pattern = @"^(\d+)\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)$";
                Match match = Regex.Match(input, pattern);

                if (!match.Success)
                {
                    // 输入格式不正确，取消验证
                    e.Cancel = true;
                    ShowMessageDelayClose("输入格式不正确");
                }

            }
            if (CurrentObject is PointClass)
            {
                var point = CurrentObject as PointClass;
                string input = ctrl.Text.Trim();

                // 使用正则表达式验证输入格式，并捕获浮点数和整数的数值
                string pattern = @"^([\d.]+)\s+([\d.]+)\s+(\d+)$";
                Match match = Regex.Match(input, pattern);

                if (!match.Success)
                {
                    // 输入格式不正确，取消验证
                    e.Cancel = true;
                    ShowMessageDelayClose("输入格式不正确");
                }

            }
        }

        private void maskedTextBox_Validated(object sender, EventArgs e)
        {
            var ctrl = sender as MaskedTextBox;

            if (CurrentObject is BoxClass)
            {
                var box = CurrentObject as BoxClass;
                string input = ctrl.Text.Trim();

                // 使用正则表达式验证输入格式，并捕获整数和浮点数的数值
                string pattern = @"^(\d+)\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)\s+([\d.]+)$";
                Match match = Regex.Match(input, pattern);

                if (!match.Success)
                {
                    ShowMessageDelayClose("输入格式不正确");
                    return;
                }

                // 提取捕获的整数和浮点数的数值
                int intValue = int.Parse(match.Groups[1].Value);
                float floatValue1 = float.Parse(match.Groups[2].Value);
                float floatValue2 = float.Parse(match.Groups[3].Value);
                float floatValue3 = float.Parse(match.Groups[4].Value);
                float floatValue4 = float.Parse(match.Groups[5].Value);

                // 在这里使用提取到的整数和浮点数的数值进行处理

                box.Id = intValue;
                box.cx = floatValue1;
                box.cy = floatValue2;
                box.width = floatValue3;
                box.height = floatValue4;

                boundingPanelContainer1.Invalidate();
                ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
            }
            else if (CurrentObject is PointClass)
            {
                var point = CurrentObject as PointClass;
                string input = ctrl.Text.Trim();

                // 使用正则表达式验证输入格式，并捕获浮点数和整数的数值
                string pattern = @"^([\d.]+)\s+([\d.]+)\s+(\d+)$";
                Match match = Regex.Match(input, pattern);

                if (!match.Success)
                {
                    ShowMessageDelayClose("输入格式不正确");
                    return;
                }

                // 提取捕获的浮点数和整数的数值
                float floatValue1 = float.Parse(match.Groups[1].Value);
                float floatValue2 = float.Parse(match.Groups[2].Value);
                int intValue = int.Parse(match.Groups[3].Value);

                // 在这里使用提取到的浮点数和整数的数值进行处理
                point.x = floatValue1;
                point.y = floatValue2;
                point.c = intValue;
                boundingPanelContainer1.Invalidate();
                ShowImageDataInTextBox(textBoxImageData, CurrentImageData);
            }
        }
    }
}

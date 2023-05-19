using DevExpress.Utils;
using DevExpress.XtraBars.Ribbon.ViewInfo;
using DevExpress.XtraEditors.Controls;
using Microsoft.ML.OnnxRuntime;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Yolo5.NetCore;
using Yolo5.NetCore.Models;

//请描述以下程序开发过程：1、程序用于yolov5训练所需的图像预处理。2、程序可以打开视频文件，裁剪视频开始、结束位置，设置帧率，转换为图片。3、程序可以对标注图片区域和标签。4、程序可以管理标签名称、编号，保存标签信息。5、程序可以管理已标注、未标注的图片，将已标注的图片随机分选为训练和校验图片。6、程序可以生成标注区域的缩略图，将所有缩略图合并为单个图片文件以供查验标注的准确性。7‘、程序可以使用指定的onnx模型，识别图片区域和类型。8、程序可以使用指定的onnx模型，识别视频内容的区域和类型。9、程序可以指定视频中指定物件动作范围，对物件进行移动跟踪，进行或者离开区域进行计数。


namespace ImageLabel
{
    public partial class frmMain : Form
    {
        List<ItemData> listItemData = new List<ItemData>();
        List<WorkSpaceClass> listworkspace = new List<WorkSpaceClass>();
        List<YoloLabelModel> listyololabel = new List<YoloLabelModel>();
        List<string> listYoloModelFilePath = new List<string> { };
        YOLOv5<EmptyYoloModel> yolo;


        ImageDataClass CurrentImageData;
        System.Drawing.Image CurrentImage;
        TransPannel CurrentMaskPanel = null;

        bool MouseDownFlag = false;
        System.Drawing.Point MouseDownLoc = new System.Drawing.Point();
        System.Drawing.Point MouseMoveLoc = new System.Drawing.Point();



        public frmMain()
        {
            InitializeComponent();

        }

        private void frmWinMain_Load(object sender, EventArgs e)
        {
            listyololabel = StaticLib.YoloLabels;

            var labels = listyololabel.Select(o => new RadioGroupItem { Value = o.Id, Description = $"{o.Id}:{o.Name}", Tag = o.Name });
            radioGroupLabel.Properties.Items.AddRange(labels.ToArray());
            //Task.Run(() =>
            //{
            //    FileInfo onnxfile = new FileInfo(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, @".\weights\7\best.onnx"));

            //    yolo = new Yolo<EmptyYoloModel>(onnxfile.FullName);

            //});
            //bindComboBoxEdit(listworkspace, listyololabel);
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
                myws = new WorkSpaceClass(CurrentDirectory, listyololabel, (m) => { this.Invoke(() => { ShowMessage(m); }); });
                myws.WorkSpacePath = CurrentDirectory;
                sw.Stop();
            });

            Debug.WriteLine($"读取文件夹完成，用时{sw.ElapsedMilliseconds / 1000f}秒");


            return myws;
        }

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var ctrl = sender as PictureBox;
            if (CurrentImage != null)
            {
                //var image = FixedSize(CurrentImage, ctrl.ClientSize.Width, ctrl.ClientSize.Height);
                e.Graphics.FillRectangle(Brushes.White, ctrl.Bounds);
                e.Graphics.DrawImage(CurrentImage, new System.Drawing.Point(0, 0));
            }
            else
            {

                e.Graphics.Clear(BackColor);
                //e.Graphics.FillRectangle(Brushes.White,new Rectangle(0,0,ctrl.Width,ctrl.Height));
            }
        }
        private void pictureBox1_Resize(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
        }

        private void splitContainerControl2_Panel1_ClientSizeChanged(object sender, EventArgs e)
        {
            var size = splitContainerControl2.Panel1.ClientSize / 2 - pictureBox1.Size / 2;
            //Debug.WriteLine($"{splitContainerControl2.Panel1.ClientSize},{pictureBox1.Size},{size}");
            pictureBox1.Location = new System.Drawing.Point(size.Width, 0);
        }

        private void galleryControl1_Gallery_CustomDrawItemImage(object sender, DevExpress.XtraBars.Ribbon.GalleryItemCustomDrawEventArgs e)
        {
            GalleryItemViewInfo vInfo = e.ItemInfo as GalleryItemViewInfo;
            var item = e.Item.Value as ImageDataClass;
            if (!vInfo.ImageInfo.IsLoaded)
            {
                using (var image = System.Drawing.Image.FromFile(item.ImageFileName))
                {
                    if (image != null)
                    {
                        var cloneimage = (System.Drawing.Image)ImageSharpExtensions.FixedSize(image, 416, 416);
                        e.Cache.DrawString(item.ImageFileName.Substring(item.ImageFileName.LastIndexOf("\\") + 1), System.Drawing.SystemFonts.SmallCaptionFont, Brushes.LightPink, e.Bounds);

                        vInfo.ImageInfo.Image = cloneimage;
                        vInfo.ImageInfo.ThumbImage = cloneimage;// FixedSize(cloneimage, e.Bounds.Width, e.Bounds.Height);
                        vInfo.ImageInfo.IsLoaded = true;

                        //e.Cache.DrawImage(cloneimage, e.Bounds, new System.Drawing.Rectangle(0, 0, cloneimage.Width, cloneimage.Height), true);
                        //e.Cache.DrawString(item.ImageFileName.Substring(item.ImageFileName.LastIndexOf("\\") + 1), System.Drawing.SystemFonts.SmallCaptionFont, Brushes.White, e.Bounds);
                    }
                    e.Handled = true;
                }
            }
            else
            {
                ImageCollection.DrawImageListImage(e.Cache, vInfo.ImageInfo.Image, vInfo.ImageInfo.ThumbImage, vInfo.Item.ImageIndex, vInfo.ImageContentBounds, vInfo.IsEnabled);
                e.Cache.DrawString(item.ImageFileName.Substring(item.ImageFileName.LastIndexOf("\\") + 1), System.Drawing.SystemFonts.SmallCaptionFont, Brushes.LightPink, e.Bounds);
                e.Handled = true;
            }
            //Debug.WriteLine($"{DateTime.Now.ToString("mm:ss")}\tvInfo.ImageInfo.IsLoaded:{vInfo.ImageInfo.IsLoaded}\tImageFileName:{item.ImageFileName}");
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
                        //转文件夹
                        //获取文件夹的所有图片、标签
                        //对比是否有重复图片
                        //对比是否有重复标签
                        //显示对话框
                        //用户选择复制图片、复制标签、提升标签值，
                        var nws = await GetWorkSpace(o);
                        //this.Invoke(() =>
                        //{
                        //    //imageListBoxControl1.DisplayMember = "ImageFileName";
                        //    //imageListBoxControl1.DataSource = nws.WorkSpaceFiles;

                        //    var path = nws.WorkSpacePath.Substring(nws.WorkSpacePath.LastIndexOf("\\") + 1);
                        //    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = path });
                        //    nws.ImageData.ForEach(o =>
                        //    {
                        //        galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem() { Value = o, Hint = o.ImageFileName.Substring(o.ImageFileName.LastIndexOf("\\") + 1) });
                        //    });


                        //});
                        listworkspace.Add(nws);


                    }
                    if (File.Exists(o))
                    {
                        var ext = o.Substring(o.LastIndexOf("."));
                        if (StaticLib.VideoFormat.Contains(ext))
                        {
                            //转视频
                            //1、获取视频总帧数、帧每秒、时间长度
                            //2、显示对话框
                            //3、用户选择按帧率、开始时间、结束时间
                            //4、截取视频保存图片
                            //
                            //if (ws != null)
                            //{
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
                                        //Debug.WriteLine($"SetPosFrames:{i}");
                                        capture.Set(VideoCaptureProperties.PosFrames, i);
                                        var mat = capture.RetrieveMat();
                                        if (frm.XFlip)
                                            mat = mat.Flip(OpenCvSharp.FlipMode.X);
                                        if (frm.YFlip)
                                            mat = mat.Flip(OpenCvSharp.FlipMode.Y);

                                        var filename = StaticLib.GetNewFileName(path, DateTime.Now.ToString("yyyyMMdd"), ".jpg");
                                        mat.SaveImage(filename);

                                    }
                                    var myws = new WorkSpaceClass(path, listyololabel, (m) => this.Invoke(() => { ShowMessage(m); }));
                                    listworkspace.Add(myws);
                                    //ws = WorkSpaceClass.Add(ws, myws);
                                    //this.Invoke(() =>
                                    //{
                                    //    //imageListBoxControl1.DisplayMember = "ImageFileName";
                                    //    //imageListBoxControl1.DataSource = myws.WorkSpaceFiles;
                                    //    var path = myws.WorkSpacePath.Substring(myws.WorkSpacePath.LastIndexOf("\\") + 1);
                                    //    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = path });
                                    //    myws.ImageData.ForEach(o =>
                                    //    {
                                    //        galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem() { Value = o, Hint = o.ImageFileName.Substring(o.ImageFileName.LastIndexOf("\\") + 1) });
                                    //    });
                                    //});


                                });

                            }
                            //}
                        }
                        else if (StaticLib.ImageFormat.Contains(ext))
                        {

                        }
                        else if (StaticLib.ONNXFormat.Contains(ext))
                        {

                            var c = radioGroupWeight.Properties.Items.Where(p => p.Value.ToString() == o).Any();
                            if (c == false)
                            {
                                listYoloModelFilePath.Add(o);
                                this.Invoke(() => { radioGroupWeight.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(o, o, true, o)); });

                            }
                        }

                    }
                    this.Invoke(() =>
                    {
                        bind(listworkspace, listyololabel, listYoloModelFilePath);

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
                var lastitem = listItemData.Where(o => o.Parameter.type == ShowParameterType.WorkSpace && o.Parameter.tag == ShowParameterTag.显示全部).Select(o => o.Parameter.Text).LastOrDefault();
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
        public void bind(List<WorkSpaceClass> listworkspace, List<YoloLabelModel> listlabels, List<string> listModelsFile, int? selectindex = null)
        {
            listItemData.Clear();
            List<YoloLabelModel> listAvailableLabels = new();
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
                    listItemData.Add(new ItemData { Text = p.Text, Parameter = p });
                }
                ws.ImageData.ForEach(imagedata =>
                {
                    imagedata.ObjectBox.ForEach(objectbox =>
                    {
                        var flag = listAvailableLabels.Where(o => o.Id == objectbox.Id).Any();
                        if (flag == false)
                            listAvailableLabels.Add(new YoloLabelModel { Id = objectbox.Id, Name = objectbox.Name });

                    });

                });
            });

            listAvailableLabels = listAvailableLabels.Join(listlabels, x => x.Id, y => y.Id, (x, y) => new YoloLabelModel { Id = x.Id, Name = y.Name }).ToList();
            listAvailableLabels.ForEach(label =>
            {
                var tag = ShowParameterTag.显示已标签;
                ItemParameter p = new ItemParameter
                {
                    type = ShowParameterType.Label,
                    tag = tag,
                    Id = label.Id,
                    Name = label.Name,
                    Text = $"Label:{label.Id}-{label.Name},{StaticLib.GetShowParameterTagName(tag)}"
                };
                listItemData.Add(new ItemData { Text = p.Text, Parameter = p });

            });
            comboBox1.Items.Clear();
            listItemData.ForEach(o =>
            {
                var index = comboBox1.Items.Add(o.Text);

            });
            SelectLastItem(selectindex);

            var labels = listlabels.Select(o => new RadioGroupItem { Value = o.Id, Description = $"{o.Id}:{o.Name}", Tag = o.Name }).ToArray();
            radioGroupLabel.Properties.Items.Clear();
            radioGroupLabel.Properties.Items.AddRange(labels);

            var weights = listModelsFile.Select(o => new RadioGroupItem { Value = o, Description = o, Tag = o }).ToArray();
            radioGroupWeight.Properties.Items.Clear();
            radioGroupWeight.Properties.Items.AddRange(weights);
        }

        private void GalleryRefresh()
        {
            var ctrl = comboBox1;
            var allitem = listItemData.Where(o => o.Text == (string)ctrl.SelectedItem).ToList();

            //var allitem = ctrl.Properties.Items.Where(o => o.CheckState == CheckState.Checked).Select(o => o.Value).Cast<ItemParameter>().ToList();
            galleryControl1.Gallery.Groups.Clear();

            galleryControl1.Gallery.BeginUpdate();
            allitem.ForEach(o =>
            {
                if (o.Parameter.type == ShowParameterType.WorkSpace)
                {
                    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = o.Text });


                    listworkspace.ForEach(ws =>
                    {
                        if (o.Parameter.tag == ShowParameterTag.显示全部)
                        {
                            ws.ImageData.ForEach(imagedata =>
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1)
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
                                    Hint = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1)
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
                                    Hint = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1)
                                });
                            });
                        }
                    });




                }
                if (o.Parameter.type == ShowParameterType.Label)
                {
                    var gindex = galleryControl1.Gallery.Groups.Add(new DevExpress.XtraBars.Ribbon.GalleryItemGroup() { Caption = o.Text });
                    listworkspace.ForEach(ws =>
                    {
                        ws.ImageData.ForEach(imagedata =>
                        {
                            var flag = imagedata.ObjectBox.Where(objectbox => objectbox.Id == o.Parameter.Id).Any();
                            if (flag)
                            {
                                galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem()
                                {
                                    Value = imagedata,
                                    Hint = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1)
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
            using (var image = System.Drawing.Image.FromFile(CurrentImageData.ImageFileName))
            {
                CurrentImage = new Bitmap(image);
                pictureBox1.Size = CurrentImage.Size;
                SetPictureLabelBox(pictureBox1, CurrentImageData);
                SetTextBox(textBox1, CurrentImageData);
                var size = splitContainerControl2.Panel1.ClientSize / 2 - pictureBox1.Size / 2;
                //Debug.WriteLine($"{splitContainerControl2.Panel1.ClientSize},{pictureBox1.Size},{size}");
                Debug.WriteLine($"{panel2.AutoScrollPosition.ToString()}");
                panel2.AutoScrollPosition = new System.Drawing.Point(0, 0);
                pictureBox1.Location = new System.Drawing.Point(size.Width, 0);

                pictureBox1.Refresh();

            }


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
                        yolo = null;
                        FileInfo onnxfile = new FileInfo(onnxfilepath);
                        if (toggleSwitch1.IsOn)
                        {
                            SessionOptions options = new SessionOptions();
                            options.AppendExecutionProvider_DML();
                            //options.AppendExecutionProvider_CUDA();
                            yolo = new YOLOv5<EmptyYoloModel>(onnxfile.FullName, options);
                        }

                        else
                            yolo = new YOLOv5<EmptyYoloModel>(onnxfile.FullName);

                        this.Invoke(() => ShowMessageDelayClose($"打开模型文件：{onnxfilepath}"));
                    }
                    catch (Exception ex)
                    {

                        this.Invoke(() => { MessageBox.Show(ex.Message); });
                    }
                });
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {

            if (CurrentImageData != null && MouseDownFlag == true && CurrentMaskPanel != null && radioGroupLabel.SelectedIndex != -1)
            {

                MouseMoveLoc.X = e.X;
                MouseMoveLoc.Y = e.Y;
                (var x, var y, var w, var h) = CalBounds(MouseDownLoc, MouseMoveLoc);
                CurrentMaskPanel.SetBounds(x, y, w, h);
                CurrentMaskPanel.box.SetBounds(new System.Drawing.Size((int)CurrentImageData.ImageWidth, (int)CurrentImageData.ImageHeight), CurrentMaskPanel.Bounds);
                SetTextBox(textBox1, CurrentImageData);
                //Debug.WriteLine($"{x},{y}-{w},{h}");
            }
            //Debug.WriteLine($"pictureBox1_MouseMove:MouseDownFlog:{MouseDownFlag} \t MouseDownLoc:{MouseDownLoc} \t MouseMoveLoc:{MouseMoveLoc}");
        }
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            var pb = sender as PictureBox;
            if (CurrentImageData != null && MouseDownFlag == false && radioGroupLabel.SelectedIndex != -1)
            {
                var LabelId = radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value as int?;
                var LableName = radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Tag as string;
                MouseDownFlag = true;

                MouseDownLoc.X = e.X;
                MouseDownLoc.Y = e.Y;
                var box = new ObjectBoxClass(LabelId.Value, LableName, new System.Drawing.Size((int)CurrentImageData.ImageWidth, (int)CurrentImageData.ImageHeight), new System.Drawing.Rectangle());

                CurrentMaskPanel = CreatePictureLable(pictureBox1, CurrentImageData, box);
                CurrentMaskPanel.SetBounds(e.X, e.Y, 0, 0);
                CurrentMaskPanel.box.SetBounds(new System.Drawing.Size((int)CurrentImageData.ImageWidth, (int)CurrentImageData.ImageHeight), CurrentMaskPanel.Bounds);

                CurrentImageData.ObjectBox.Add(CurrentMaskPanel.box);
                CurrentImageData.Saved = false;


                //MaskPanel = new TransPannel(boxinit);
                //SetTextBox(textBox1, imagedata);
                //pictureBox1.Controls.Add(MaskPanel);

                //MaskPanel.ExitClick += new EventHandler((s, e) =>
                //{
                //    pb.Controls.Remove(MaskPanel);
                //    imagedata.ObjectBox.Remove(MaskPanel.box);
                //    SetTextBox(textBox1, imagedata);

                //});
                //MaskPanel.Resize += new EventHandler((s, e) =>
                //{

                //    var ctrl = s as TransPannel;
                //    var box = MaskPanel.box;
                //    StaticLib.BoundsToYoloData(new System.Drawing.Size((int)imagedata.ImageWidth, (int)imagedata.ImageHeight), ctrl.Bounds, ref box);
                //    imagedata.Saved = false;
                //    SetTextBox(textBox1, imagedata);
                //    //Debug.WriteLine($"Resize:{ctrl.Bounds}");
                //});
                //MaskPanel.LocationChanged += new EventHandler((s, e) =>
                //{
                //    var ctrl = s as TransPannel;
                //    var box = MaskPanel.box;
                //    StaticLib.BoundsToYoloData(new System.Drawing.Size((int)imagedata.ImageWidth, (int)imagedata.ImageHeight), ctrl.Bounds, ref box);
                //    imagedata.Saved = false;
                //    SetTextBox(textBox1, imagedata);
                //    //Debug.WriteLine($"LocationChanged:{ctrl.Bounds}");
                //});

            }
            else
            {

                if (radioGroupLabel.SelectedIndex == -1)
                {
                    ShowMessageDelayClose("未选择标签");

                }
            }
            //Debug.WriteLine($"pictureBox1_MouseDown:MouseDownFlog:{MouseDownFlag} \t MouseDownLoc:{MouseDownLoc}");
        }
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {

            if (CurrentImageData != null && MouseDownFlag == true && radioGroupLabel.SelectedIndex != -1)
            {
                if (CurrentMaskPanel.Bounds.Width > 0.01 && CurrentMaskPanel.Bounds.Height > 0.01)
                    CurrentMaskPanel.box.SetBounds(new System.Drawing.Size((int)CurrentImageData.ImageWidth, (int)CurrentImageData.ImageHeight), CurrentMaskPanel.Bounds);
                else
                {
                    CurrentImageData.ObjectBox.Remove(CurrentImageData.ObjectBox.Last());
                    SetTextBox(textBox1, CurrentImageData);
                    ShowMessageDelayClose("宽度或高度小于阈值");
                }
                MouseDownFlag = false;
            }
            //Debug.WriteLine($"pictureBox1_MouseUp:MouseDownFlog:{MouseDownFlag}");
        }


        private void ShowMessage(string msg)
        {
            toolStripStatusLabelForm.Text = msg;
        }

        private void ShowMessageDelayClose(string msg)
        {
            toolStripStatusLabelForm.Text = msg;
            Task.Run(async () => { await Task.Delay(3000); this.Invoke(() => toolStripStatusLabelForm.Text = ""); });
        }

        private (int x, int y, int w, int h) CalBounds(System.Drawing.Point xy1, System.Drawing.Point xy2)
        {
            int x, y, w, h;
            if (xy1.X <= xy2.X)
            {
                x = xy1.X;
                w = xy2.X - xy1.X;
            }
            else
            {
                x = xy2.X;
                w = xy1.X - xy2.X;
            }

            if (xy1.Y <= xy2.Y)
            {
                y = xy1.Y;
                h = xy2.Y - xy1.Y;
            }
            else
            {
                y = xy2.Y;
                h = xy1.Y - xy2.Y;
            }
            return (x, y, w, h);
        }

        private void SetPictureLabelBox(PictureBox pb, ImageDataClass idc)
        {
            if (pb != null)
            {
                pb.Controls.Clear();
                textBox1.Text = "";
                idc.ObjectBox.ForEach(o =>
                {
                    CreatePictureLable(pb, idc, o);

                });

            }
        }

        private TransPannel CreatePictureLable(PictureBox pb, ImageDataClass idc, ObjectBoxClass box)
        {
            TransPannel t = new TransPannel(box);
            t.ExitClick += new EventHandler((s, e) =>
            {
                pb.Controls.Remove(t);
                idc.ObjectBox.Remove(box);
                SetTextBox(textBox1, idc);
                idc.Saved = false;
            });
            t.Resize += new EventHandler((s, e) =>
            {

                var ctrl = s as TransPannel;
                StaticLib.BoundsToYoloData(new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), ctrl.Bounds, ref box);
                CurrentImageData.Saved = false;
                SetTextBox(textBox1, idc);
                //MaskPanel.box = o;
                //Debug.WriteLine($"Resize:{ctrl.Bounds}");
            });
            t.LocationChanged += new EventHandler((s, e) =>
            {
                var ctrl = s as TransPannel;
                StaticLib.BoundsToYoloData(new System.Drawing.Size((int)idc.ImageWidth, (int)idc.ImageHeight), ctrl.Bounds, ref box);
                CurrentImageData.Saved = false;
                SetTextBox(textBox1, idc);
                //MaskPanel.box = o;
                //Debug.WriteLine($"LocationChanged:{ctrl.Bounds}");
            });
            t.SetBounds((int)(box.x * idc.ImageWidth), (int)(box.y * idc.ImageHeight), (int)(box.width * idc.ImageWidth), (int)(box.height * idc.ImageHeight));
            pb.Controls.Add(t);
            return t;
        }

        private void SetTextBox(System.Windows.Forms.TextBox tb, ImageDataClass idc)
        {
            if (tb != null)
            {
                tb.Text = "";
                idc.ObjectBox.ForEach(o =>
                {
                    tb.Text += $"{o.Id} {o.cx} {o.cy} {o.width} {o.height}\r\n";
                });

            }

        }

        private void 新增标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            frmInput input = new frmInput("添加标签", "请输入标签名：", "");
            if (input.ShowDialog() == DialogResult.OK && string.IsNullOrWhiteSpace(input.InputString) == false)
            {
                int id = 0;
                if (listyololabel.Count > 0)
                    id = listyololabel.Select(o => o.Id).Max() + 1;

                listyololabel.Add(new YoloLabelModel() { Id = id, Name = input.InputString });
                radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem() { Value = id, Description = $"{id}:{input.InputString}", Tag = input.InputString });

            }
        }

        private void 删除标签ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("确定删除标签？", "警告", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Yes && radioGroupLabel.SelectedIndex != -1)
            {
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                listyololabel.Remove(listyololabel.Where(o => o.Id == selectid).FirstOrDefault());
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
                    foreach (var workspace in listworkspace)
                    {
                        foreach (var imagedata in workspace.ImageData)
                        {

                            var shortimagefilename = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1);
                            var longimagefilename = Path.Combine(path, shortimagefilename);
                            File.Move(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = imagedata.TextFileName.Substring(imagedata.TextFileName.LastIndexOf("\\") + 1);
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
                    galleryControl1.Gallery.Groups[gindex].Items.Add(new DevExpress.XtraBars.Ribbon.GalleryItem() { Value = o, Hint = o.ImageFileName.Substring(o.ImageFileName.LastIndexOf("\\") + 1) });
                });


                //radioGroupLabel.Properties.Items.Clear();
                //nws.Labels.ForEach(o =>
                //{
                //    radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(o.Key, o.Key.ToString() + ":" + o.Value, true, o.Value));

                //});
                listworkspace.Clear();
                listworkspace.Add(nws);

                ShowMessageDelayClose($"归集文件夹{path}完毕");
            }
        }


        //private void 移除文件夹ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    listworkspace.Clear();
        //    CurrentImageData = null;
        //    galleryControl1.Gallery.Groups.Clear();
        //    pictureBox1.Controls.Clear();
        //    //radioGroupLabel.Properties.Items.Clear();

        //    CurrentImage = null;
        //    ShowMessageDelayClose("移除完毕");
        //    textBox1.Text = "";
        //}


        //private void 分集复制ToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    //train,80%文件放这里
        //    //--images
        //    //--labels
        //    //valid，20%文件放这里
        //    //--images
        //    //--labels
        //    //data.yaml
        //    FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
        //    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
        //    {
        //        try
        //        {
        //            var trainpath = Path.Combine(folderBrowserDialog.SelectedPath, "train");
        //            var trainimagepath = Path.Combine(trainpath, "images");
        //            var trainlabelspath = Path.Combine(trainpath, "labels");
        //            var validpath = Path.Combine(folderBrowserDialog.SelectedPath, "valid");
        //            var validimagepath = Path.Combine(validpath, "images");
        //            var validlabelspath = Path.Combine(validpath, "labels");
        //            var yamlfilename = Path.Combine(folderBrowserDialog.SelectedPath, "data.yaml");

        //            Directory.CreateDirectory(trainimagepath);
        //            Directory.CreateDirectory(trainlabelspath);
        //            Directory.CreateDirectory(validimagepath);
        //            Directory.CreateDirectory(validlabelspath);


        //            foreach (var workspace in listworkspace)
        //            {
        //                Random random = new Random();

        //                var tempimagedata = workspace.ImageData.Where(o => o.TextFileName != null).ToList();
        //                for (int i = 0; i < tempimagedata.Count(); i++)
        //                {
        //                    var pos = random.Next(tempimagedata.Count());
        //                    var temp = tempimagedata[i];
        //                    tempimagedata[i] = tempimagedata[pos];
        //                    tempimagedata[pos] = temp;
        //                }
        //                for (int i = 0; i < tempimagedata.Count(); i++)
        //                {
        //                    string imagepath = null;
        //                    string textpath = null;
        //                    var imagedata = tempimagedata[i];
        //                    if ((float)i / tempimagedata.Count() < 0.8)
        //                    {
        //                        imagepath = trainimagepath;
        //                        textpath = trainlabelspath;
        //                    }
        //                    else
        //                    {
        //                        imagepath = validimagepath;
        //                        textpath = validlabelspath;
        //                    }
        //                    var shortimagefilename = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1);
        //                    var longimagefilename = Path.Combine(imagepath, shortimagefilename);
        //                    File.Copy(imagedata.ImageFileName, longimagefilename, true);
        //                    if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
        //                    {
        //                        var shorttextfilename = imagedata.TextFileName.Substring(imagedata.TextFileName.LastIndexOf("\\") + 1);
        //                        var longtextfilename = Path.Combine(textpath, shorttextfilename);
        //                        File.Copy(imagedata.TextFileName, longtextfilename, true);
        //                    }
        //                }


        //                //List<KeyValuePair<ImageDataClass, Mat>> listHist = new List<KeyValuePair<ImageDataClass, Mat>>();




        //                //foreach (var imagedata in workspace.ImageData)
        //                //{
        //                //Mat panda = new Mat(imagedata.ImageFileName, ImreadModes.AnyColor);//读取为彩图

        //                //Mat[] mats = Cv2.Split(panda);//一张图片，将panda拆分成3个图片装进mat
        //                //Mat[] mats0 = new Mat[] { mats[0] };//panda的第一个通道，也就是B
        //                //Mat[] mats1 = new Mat[] { mats[1] };//panda的第二个通道，也就是G
        //                //Mat[] mats2 = new Mat[] { mats[2] };//panda的第三个通道，也就是R

        //                //Mat[] hist = new Mat[] { new Mat(), new Mat(), new Mat() };//一个矩阵数组，用来接收直方图,记得全部初始化
        //                //int[] channels = new int[] { 0 };//一个通道,初始化为通道0,这些东西可以共用设置一个就行
        //                //int[] histsize = new int[] { 256 };//一个通道，初始化为256箱子
        //                //Rangef[] range = new Rangef[1] { new Rangef(0.0f, 256f) };
        //                //Mat mask = new Mat();//不做掩码

        //                //Cv2.CalcHist(mats0, channels, mask, hist[0], 1, histsize, range);//对被拆分的图片单独进行计算
        //                //Cv2.CalcHist(mats1, channels, mask, hist[1], 1, histsize, range);//对被拆分的图片单独进行计算
        //                //Cv2.CalcHist(mats2, channels, mask, hist[2], 1, histsize, range);//对被拆分的图片单独进行计算


        //                ////Cv2.Add(hist[0], hist[1], hist[0]);
        //                ////Cv2.Add(hist[0], hist[2], hist[0]);

        //                //listHist.Add(new KeyValuePair<ImageDataClass, Mat>(imagedata, hist[0]));                     

        //                //}
        //                //for (int i = 0; i < listHist.Count - 1; i++)
        //                //             {
        //                //                 var mat1 = listHist[i].Value;
        //                //                 var mat2 = listHist[i + 1].Value;

        //                //                 var compare = Cv2.CompareHist(mat1, mat2, HistCompMethods.Correl);
        //                //                 Debug.WriteLine($"{listHist[i].Key.ImageFileName} : {listHist[i + 1].Key.ImageFileName} ={compare}");


        //                //             }


        //            }

        //            SaveCocoYamlFile(yamlfilename, trainimagepath, validimagepath);
        //            ShowMessageDelayClose($"分集文件夹{folderBrowserDialog.SelectedPath}完毕");
        //        }
        //        catch (Exception ex)
        //        {
        //            MessageBox.Show(ex.Message);
        //        }
        //    }
        //}

        private void SaveCocoYamlFile(string yamlfilename, string trainimagepath, string validimagepath)
        {
            YOLODataClass y = new YOLODataClass();

            y.names = new List<string>();
            y.names.AddRange(listyololabel.Select(o => o.Name).ToArray());
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
                listworkspace.Add(nws);
                bind(listworkspace, listyololabel, listYoloModelFilePath);

                ShowMessageDelayClose($"打开文件夹{CurrentDirectory}完毕");
            }
        }

        private void toolStripButtonOpenFile_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonSave_Click(object sender, EventArgs e)
        {
            var count = 0;
            listworkspace.ForEach(o =>
            {

                o.ImageData.Where(o => o.Saved == false).ForEach(p =>
                {
                    count++;
                    if (p.ObjectBox != null)
                    {
                        if (string.IsNullOrEmpty(p.TextFileName))
                        {
                            var fullpath = p.ImageFileName.Substring(0, p.ImageFileName.LastIndexOf("\\"));
                            var imagefilename = p.ImageFileName.Substring(p.ImageFileName.LastIndexOf('\\') + 1);
                            var textfilename = Path.Combine(fullpath, imagefilename.Substring(0, imagefilename.LastIndexOf(".")) + ".txt");
                            p.TextFileName = textfilename;
                        }

                        StringBuilder sb = new StringBuilder();
                        p.ObjectBox.ForEach(q =>
                        {
                            sb.Append(q.Id);
                            sb.Append(" ");
                            sb.Append(q.cx);
                            sb.Append(" ");
                            sb.Append(q.cy);
                            sb.Append(" ");
                            sb.Append(q.width);
                            sb.Append(" ");
                            sb.Append(q.height);
                            sb.AppendLine("");
                        });

                        System.IO.File.WriteAllText(p.TextFileName, sb.ToString());
                        Debug.WriteLine($"保存数据：{p.TextFileName}:{sb.ToString()}");
                    }

                    p.Saved = true;

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
                    foreach (var workspace in listworkspace)
                    {
                        foreach (var imagedata in workspace.ImageData)
                        {

                            var shortimagefilename = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1);
                            var longimagefilename = Path.Combine(path, shortimagefilename);
                            File.Copy(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = imagedata.TextFileName.Substring(imagedata.TextFileName.LastIndexOf("\\") + 1);
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


                    foreach (var workspace in listworkspace)
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
                            var shortimagefilename = imagedata.ImageFileName.Substring(imagedata.ImageFileName.LastIndexOf("\\") + 1);
                            var longimagefilename = Path.Combine(imagepath, shortimagefilename);
                            File.Copy(imagedata.ImageFileName, longimagefilename, true);
                            if (string.IsNullOrEmpty(imagedata.TextFileName) == false)
                            {
                                var shorttextfilename = imagedata.TextFileName.Substring(imagedata.TextFileName.LastIndexOf("\\") + 1);
                                var longtextfilename = Path.Combine(textpath, shorttextfilename);
                                File.Copy(imagedata.TextFileName, longtextfilename, true);
                            }
                            #endregion
                        }


                        //List<KeyValuePair<ImageDataClass, Mat>> listHist = new List<KeyValuePair<ImageDataClass, Mat>>();




                        //foreach (var imagedata in workspace.ImageData)
                        //{
                        //Mat panda = new Mat(imagedata.ImageFileName, ImreadModes.AnyColor);//读取为彩图

                        //Mat[] mats = Cv2.Split(panda);//一张图片，将panda拆分成3个图片装进mat
                        //Mat[] mats0 = new Mat[] { mats[0] };//panda的第一个通道，也就是B
                        //Mat[] mats1 = new Mat[] { mats[1] };//panda的第二个通道，也就是G
                        //Mat[] mats2 = new Mat[] { mats[2] };//panda的第三个通道，也就是R

                        //Mat[] hist = new Mat[] { new Mat(), new Mat(), new Mat() };//一个矩阵数组，用来接收直方图,记得全部初始化
                        //int[] channels = new int[] { 0 };//一个通道,初始化为通道0,这些东西可以共用设置一个就行
                        //int[] histsize = new int[] { 256 };//一个通道，初始化为256箱子
                        //Rangef[] range = new Rangef[1] { new Rangef(0.0f, 256f) };
                        //Mat mask = new Mat();//不做掩码

                        //Cv2.CalcHist(mats0, channels, mask, hist[0], 1, histsize, range);//对被拆分的图片单独进行计算
                        //Cv2.CalcHist(mats1, channels, mask, hist[1], 1, histsize, range);//对被拆分的图片单独进行计算
                        //Cv2.CalcHist(mats2, channels, mask, hist[2], 1, histsize, range);//对被拆分的图片单独进行计算


                        ////Cv2.Add(hist[0], hist[1], hist[0]);
                        ////Cv2.Add(hist[0], hist[2], hist[0]);

                        //listHist.Add(new KeyValuePair<ImageDataClass, Mat>(imagedata, hist[0]));                     

                        //}
                        //for (int i = 0; i < listHist.Count - 1; i++)
                        //             {
                        //                 var mat1 = listHist[i].Value;
                        //                 var mat2 = listHist[i + 1].Value;

                        //                 var compare = Cv2.CompareHist(mat1, mat2, HistCompMethods.Correl);
                        //                 Debug.WriteLine($"{listHist[i].Key.ImageFileName} : {listHist[i + 1].Key.ImageFileName} ={compare}");


                        //             }


                    }

                    SaveCocoYamlFile(yamlfilename, trainimagepath, validimagepath);
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
            if (CurrentImage == null)
            {
                ShowMessageDelayClose("没有指定图片");
                return;
            }
            if (yolo == null)
            {
                ShowMessageDelayClose("没有指定模型");
                return;
            }


            using var mat_img = ((Bitmap)CurrentImage).ToMat().CvtColor(ColorConversionCodes.RGBA2RGB);

            var predictions = yolo.Predict(mat_img);
            foreach (var prediction in predictions)
            {
                var labelid = prediction.LabelId;
                var checkanylabel = listyololabel.Where(o => o.Id == labelid).Any();
                var labelname = checkanylabel ? listyololabel[labelid].Name : $"{labelid}";
                CurrentImageData.ObjectBox.Add(
                    new ObjectBoxClass(labelid,
                                        labelname,
                                        new System.Drawing.Size(CurrentImage.Width, CurrentImage.Height),
                                        new System.Drawing.Rectangle((int)prediction.Rectangle.X, (int)prediction.Rectangle.Y, (int)prediction.Rectangle.Width, (int)prediction.Rectangle.Height)));

            }

            SetPictureLabelBox(pictureBox1, CurrentImageData);

        }

        private void toolStripButtonSwab_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButtonAddTag_Click(object sender, EventArgs e)
        {
            frmInput input = new frmInput("添加标签", "请输入标签名：", "");
            if (input.ShowDialog() == DialogResult.OK && string.IsNullOrWhiteSpace(input.InputString) == false)
            {
                int id = 0;
                if (listyololabel.Count > 0)
                    id = listyololabel.Select(o => o.Id).Max() + 1;

                listyololabel.Add(new YoloLabelModel() { Id = id, Name = input.InputString });
                radioGroupLabel.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem() { Value = id, Description = $"{id}:{input.InputString}", Tag = input.InputString });

            }


        }

        private void toolStripButtonRemoveTag_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("确定删除标签？", "警告", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Yes && radioGroupLabel.SelectedIndex != -1)
            {
                var selectid = (int)radioGroupLabel.Properties.Items[radioGroupLabel.SelectedIndex].Value;
                listyololabel.Remove(listyololabel.Where(o => o.Id == selectid).FirstOrDefault());
                radioGroupLabel.Properties.Items.RemoveAt(radioGroupLabel.SelectedIndex);
            }
        }

        private void toolStripButtonPlay_Click(object sender, EventArgs e)
        {
            if (yolo != null)
            {
                frmPlay frm = new frmPlay(yolo, listyololabel);
                frm.Show();


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
                    listyololabel.Clear();
                    radioGroupLabel.Properties.Items.Clear();
                    for (int i = 0; i < yd.names.Count; i++)
                    {
                        listyololabel.Add(new YoloLabelModel() { Id = i, Name = yd.names[i] });
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
                SaveCocoYamlFile(saveFileDialog.FileName, null, null);

            }
            ShowMessageDelayClose("保存标签完毕");
        }

        private void toolStripButtonLabelManager_Click(object sender, EventArgs e)
        {
            frmLabel frm = new frmLabel(listworkspace, listyololabel);
            frm.FormClosed += (s, e) =>
            {
                var labels = listyololabel.Select(o => new RadioGroupItem { Value = o.Id, Description = $"{o.Id}:{o.Name}", Tag = o.Name });
                radioGroupLabel.Properties.Items.Clear();
                radioGroupLabel.Properties.Items.AddRange(labels.ToArray());

                listworkspace.ForEach(o =>
                {
                    o.ImageData.ForEach(p =>
                    {
                        var flagchanged = false;
                        p.ObjectBox.ForEach(q =>
                        {
                            var label = listyololabel.Where(x => x.Id == q.Id).FirstOrDefault();
                            if (label != null)
                            {
                                q.Name = label.Name;
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
            bind(listworkspace, listyololabel, listYoloModelFilePath);

        }

        private void toolStripButtonClose_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex != -1)
            {
                var item = listItemData.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
                if (item != null && item.Parameter.type == ShowParameterType.WorkSpace)
                {
                    var workspacepath = item.Parameter.Name;
                    var ws = listworkspace.Where(o => o.WorkSpacePath == workspacepath).FirstOrDefault();
                    if (ws != null)
                    {
                        listworkspace.Remove(ws);
                        bind(listworkspace, listyololabel, listYoloModelFilePath);


                    }
                }
            }
            ShowMessageDelayClose("关闭文件夹完毕");
        }
        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            GalleryRefresh();

        }


        private void toolStripButtonMove_Click(object sender, EventArgs e)
        {
            radioGroupLabel.SelectedIndex = -1;
        }

        private void toolStripButtonAdjust_Click(object sender, EventArgs e)
        {
            radioGroupLabel.SelectedIndex = -1;
        }

        private void toolStripMenuItemSaveProject_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.Filter = "yaml文件(*.yaml)|*.yaml|所有文件(*.*)|*.*";
            saveFileDialog.RestoreDirectory = true;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var serializer = new SerializerBuilder().WithNamingConvention(UnderscoredNamingConvention.Instance).Build();

                Project p = new Project(listworkspace, listyololabel, listItemData, listYoloModelFilePath);
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
                    listworkspace = p.listworkspace ?? new List<WorkSpaceClass>();
                    listyololabel = p.listyololabel ?? new List<YoloLabelModel>();
                    listItemData = p.listItemData ?? new List<ItemData>();
                    listYoloModelFilePath = p.listYoloModelFilePath ?? new List<string>();
                    bind(listworkspace, listyololabel, listYoloModelFilePath);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }



        private void toolStripButtonRemove_Click(object sender, EventArgs e)
        {
            CurrentImageData.ObjectBox = new List<ObjectBoxClass>();
            if (File.Exists(CurrentImageData.TextFileName))
            {
                File.Delete(CurrentImageData.TextFileName);
                CurrentImageData.TextFileName = null;
            }

            SetPictureLabelBox(pictureBox1, CurrentImageData);
            SetTextBox(textBox1, CurrentImageData);
        }




        //Size为输入单元格的尺寸
        private void CropImage(List<Image> src, out Image des, System.Drawing.Size size)
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
                    g.DrawImage(o, new Rectangle(x, y, size.Width, size.Height));
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
                List<Image> listImage = GetListImage();
                Image descimage;
                CropImage(listImage, out descimage, new System.Drawing.Size(100, 100));
                descimage.Save(saveFileDialog.FileName, ImageFormat.Png);

            }
            ShowMessageDelayClose("生成缩略图完毕");


        }

        private List<Image> GetListImage()
        {
            List<Image> listImage = new List<Image>();

            if (comboBox1.SelectedIndex != -1)
            {
                var item = listItemData.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
                if (item.Parameter.type == ShowParameterType.WorkSpace)
                {
                    galleryControl1.Gallery.Groups.ForEach(o =>
                    {
                        o.Items.ForEach(p =>
                        {
                            var idc = p.Value as ImageDataClass;
                            var image = (Image)new Bitmap(idc.ImageFileName);
                            using (var g = Graphics.FromImage(image))
                            {
                                g.DrawString(idc.ImageFileName.Substring(idc.ImageFileName.LastIndexOf("\\")), SystemFonts.CaptionFont, Brushes.LightPink, new System.Drawing.Point(0, 0));
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
                            var Oimage = (Image)new Bitmap(idc.ImageFileName);
                            idc.ObjectBox.Where(o => o.Id == item.Parameter.Id).ForEach(q =>
                            {
                                var x = (int)(q.x * Oimage.Width);
                                var y = (int)(q.y * Oimage.Height);
                                var w = (int)(q.width * Oimage.Width);
                                var h = (int)(q.height * Oimage.Height);
                                var image = new Bitmap(w, h);
                                using (var g = Graphics.FromImage(image))
                                {
                                    g.DrawImage(Oimage, new Rectangle(0 + 1, 0 + 1, w - 2, h - 2), new Rectangle(x, y, w, h), GraphicsUnit.Pixel);
                                    g.DrawString(idc.ImageFileName.Substring(idc.ImageFileName.LastIndexOf("\\")), SystemFonts.CaptionFont, Brushes.LightPink, new System.Drawing.Point(0, 0));
                                }
                                listImage.Add((Image)image);

                            });

                        });
                    });
                }



            }
            else
            {
                listworkspace.ForEach(ws =>
                {
                    ws.ImageData.ForEach(idc =>
                    {
                        listImage.Add((Image)new Bitmap(idc.ImageFileName));
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
            var listimage1 = listimagedata.Select(o => (Image)new Bitmap(o.ImageFileName)).ToList();

            Image descimage;
            CropImage(listimage1, out descimage, new System.Drawing.Size(100, 100));
            descimage.Save("1.png", ImageFormat.Png);
            SortImage(listimagedata);


            var listimage2 = listimagedata.Select(o => (Image)new Bitmap(o.ImageFileName)).ToList();
            CropImage(listimage2, out descimage, new System.Drawing.Size(100, 100));
            descimage.Save("2.png", ImageFormat.Png);

            //bind(listworkspace, listyololabel, listYoloModelFilePath, comboBox1.SelectedIndex);
        }

        public List<ImageDataClass> GetListImageData()
        {
            List<ImageDataClass> listidc = new List<ImageDataClass>();
            if (comboBox1.SelectedIndex != -1)
            {
                var item = listItemData.Where(o => o.Text == (string)comboBox1.SelectedItem).FirstOrDefault();
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
                var checkfilename = listYoloModelFilePath.Where(o => string.Equals(o, fileDialog.FileName, StringComparison.OrdinalIgnoreCase)).Any();
                if (checkfilename == false)
                {
                    listYoloModelFilePath.Add(fileDialog.FileName);
                    radioGroupWeight.Properties.Items.Add(new DevExpress.XtraEditors.Controls.RadioGroupItem(fileDialog.FileName, fileDialog.FileName, true, fileDialog.FileName));
                }
            }
        }

        private void 删除ONNX文件ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var r = MessageBox.Show("确定删除标签？", "警告", MessageBoxButtons.YesNoCancel);
            if (r == DialogResult.Yes && radioGroupWeight.SelectedIndex != -1)
            {
                var ModelFilePath = (string)radioGroupWeight.Properties.Items[radioGroupWeight.SelectedIndex].Value;
                listYoloModelFilePath.Remove(listYoloModelFilePath.Where(o => string.Equals(o, ModelFilePath, StringComparison.OrdinalIgnoreCase)).FirstOrDefault());
                radioGroupWeight.Properties.Items.RemoveAt(radioGroupWeight.SelectedIndex);
            }
        }
    }
}

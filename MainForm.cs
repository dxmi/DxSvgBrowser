using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DevExpress.Utils.Svg;
using DevExpress.XtraEditors;

namespace SvgBrowser {
    public partial class MainForm : XtraForm {
        DirectoryInfo parent;

        public MainForm() {
            InitializeComponent();
            gridView1.FocusedRowChanged += gridView1_FocusedRowChanged;
            gridView1.CustomUnboundColumnData += gridView1_CustomUnboundColumnData;
            gridControl1.KeyUp += gridControl1_KeyUp;
            Navigate(null);
        }

        void gridView1_CustomUnboundColumnData(object sender, DevExpress.XtraGrid.Views.Base.CustomColumnDataEventArgs e) {
            if(e.Column != null && e.Column.FieldName == "Icon") {
                if(e.IsGetData) {
                    var info = e.Row as FileSystemInfo;
                    if(info != null) {
                        e.Value = (info.Attributes & FileAttributes.Directory) == FileAttributes.Directory ? "#" : "-";
                    } else {
                        e.Value = "?";
                    }
                }
            }
        }

        void gridControl1_KeyUp(object sender, KeyEventArgs e) {
            if(e.KeyCode == Keys.Enter) {
                e.Handled = true;
                GoDown();
            }
        }

        void GoDown() {
            DirectoryInfo directoryInfo = null;
            if(gridView1.IsDataRow(gridView1.FocusedRowHandle)) {
                directoryInfo = gridView1.GetRow(gridView1.FocusedRowHandle) as DirectoryInfo;
                parent = directoryInfo;
                RefreshFileList();
            }
        }

        void gridView1_FocusedRowChanged(object sender, DevExpress.XtraGrid.Views.Base.FocusedRowChangedEventArgs e) {
            FileInfo fileInfo = null;
            if(gridView1.IsDataRow(e.FocusedRowHandle)) {
                fileInfo = gridView1.GetRow(e.FocusedRowHandle) as FileInfo;
            }
            UpdateDetails(fileInfo);
        }

        void UpdateDetails(FileInfo fileInfo) {
            Image image = null;
            if(fileInfo != null) {
                try {
                    SvgBitmap svg = SvgBitmap.FromFile(fileInfo.FullName);
                    image = svg.Render(null);
                } catch(Exception ex) { }
            }
            if(pictureEdit1.Image != null) {
                pictureEdit1.Image.Dispose();
            }
            pictureEdit1.Image = image;
        }

        void barButtonItem1_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            if(parent == null)
                return;
            parent = parent.Parent;
            RefreshFileList();
        }

        private void barButtonItem2_ItemClick(object sender, DevExpress.XtraBars.ItemClickEventArgs e) {
            parent = null;
            RefreshFileList();
        }

        void RefreshFileList() {
            var list = new List<FileSystemInfo>();
            if(parent == null) {
                foreach(var driveInfo in DriveInfo.GetDrives()) {
                    if(driveInfo.IsReady) {
                        list.Add(driveInfo.RootDirectory);
                    }
                }
            } else {
                list.AddRange(parent.GetDirectories());
                list.AddRange(parent.GetFiles("*.svg", SearchOption.TopDirectoryOnly));
            }
            gridControl1.DataSource = list;
        }

        void Navigate(string path) {
            parent = path == null ? null : new DirectoryInfo(path);
            RefreshFileList();
        }
    }
}

using AppStarter.Helpers;
using AppStarter.Models;
using AppStarter.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AppStarter
{
    public partial class FormMain : Form
    {
        const string DataFile = "AppStarter.data";
        public AppStartData Data { get; set; }
        private List<TreeNode> CategoryNodes { get; set; }

        public FormMain()
        {
            InitializeComponent();
            CategoryNodes = new List<TreeNode>();
        }

        private void LoadData()
        {
            if (File.Exists(DataFile))
            {
                Data = XmlHelper.DeserializeFromFile<AppStartData>(DataFile);
            }
            else
            {
                Data = new AppStartData();

                //var path = Environment.ExpandEnvironmentVariables("%systemroot%\\explorer.exe");
                //var icon = (Icon)Icon.ExtractAssociatedIcon(path).Clone();
                //treeView1.ImageList.Images.Add(icon);

                //Data.Items.Add(new AppStartItem()
                //{
                //    Category = "Test",
                //    Text = "Text",
                //    Path = path,
                //    IconID = icon,
                //});
            }
            CreateTreeView();
        }
        private void CreateTreeView()
        {
            var imageIdx = 0;
            treeView1.ImageList = new ImageList();

            foreach (var cat in Data.Categories)
            {
                CreateCategory(cat);
            }

            foreach (var item in Data.Items)
            {
                if (File.Exists(item.Path))
                {
                    var icon = (Icon)Icon.ExtractAssociatedIcon(item.Path).Clone();
                    treeView1.ImageList.Images.Add(icon);
                    CreateStartItem(item, imageIdx);
                    imageIdx++;
                }
            }
        }
        private void CreateCategory(string text)
        {
            var categoryNode = new TreeNode { Text = text };
            treeView1.Nodes.Add(categoryNode);
            CategoryNodes.Add(categoryNode);
        }
        private void CreateStartItem(AppStartItem item, int imageIdx)
        {
            var catNode = CategoryNodes.FirstOrDefault(c => c.Text == item.Category);

            if (catNode != null)
            {
                var starterNode = new TreeNode { Text = item.Text, ImageIndex = imageIdx };
                catNode.Nodes.Add(starterNode);
            }
            else
            {
                throw new NoNullAllowedException();
            }
        }
        private void SaveData()
        {
            XmlHelper.SerializeToFile<AppStartData>(Data, DataFile);
        }
        private void OpenData()
        {
            // ProcessStart
            // Dialog YesNo - Reload Data?
        }

        #region EVENTS
        // FORM
        private void FormMain_Load(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.Size.Width == 0 || Properties.Settings.Default.Size.Height == 0)
            {
                // first start
                Properties.Settings.Default.Location = this.Location;
                Properties.Settings.Default.Size = this.Size;
                Properties.Settings.Default.WindowState = this.WindowState;
            }
            else
            {
                this.WindowState = Properties.Settings.Default.WindowState;

                // we don't want a minimized window at startup
                if (this.WindowState == FormWindowState.Minimized) this.WindowState = FormWindowState.Normal;

                this.Location = Properties.Settings.Default.Location;
                this.Size = Properties.Settings.Default.Size;
            }

            LoadData();
        }
        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Hide();
            e.Cancel = true;

        }

        // NOTIFY-ICON
        private void editAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenData();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseApp();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RestoreApp();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RestoreApp();
        }

        #endregion

        protected override void WndProc(ref Message message)
        {
            if (message.Msg == SingleInstance.WM_SHOWFIRSTINSTANCE)
            {
                ShowWindow();
            }
            base.WndProc(ref message);
        }

        public void ShowWindow()
        {
            // Insert code here to make your form show itself.
            WinApi.ShowToFront(this.Handle);
        }

        private void CloseApp()
        {
            Properties.Settings.Default.WindowState = this.WindowState;
            if (this.WindowState == FormWindowState.Normal)
            {
                // save location and size if the state is normal
                Properties.Settings.Default.Location = this.Location;
                Properties.Settings.Default.Size = this.Size;
            }
            else
            {
                // save the RestoreBounds if the form is minimized or maximized!
                Properties.Settings.Default.Location = this.RestoreBounds.Location;
                Properties.Settings.Default.Size = this.RestoreBounds.Size;
            }

            // don't forget to save the settings
            Properties.Settings.Default.Save();

            notifyIcon1.Dispose();

            SaveData();

            Environment.Exit(0);
        }

        private void RestoreApp()
        {
            //this.TopMost = true;
            //this.ShowInTaskbar = true;
            this.BringToFront();
            this.Show();
            this.Activate();
            //this.TopMost = false;
        }
        private void HideApp()
        {
            //this.ShowInTaskbar = false;
            this.Hide();
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
#if (!DEBUG)
            this.HideApp();
#endif 
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var hitTest = e.Node.TreeView.HitTest(e.Location);
            if (hitTest.Location == TreeViewHitTestLocations.PlusMinus)
                return;

            if (e.Node.IsExpanded)
                e.Node.Collapse();
            else
                e.Node.Expand();
        }
    }
}

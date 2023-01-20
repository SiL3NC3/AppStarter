using AppStarter.Controls;
using AppStarter.Helpers;
using AppStarter.Models;
using AppStarter.Threading;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AppStarter
{
    public partial class FormMain : Form
    {
        #region SINGLE INSTANCE Implementation
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
        #endregion

        bool startBlocked = false;
        bool preventHide = false;

        const string DataFile = "Apps.xml";
        readonly Size DefaultIconSize = new Size(24, 24);

        public AppStartData Data { get; set; }
        private List<TreeNode> CategoryNodes { get; set; }

        public FormMain()
        {
            InitializeComponent();
            CategoryNodes = new List<TreeNode>();
            var v = AssemblyHelper.GetVersion();
            this.Text = this.Text + $" v{v.Major}.{v.Minor}";

        }

        private void LoadData()
        {
            treeView1.Nodes.Clear();
            treeView1.ImageList = new ImageList();
            treeView1.ImageList.ImageSize = DefaultIconSize;
            treeView1.ImageList.Images.Add(new Bitmap(1, 1));

            if (File.Exists(DataFile))
            {
                Data = XmlHelper.DeserializeFromFile<AppStartData>(DataFile);
            }
            else
            {
                Data = new AppStartData();

                var explorer = Environment.ExpandEnvironmentVariables("%systemroot%\\explorer.exe");

                Data.Items.Add(new AppStartItem()
                {
                    ID = 1,
                    Category = "Basic",
                    Text = "Explorer",
                    Path = explorer
                });

                Data.Items.Add(new AppStartItem()
                {
                    ID = 2,
                    Category = "Basic",
                    Text = "CMD",
                    Path = @"C:\Windows\System32\cmd.exe",
                    Arguments = ""
                });

                SaveData();
            }
            CreateTreeView();
        }

        private void CreateTreeView()
        {
            var imageIdx = 1;

            CategoryNodes.Clear();
            treeView1.Nodes.Clear();

            foreach (var cat in Data.Categories)
            {
                CreateCategory(cat);
            }

            foreach (var item in Data.Items)
            {
                if (item.Path.EndsWith("\\"))
                {
                    if (Directory.Exists(item.Path))
                    {
                        CreateStartItem(item, 1);
                    }
                }
                else if (File.Exists(item.Path))
                {
                    var icon = (Icon)Icon.ExtractAssociatedIcon(item.Path).Clone();
                    treeView1.ImageList.Images.Add(icon);
                    CreateStartItem(item, imageIdx);
                    imageIdx++;
                }
                else
                {
                    CreateStartItem(item, 0);
                }
            }

            foreach (TreeNode node in treeView1.Nodes)
            {
                node.Expand();
            }
        }

        private void CreateCategory(string text)
        {
            if (text == null)
                text = "(no category)";

            var categoryNode = new TreeNode { Text = text, ImageIndex = 0, SelectedImageIndex = 0 };
            treeView1.Nodes.Add(categoryNode);
            CategoryNodes.Add(categoryNode);
        }
        private void CreateStartItem(AppStartItem item, int imageIdx)
        {
            if (item.Category == null)
                item.Category = "(no category)";

            var catNode = CategoryNodes.FirstOrDefault(c => c.Text == item.Category);

            if (catNode != null)
            {
                var starterNode = new TreeNode
                {
                    Tag = item.ID,
                    Text = item.Text,
                    ImageIndex = imageIdx,
                    SelectedImageIndex = imageIdx,
                    ToolTipText = item.Path
                };

                if (imageIdx == 0)
                    starterNode.ForeColor = Color.Red;

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
            Process.Start($"{Environment.CurrentDirectory}\\{DataFile}");
        }

        private void UpdateContextMenu()
        {
            AppStartItem appStartItem = null;

            try
            {
                var selectedNode = treeView1.SelectedNode;

                if (selectedNode != null && selectedNode.Nodes.Count == 0)
                {
                    //Console.WriteLine($"StartItem({selectedNode.Text})");
                    //appStartItem = Data.Items.FirstOrDefault(i => i.Text == selectedNode.Text);

                    // IS START-ITEM
                    newToolStripMenuItem.Enabled = false;
                    editToolStripMenuItem.Enabled = true;
                    deleteToolStripMenuItem.Enabled = true;
                }
                else
                {
                    // IS CATEGORY-ITEM
                    newToolStripMenuItem.Enabled = true;
                    editToolStripMenuItem.Enabled = false;
                    deleteToolStripMenuItem.Enabled = false;

                }
            }
            catch (Exception ex)
            {
                var msg = $"Start-Pfad:{Environment.NewLine}" +
                          $"  {appStartItem.Path}{Environment.NewLine}" +
                          $"Fehler:{Environment.NewLine}" +
                          $"  {ex.Message}";
                MessageBox.Show(this, msg, "FEHLER beim Aktualisieren des Kontextmenüs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void StartItem()
        {
            if (startBlocked)
                return;

            AppStartItem appStartItem = null;

            try
            {
                var selectedNode = treeView1.SelectedNode;

                if (selectedNode != null && selectedNode.Nodes.Count == 0 && selectedNode.ForeColor != Color.Red)
                {
                    Console.WriteLine($"StartItem({selectedNode.Text})");

                    appStartItem = Data.Items.FirstOrDefault(i => i.Text == selectedNode.Text);

                    if (appStartItem != null)
                    {
                        HideApp();
                        var startInfo = new ProcessStartInfo()
                        {
                            FileName = appStartItem.Path,
                            Arguments = appStartItem.Arguments,
                            UseShellExecute = false,
                            CreateNoWindow = false
                        };
                        Process.Start(startInfo);
                        //Process.Start($"{appStartItem.Path} {startInfo}");
                    }

                }
            }
            catch (Exception ex)
            {
                var msg = $"Start-Pfad:{Environment.NewLine}" +
                          $"  {appStartItem.Path}{Environment.NewLine}" +
                          $"Fehler:{Environment.NewLine}" +
                          $"  {ex.Message}";
                MessageBox.Show(this, msg, "FEHLER beim Starten", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

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

        // CONTEXT-MENU
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("NEW ITEM: " + treeView1.SelectedNode.Text);

            preventHide = true;
            using (var dialog = new DialogStartItem(DialogMode.New))
            {
                Console.WriteLine("CREATE NEW ITEM...");
                dialog.SetCategories(Data.Categories);

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    var lastID = Data.Items.Max(i => i.ID);
                    dialog.StartItem.ID = ++lastID;

                    Data.Items.Add(dialog.StartItem);
                    SaveData();
                    LoadData();
                }
            }
            preventHide = false;
        }
        private void editToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("EDIT ITEM: " + treeView1.SelectedNode.Text);
            var id = (int)treeView1.SelectedNode.Tag;
            preventHide = true;
            using (var dialog = new DialogStartItem(DialogMode.Edit))
            {
                dialog.SetCategories(Data.Categories);
                dialog.StartItem = Data.Items.FirstOrDefault(i => i.ID == (int)treeView1.SelectedNode.Tag);
                dialog.Apply();

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    Console.WriteLine("SAVE ITEM...");
                    var item = Data.Items.FirstOrDefault(i => i.ID == dialog.StartItem.ID);
                    item.Text = dialog.StartItem.Text;
                    item.Path = dialog.StartItem.Path;
                    item.Arguments = dialog.StartItem.Arguments;
                    SaveData();
                    LoadData();
                }
            }
            preventHide = false;
        }
        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Console.WriteLine("DELETE ITEM: " + treeView1.SelectedNode.Text);
            var id = (int)treeView1.SelectedNode.Tag;
            preventHide = true;
            DialogResult dialogResult = MessageBoxEx.Show(this, "Eintrag wirklich löschen?", "Kurze Zwischenfrage...", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                Console.WriteLine("DELETE ITEM...");
                Data.Items.Remove(Data.Items.Find(i => i.ID == id));
                SaveData();
                LoadData();
            }
            preventHide = false;
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var hitTest = e.Node.TreeView.HitTest(e.Location);
            if (hitTest.Location == TreeViewHitTestLocations.PlusMinus)
                return;

            var selectedNode = ((TreeView)sender).SelectedNode = e.Node;

            if (e.Button == MouseButtons.Right)
            {
                UpdateContextMenu();
                contextMenu.Show(Cursor.Position);
                //e.Cancel = true;
                return;
            }
            else
            {
                StartItem();
            }

            //if (e.Node == treeView1.SelectedNode)
            //    StartItem();

            //if (e.Node.IsExpanded)
            //    e.Node.Collapse();
            //else
            //    e.Node.Expand();
        }


        // NOTIFY-ICON
        private void editAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenData();
        }
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                //if (this.WindowState == FormWindowState.Minimized)
                RestoreApp();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                RestoreApp();
        }

        private void FormMain_Deactivate(object sender, EventArgs e)
        {
            this.HideApp();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            //if (e.Button == MouseButtons.Right)
            //{
            //    e.Cancel = true;
            //    return;
            //}

            //if (treeView1.SelectedNode != null)
            //    StartItem();
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CloseApp();
        }

        private void reloadAppsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        #endregion

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

            //SaveData();

            Environment.Exit(0);
        }

        private void RestoreApp()
        {
            startBlocked = true;
            //this.TopMost = true;
            //this.ShowInTaskbar = true;
            this.BringToFront();
            this.Show();
            this.Activate();
            this.WindowState = FormWindowState.Normal;

            startBlocked = true;
            this.treeView1.Focus();
            treeView1.SelectedNode = null;
            treeView1.Update();
            startBlocked = false;
            //this.TopMost = false;
        }
        private void HideApp()
        {
            if (!preventHide)
                this.Hide();
        }


    }
}

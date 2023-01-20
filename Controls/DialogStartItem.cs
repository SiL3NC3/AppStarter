using AppStarter.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace AppStarter.Controls
{
    public enum DialogMode { New, Edit, Delete }

    public partial class DialogStartItem : Form
    {
        public DialogMode Mode { get; set; }
        public AppStartItem StartItem { get; set; }

        public DialogStartItem(DialogMode mode)
        {
            Mode = mode;
            InitializeComponent();
        }

        public void Apply()
        {
            switch (Mode)
            {
                case DialogMode.New:
                    StartItem = new AppStartItem();
                    break;
                case DialogMode.Edit:
                    cbCategory.Text = StartItem.Category;
                    tbText.Text = StartItem.Text;
                    tbPath.Text = StartItem.Path;
                    tbArguments.Text = StartItem.Arguments;
                    break;
                case DialogMode.Delete:
                    break;
                default:
                    break;
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrWhiteSpace(tbText.Text.Trim()))
            {
                MessageBoxEx.Show(this, "Text?!");
                return;
            }

            switch (Mode)
            {
                case DialogMode.New:
                    StartItem = new AppStartItem();
                    break;
                case DialogMode.Edit:
                    break;
                case DialogMode.Delete:
                    break;
                default:
                    break;
            }

            if (String.IsNullOrWhiteSpace(cbCategory.Text))
                StartItem.Category = "-ohne Kategorie-";
            else
                StartItem.Category = cbCategory.Text;
            StartItem.Text = tbText.Text;
            StartItem.Path = tbPath.Text;
            StartItem.Arguments = tbArguments.Text;

            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnLocate_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            if (Mode == DialogMode.Edit)
            {
                openFileDialog1.InitialDirectory = Path.GetDirectoryName(StartItem.Path);
                openFileDialog1.FileName = Path.GetFileName(StartItem.Path);
            }

            DialogResult result = openFileDialog1.ShowDialog(); // Show the dialog.
            if (result == DialogResult.OK) // Test result.
            {
                tbPath.Text = openFileDialog1.FileName;
                tbText.Text = Path.GetFileNameWithoutExtension(openFileDialog1.FileName);
            }
        }

        internal void SetCategories(List<string> categories)
        {
            cbCategory.Items.Clear();
            foreach (var cat in categories)
            {
                cbCategory.Items.Add(cat);
            }
        }
    }
}

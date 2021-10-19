using System;
using System.Linq;
using System.Windows.Forms;
using Diascan.Utils.IO;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Diascan.Agent.ClientApp
{
    public partial class InspectionsDirPath : Form
    {
        private bool closeForm;
        private string[] InspectionsDirs { get; set; }
        public string InspectionsPath { get; set; }

        public InspectionsDirPath(string[] inspectionsDirs)
        {
            InitializeComponent();
            closeForm = false;
            InspectionsDirs = inspectionsDirs;
        }

        private void btnInspectionsDirPath_Click(object sender, EventArgs e)
        {
            using (var ofd = new CommonOpenFileDialog(){IsFolderPicker = true})
            {
                if (ofd.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    InspectionsPath = ofd.FileName;
                    tbInspectionsDirPath.Text = ofd.FileName;
                }
            }
        }
    

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (Directory.Exists(InspectionsPath) || Directory.Exists(tbInspectionsDirPath.Text))
            {
                if (string.IsNullOrEmpty(InspectionsPath))
                    InspectionsPath = tbInspectionsDirPath.Text;

                if (InspectionsDirs.Any(path => path.Contains(InspectionsPath)))
                {
                    errorProvider.SetError(tbInspectionsDirPath, "Выбранный вами путь уже существует в списке инспекций!\r\nВыберите другой путь!");
                }
                else
                {
                    closeForm = true;
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            else
            {
                errorProvider.SetError(tbInspectionsDirPath, "Путь к инспекции не выбран!");
            }
        }

        private void btnСancel_Click(object sender, EventArgs e)
        {
            closeForm = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void InspectionsDirPath_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = e.CloseReason != CloseReason.WindowsShutDown && e.CloseReason != CloseReason.ApplicationExitCall;
            if (e.Cancel)
            {
                ShowInTaskbar = false;
                Visible = false;
                WindowState = FormWindowState.Minimized;
            }

            if (closeForm)
                e.Cancel = false;
        }
    }
}

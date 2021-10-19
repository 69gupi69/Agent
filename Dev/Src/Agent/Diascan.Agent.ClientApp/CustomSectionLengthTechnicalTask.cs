using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.ClientApp
{
    public partial class CustomSectionLengthTechnicalTask : Form
    {
        private bool                             closeForm;
        private List<DataLocation>               datasetLocation;
        private Dictionary<string, double> inspectionDirNameSectLengTechTask;
        private string[] customTextBoxNames;

        public CustomSectionLengthTechnicalTask(List<DataLocation> datasetLocation, Dictionary<string, double> inspectionDirNameSectLengTechTask)
        {
            this.inspectionDirNameSectLengTechTask = inspectionDirNameSectLengTechTask;
            this.datasetLocation = datasetLocation;
            customTextBoxNames = new string[datasetLocation.Count];
            InitializeComponent();
            CustomInitializeComponent();
        }


        private void CustomInitializeComponent()
        {
            var label1LocationY = 40; // точка от которой рисуется объект по оси Y
            var label1SizeH = 13; // ширина объекта на форме
            var label2LocationY = 61; // точка от которой рисуется объект по оси Y
            var label2SizeH = 13; // ширина объекта на форме
            var label2SizeW = 10; // высота объекта на форме
            var differenceLabel2SizeW = 0; // ширина объекта на форме
            var mtbPlotLengthTechSpecLocationY = 56; // точка от которой рисуется объект по оси Y
            var mtbPlotLengthTechSpecSizeH = 20; // высота объекта на форме
            var pictureBox1LocationY = 12; // точка от которой рисуется объект по оси Y
            var pictureBox1SizeH = 46; // точка от которой рисуется объект по оси X
            var formSizeW = 441; // ширина формы
            for (var i = 0; i < datasetLocation.Count; i++)
            {
                var mtbPlotLengthTechSpec = new MaskedTextBox();
                var label1                = new Label();
                var pictureBox1           = new PictureBox();
                var label2                = new Label();
                ((ISupportInitialize)(pictureBox1)).BeginInit();
                // 
                // label2
                // 
                label2.AutoSize = true;
                label2.Location = new Point(15, label2LocationY);
                label2.Name = $@"label_{datasetLocation[i].InspectionDirName}_{i}";
                label2.Size = new Size(label2SizeW, label2SizeH);
                label2.TabIndex = 36+ 4 + i;
                label2.Text = $@"{datasetLocation[i].InspectionDirName}";
                // 
                // pictureBox1
                // 
                pictureBox1.BackColor = Color.Transparent;
                pictureBox1.Image = Properties.Resources.information;
                pictureBox1.Location = new Point(15, pictureBox1LocationY);
                pictureBox1.Name = $@"pictureBox{i}";
                pictureBox1.Size = new Size(53, pictureBox1SizeH);
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
                pictureBox1.TabIndex = 37+4+i;
                pictureBox1.TabStop = false;
                this.Controls.Add(pictureBox1);
                this.Controls.Add(label2);

                // 
                // mtbPlotLengthTechSpec
                // ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)))
                mtbPlotLengthTechSpec.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
                mtbPlotLengthTechSpec.Location = new Point(label2.PreferredWidth+ label2.Location.X + 5/*отступ*/, mtbPlotLengthTechSpecLocationY);
                mtbPlotLengthTechSpec.Name = $@"mtbPlotLengthTechSpec{i}";
                mtbPlotLengthTechSpec.Size = new Size(80, mtbPlotLengthTechSpecSizeH);
                mtbPlotLengthTechSpec.TabIndex = 35 + 4 + i;
                mtbPlotLengthTechSpec.KeyPress += new KeyPressEventHandler(this.mtbPlotLengthTechSpec_KeyPress);

                customTextBoxNames[i] = mtbPlotLengthTechSpec.Name;
                // 
                // label1
                // ((AnchorStyles)((AnchorStyles.Top | AnchorStyles.Right)))
                label1.Anchor = ((AnchorStyles)(((AnchorStyles.Top | AnchorStyles.Left) | AnchorStyles.Right)));
                label1.AutoSize = true;
                label1.Location = new Point(label2.PreferredWidth + label2.Location.X + 5/*отступ*/, label1LocationY);
                label1.Name = $@"label{i}";
                label1.Size = new Size(132, label1SizeH);
                label1.TabIndex = 34 + 4 + i;
                label1.Text = "Длина участка по ТЗ (м)";

                this.Controls.Add(label1);
                this.Controls.Add(mtbPlotLengthTechSpec);

                differenceLabel2SizeW = this.Controls[$@"label_{datasetLocation[i].InspectionDirName}_{i}"].Size.Width - label2SizeW;

                var indent           = label2LocationY - (pictureBox1LocationY + pictureBox1SizeH);
                pictureBox1LocationY = pictureBox1LocationY + pictureBox1SizeH + label2SizeH + indent + pictureBox1LocationY;
                label2LocationY      = pictureBox1LocationY + pictureBox1SizeH + indent;

                indent                         = mtbPlotLengthTechSpecLocationY - (label1LocationY + label1SizeH);
                label1LocationY                = label1LocationY + label1SizeH + mtbPlotLengthTechSpecSizeH + indent + label1LocationY;
                mtbPlotLengthTechSpecLocationY = label1LocationY + label1SizeH + indent;

                ((ISupportInitialize)(pictureBox1)).EndInit();
            }

            formSizeW += differenceLabel2SizeW;

            var size = new Size(formSizeW, this.Controls[this.Controls.Count - 1].Location.Y + this.Controls[this.Controls.Count - 1].Height + 52);
            this.ClientSize = size;
            this.MinimumSize = size;
            size.Width = 3840;
            this.MaximumSize = size;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            for (var i = 0; i < customTextBoxNames.Length;i++)
            {
                if (this.Controls.ContainsKey(customTextBoxNames[i]))
                {
                    if (double.TryParse(this.Controls[customTextBoxNames[i]].Text, out var sectLengTechTask))
                    {
                        if (!inspectionDirNameSectLengTechTask.ContainsKey(datasetLocation[i].InspectionDirName))
                            inspectionDirNameSectLengTechTask.Add(datasetLocation[i].InspectionDirName,  sectLengTechTask);
                    }
                    else
                    {
                        errorProvider.SetError(this.Controls[customTextBoxNames[i]], "Недопустимое значение поля!");
                        return;
                    }
                }
                else
                {
                    closeForm = true;
                    DialogResult = DialogResult.Cancel;
                    Close();
                    return;
                }
            }
            closeForm = true;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnСancel_Click(object sender, EventArgs e)
        {
            closeForm = true;
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void CustomSectionLengthTechnicalTask_FormClosing(object sender, FormClosingEventArgs e)
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

        // Ввод длинны участка по ТЗ (м)
        private void mtbPlotLengthTechSpec_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Regex("[0-9]{1}").IsMatch(e.KeyChar.ToString()) ||
                e.KeyChar == (char)Keys.Enter ||
                e.KeyChar == (char)Keys.Back ||
                e.KeyChar == (char)Keys.Snapshot ||
                e.KeyChar == (char)Keys.Delete)
            {
                errorProvider.Clear();
                e.Handled = false;
            }
            else
            {
                errorProvider.SetError((MaskedTextBox)sender, "Недопустимый символ!");
                e.Handled = true;
            }
        }
    }
}

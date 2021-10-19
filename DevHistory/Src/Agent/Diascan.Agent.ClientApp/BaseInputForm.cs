using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Diascan.Agent.FuzzySearch;

namespace Diascan.Agent.ClientApp
{
    public partial class BaseInputForm : Form
    {
        protected bool   gateValve;
        protected double receptionChamber;
        protected double triggerChamber;
        protected double plotLengthTechSpec;
        protected double latitude;

        public BaseInputForm(string omniFilePath)
        {
            InitializeComponent();
            dtpDateWorkItem.Value = DateTime.Now;
            TopMost = true;
            BeforeOpenForm(omniFilePath);
        }


        public BaseInputForm()
        {
            InitializeComponent();
        }


        private void BeforeOpenForm(string omniFilePath)
        {
            var xOmni = XDocument.Load(omniFilePath);
            var responsible = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("RESPONSIBLE_ENGINEER")?.Value;
            var runIdentificator = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("RUN_IDENTIFICATOR")?.Value;
            var defectoskopNumber = xOmni.Element("RUN")?.Attribute("DEFECTOSKOP_IDENTIFICATION_NUMBER")?.Value;
            var dateWorkItem = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("TASK_DATE")?.Value;

            var latDegText = xOmni.Element("RUN")?.Element("NAV_CONSTANT_PARAMETERS")?.Attribute("LATDEG")?.Value;
            var latMinText = xOmni.Element("RUN")?.Element("NAV_CONSTANT_PARAMETERS")?.Attribute("LATMIN")?.Value;
            var latSecText = xOmni.Element("RUN")?.Element("NAV_CONSTANT_PARAMETERS")?.Attribute("LATSEC")?.Value;
            if (latDegText != null && latMinText != null && latSecText != null)
            {
                double.TryParse(latDegText, out var latDeg);
                double.TryParse(latMinText, out var latMin);
                double.TryParse(latSecText, out var latSec);
                latitude = latDeg + latMin / 60 + latSec / 3600;
            }

            if (!string.IsNullOrEmpty(defectoskopNumber))
                tbDefectoscope.Text = defectoskopNumber;

            if (!string.IsNullOrEmpty(responsible))
                tbResponsibleForPass.Text = responsible;

            if (!string.IsNullOrEmpty(runIdentificator))
                tbName.Text = runIdentificator.ToUpper();
        }


        //  Ввод имени прогона
        private void tbName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Regex("[0-9A-Za-z]{1}").IsMatch(e.KeyChar.ToString()) || e.KeyChar == (char)Keys.Back || e.KeyChar == (char)Keys.Enter)
            {
                if (new Regex("[a-z]{1}").IsMatch(e.KeyChar.ToString()))
                    e.KeyChar = char.ToUpper(e.KeyChar);
                errorProvider.Clear();
                e.Handled = false;
            }
            else
            {
                errorProvider.SetError(tbName, "Недопустимый символ!");
                e.Handled = true;
            }
        }


        //  Проверка заполнения всех полей
        private bool CheckAllFields()
        {
            return tbName.Text.Equals("") ||
                   tbDefectoscope.Text.Equals("") ||
                   tbResponsibleForPass.Text.Equals("") ||
                   tbEndOfTriggerChamber.Text.Equals("") ||
                   tbStartOfReceptionChamber.Text.Equals("") ||
                   mtbPlotLengthTechSpec.Text.Equals("") ||
                   CheckFields();
        }
        
        
        //  Закончить вовод данных
        private void btnOK_Click(object sender, EventArgs e)
        {
            errorProvider.Clear();
            if (CheckAllFields())
            {
                errorProvider.SetError(btnOK, "Не все поля заполнены!");
                return;
            }
            if (tbName.TextLength < 5)
            {
                errorProvider.SetError(tbName, "Код прогона не содержит символов!");
                return;
            }
            if (!double.TryParse(tbEndOfTriggerChamber.Text, out receptionChamber))
            {
                errorProvider.SetError(tbEndOfTriggerChamber, "Недопустимые символы!");
                return;
            }
            if (!double.TryParse(tbStartOfReceptionChamber.Text, out triggerChamber))
            {
                errorProvider.SetError(tbStartOfReceptionChamber, "Недопустимые символы!");
                return;
            }
            if (!double.TryParse(mtbPlotLengthTechSpec.Text, out plotLengthTechSpec))
            {
                errorProvider.SetError(mtbPlotLengthTechSpec, "Недопустимые символы!");
                return;
            }
            if(!CheckBeforeCloseForm()) return;
            DoBeforeCloseForm();
            Close();
        }


        protected virtual bool CheckFields()
        {
            return true;
        }

        protected virtual bool CheckBeforeCloseForm()
        {
            return true;
        }

        protected virtual void DoBeforeCloseForm()
        {
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
                errorProvider.SetError(mtbPlotLengthTechSpec, "Недопустимый символ!");
                e.Handled = true;
            }
        }

        private void cbGateValve_Click(object sender, EventArgs e)
        {
            gateValve = cbGateValve.Checked;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using System.IO;
using DiascanIO = Diascan.Utils.IO;
using DiCore.Lib.NDT.Types;
using DiCore.Lib.NDT.DiagnosticData;
using Diascan.Agent.TaskManager;
using Diascan.NDT.Enums;

namespace Diascan.Agent.ClientApp
{
    public partial class BaseInput : Form
    {
        /// <summary>
        /// Просечки
        /// </summary>
        protected bool               notch;
        protected bool               gateValve;
        protected double             receptionChamber;
        protected double             triggerChamber;
        protected double             latitude;
        protected string             defectoscope;
        protected List<int>          omniCarrierIds;
        protected List<DataType>     dataTypes;
        protected List<DataLocation> datasetLocation;

        protected Dictionary<string, double> inspectionDirNameSectLengTechTask;

        /// <summary>
        /// Базовые входные данные
        /// </summary>
        /// <param name="omniFilePath">Путь к omni файлу</param>
        /// <param name="datasetLocation">Пути к данным инспекций</param>
        /// <param name="notch">Просечки</param>
        public BaseInput(string omniFilePath, List<DataLocation> datasetLocation, bool notch)
        {
            InitializeComponent();
            defectoscope = String.Empty;
            dtpDateWorkItem.Value                  = DateTime.Now;
            this.inspectionDirNameSectLengTechTask = new Dictionary<string, double>();
            this.datasetLocation                   = datasetLocation;
            TopMost                                = true;
            this.notch                             = notch;
            BeforeOpenForm(omniFilePath);
        }


        public BaseInput()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Формирование пользовательских путей к инспекциям
        /// </summary>
        /// <param name="omniFilePath">Путь к omni файлу</param>
        /// <param name="xOmni">XML Документ omni</param>
        private void FormationСustomPathsToInspections(string omniFilePath, XDocument xOmni)
        {
            var inspectionsDirCount = xOmni.Element("RUN")?.Element("INSPECTION_PARTS")?.DescendantNodes().Count() ?? 0;
            var inspectionsDirs     = SearchInspectionsDir(omniFilePath);

            if (inspectionsDirs.Count() < inspectionsDirCount)
            {
                for (var i = inspectionsDirs.Count(); i < inspectionsDirCount; i++)
                {
                    using (var form = new InspectionsDirPath(inspectionsDirs))
                    {
                        var dialogResult = form.ShowDialog();
                        if (dialogResult == DialogResult.Cancel || string.IsNullOrEmpty(form.InspectionsPath))
                        {
                            this.DialogResult = DialogResult.Cancel;
                            form.Dispose();
                            return;
                        }
                        else
                        {
                            datasetLocation.Add(new DataLocation()
                            {
                                BaseFile           = Path.GetFileName(omniFilePath),
                                BaseDirectory      = Path.GetDirectoryName(omniFilePath),
                                InspectionDirName  = new DirectoryInfo(form.InspectionsPath).Name,
                                InspectionFullPath = form.InspectionsPath
                            });
                        }
                    }
                }

                if (datasetLocation.Count < inspectionsDirCount)
                {
                    datasetLocation.Add(new DataLocation()
                    {
                        BaseFile           = Path.GetFileName(omniFilePath),
                        BaseDirectory      = Path.GetDirectoryName(omniFilePath),
                        InspectionDirName  = new DirectoryInfo(inspectionsDirs[0]).Name,
                        InspectionFullPath = inspectionsDirs[0]
                    });
                }
            }
            else
            {
                foreach (var inspectionsDir in inspectionsDirs)
                {
                    datasetLocation.Add(new DataLocation()
                    {
                        BaseFile           = Path.GetFileName(omniFilePath),
                        BaseDirectory      = Path.GetDirectoryName(omniFilePath),
                        InspectionDirName  = new DirectoryInfo(inspectionsDir).Name,
                        InspectionFullPath = inspectionsDir
                    });
                }
            }
        }


        private void BeforeOpenForm(string omniFilePath)
        {
            var xOmni             = XDocument.Load(omniFilePath);
            defectoscope          = xOmni.Element("RUN")?.Attribute("DEVICE_TYPE")?.Value;
            var responsible       = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("RESPONSIBLE_ENGINEER")?.Value;
            var runIdentificator  = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("RUN_IDENTIFICATOR")?.Value;
            var defectoskopNumber = xOmni.Element("RUN")?.Attribute("DEFECTOSKOP_IDENTIFICATION_NUMBER")?.Value;
            var dateWorkItem      = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("BEGIN_TIME_DATE")?.Value;

            if (DateTime.TryParse(dateWorkItem, out var date))
                dtpDateWorkItem.Value = date;

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

            omniCarrierIds = TaskHelper.GetCarrierIds(omniFilePath);

            FormationСustomPathsToInspections(omniFilePath, xOmni);

            dataTypes = DiagnosticData.GetAvailableDataTypes(datasetLocation[0]).Where(q => q != DataType.MflT2 && q != DataType.MflT22).ToList();

            notch = dataTypes.Any(z => z.IsNotchDataType());

            if (datasetLocation.Count != 0)
            {
                using (var form =new CustomSectionLengthTechnicalTask(datasetLocation, inspectionDirNameSectLengTechTask))
                {
                    var dialogResult = form.ShowDialog();
                    if (dialogResult == DialogResult.Cancel || inspectionDirNameSectLengTechTask.Count != datasetLocation.Count)
                    {
                        this.DialogResult = DialogResult.Cancel;
                        form.Dispose();
                        return;
                    }

                    gateValve = form.cbGateValve.Checked;
                }
            }
            else
            {
                this.DialogResult = DialogResult.Cancel;
                throw new Exception($"Расположение каталогов инспекций не соответствует \"Формату данных ВИП\" или в \"{Path.GetFileName(omniFilePath)}\" файле отсутствует тег <INSPECTION_PARTS>");
            }
        }

        /// <summary>
        /// Поиск имен инспекционных каталогов
        /// </summary>
        /// <param name="omniFilePath">Полный путь к файлу описания прогона "*.omni"</param>
        private string[] SearchInspectionsDir(string omniFilePath)
        {
            var inspectionsDir = DiascanIO.Directory.EnumerateDirectories(DiascanIO.Path.GetDirectoryName(omniFilePath) )
                                .Where( s => s.Contains( $"{Path.GetFileNameWithoutExtension( DiascanIO.Path.GetFileName( omniFilePath ) ) }_") )
                                .ToArray();

            for (var i = 0; i < inspectionsDir.Length; i++)
            {
                var dirInfo = new DirectoryInfo(inspectionsDir[i]);
                if (dirInfo.Exists)
                {
                    var strArry = dirInfo.Name.Split(new string[]{ $"{Path.GetFileNameWithoutExtension(DiascanIO.Path.GetFileName(omniFilePath))}_" }, StringSplitOptions.RemoveEmptyEntries );
                    if (strArry.Length == 1)
                        if (new Regex("[0-9]{1}").IsMatch(strArry[0]) && !new Regex("[A-Za-z]{1}").IsMatch(strArry[0]))
                            continue;
                        else
                            inspectionsDir = inspectionsDir.Remove(i);
                    else
                        inspectionsDir = inspectionsDir.Remove(i);
                }
                else
                    inspectionsDir = inspectionsDir.Remove(i);
            }

            return inspectionsDir;
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
    }

    public static class StringArrey
    {
        public static string[] Remove(this string[] oldStrArrey, int removeItemOfIndex)
        {
            var newStrArrey = new string[oldStrArrey.Length - 1];

            for (int i = 0, j = 0; i < newStrArrey.Length; ++i, ++j)
            {
                if (j == removeItemOfIndex)
                    ++j;

                newStrArrey[i] = oldStrArrey[j];
            }

            return newStrArrey;
        }
    }
}

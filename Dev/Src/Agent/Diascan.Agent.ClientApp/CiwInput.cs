using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Diascan.Agent.Types;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.ClientApp
{
    public partial class CiwInput : BaseInput
    {
        private ReferenceInputData referenceInputData;

        public CiwInput(ReferenceInputData referenceInputData, string omniFilePath, List<DataLocation> datasetLocation) : base(omniFilePath, datasetLocation, referenceInputData.Notch)
        {
            if (base.DialogResult == DialogResult.Cancel)
            {
                InitializeComponent();
                this.Close();
            }
            else
            {
                InitializeComponent();
                this.referenceInputData = referenceInputData;
                ReadOmni(omniFilePath);
            }
        }

        private void ReadOmni(string omniFilePath)
        {
            var xOmni = XDocument.Load(omniFilePath);
            var contractor = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("CUSTOMER")?.Value;
            var pipeline = xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPELINE_NAME")?.Value;
            var route = xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPELINE_SITE")?.Value;
            var diameter = float.TryParse(xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPE_DIAMETR")?.Value, out var dim) ? dim * 25.4F : float.NaN;

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

            //************************* routes ***********************************
            lRoute.Text += route == null ? "" : $": {route}";
            tbRoute.Text = route;
            //************************* routes ***********************************
            //************************* pipelines ********************************
            lPipeline.Text += pipeline == null ? "" : $": {pipeline}";
            tbPipeline.Text = pipeline;
            //************************* pipelines ********************************
            //************************* customers ********************************
            lContractor.Text += contractor == null ? "" : $": {contractor}";
            tbContractor.Text = contractor;
            //************************* customers ********************************
            //************************* diameter ********************************
            var diameters = SharingEvents.SharingEvents.OnGetAllDiameters();
            lDiameter.Text += float.IsNaN(diameter) ? "" : $": {diameter}";
            cbDiameter.DataSource = new BindingSource(diameters, null);
            cbDiameter.DisplayMember = "Value";
            cbDiameter.SelectedIndex = -1;
            cbDiameter.Text = diameter.ToString();
            //************************* diameter ********************************
        }

        protected override bool CheckFields()
        {
            return tbContractor.Text.Equals("") ||
                   tbPipeline.Text.Equals("") ||
                   tbRoute.Text.Equals("");
        }

        protected override bool CheckBeforeCloseForm()
        {
            return true;
        }

        protected override void DoBeforeCloseForm()
        {
            double.TryParse(cbDiameter.Text, out var diameter);
            referenceInputData.WorkItemName                      = tbName.Text;
            referenceInputData.Contractor                        = new KeyValue<Guid, string>(Guid.Empty, tbContractor.Text);
            referenceInputData.PipeLine                          = new KeyValue<Guid, string>(Guid.Empty, tbPipeline.Text);
            referenceInputData.Route                             = new KeyValue<Guid, string>(Guid.Empty, tbRoute.Text);
            referenceInputData.DateWorkItem                      = dtpDateWorkItem.Value;
            referenceInputData.Diameter                          = diameter;
            referenceInputData.FlawDetector                      = tbDefectoscope.Text;
            referenceInputData.ResponsibleWorkItem               = tbResponsibleForPass.Text;
            referenceInputData.ReceptionChamber                  = receptionChamber;
            referenceInputData.TriggerChamber                    = triggerChamber;
            referenceInputData.RealLatitude                      = latitude;
            referenceInputData.GateValve                         = gateValve;
            referenceInputData.Local                             = true;
            referenceInputData.OmniCarrierIds                    = omniCarrierIds;
            referenceInputData.InspectionDirNameSectLengTechTask = inspectionDirNameSectLengTechTask;
            referenceInputData.Notch                             = base.notch;
            referenceInputData.DataTypes                         = base.dataTypes;
            referenceInputData.Defectoscope = defectoscope;
        }
    }
}

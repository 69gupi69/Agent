using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;
using Diascan.Agent.FuzzySearch;
using Diascan.Agent.SharingEvents;
using Diascan.Agent.Types;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.ClientApp
{
    public partial class DataInput : BaseInput
    {
        private readonly ReferenceInputData referenceInputData;
        private readonly DataModel[] dataModel;

        public DataInput(ReferenceInputData referenceInputData, string omniFilePath, List<DataLocation> datasetLocation) : base(omniFilePath, datasetLocation, referenceInputData.Notch)
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
                dataModel = SharingEvents.SharingEvents.OnGetAllDataModel();
                ResultFuzzySearch(omniFilePath);
            }
        }


        protected override bool CheckFields()
        {
            return cbContractor.Text.Equals("") ||
                   cbPipeline.Text.Equals("") ||
                   cbRoute.Text.Equals("");
        }


        protected override bool CheckBeforeCloseForm()
        {
            if (cbContractor.SelectedValue == null)
            {
                errorProvider.SetError(cbContractor, "Не выбрано значение!");
                return false;
            }
            if (cbPipeline.SelectedValue == null)
            {
                errorProvider.SetError(cbPipeline, "Не выбрано значение!");
                return false;
            }
            if (cbRoute.SelectedValue == null)
            {
                errorProvider.SetError(cbRoute, "Не выбрано значение!");
                return false;
            }
            return true;
        }

        
        protected override void DoBeforeCloseForm()
        {
            double.TryParse(tbDiameter.Text, out var diameter);

            var contractorKey   = ((KeyValuePair<Guid, string>)((BindingSource)cbContractor.DataSource).Current).Key;
            var contractorValue = ((KeyValuePair<Guid, string>)((BindingSource)cbContractor.DataSource).Current).Value;

            var pipeLineKey   = ((KeyValuePair<Guid, string>)((BindingSource)cbPipeline.DataSource).Current).Key;
            var pipeLineValue = ((KeyValuePair<Guid, string>)((BindingSource)cbPipeline.DataSource).Current).Value;

            var routeKey   = ((KeyValuePair<Guid, string>)((BindingSource)cbRoute.DataSource).Current).Key;
            var routeValue = ((KeyValuePair<Guid, string>)((BindingSource)cbRoute.DataSource).Current).Value;

            referenceInputData.WorkItemName                      = tbName.Text;
            referenceInputData.Contractor                        = new KeyValue<Guid, string>(contractorKey, contractorValue);
            referenceInputData.PipeLine                          = new KeyValue<Guid, string>(pipeLineKey, pipeLineValue);
            referenceInputData.Route                             = new KeyValue<Guid, string>(routeKey, routeValue);
            referenceInputData.DateWorkItem                      = dtpDateWorkItem.Value;
            referenceInputData.Diameter                          = diameter;
            referenceInputData.FlawDetector                      = tbDefectoscope.Text;
            referenceInputData.ResponsibleWorkItem               = tbResponsibleForPass.Text;
            referenceInputData.ReceptionChamber                  = receptionChamber;
            referenceInputData.TriggerChamber                    = triggerChamber;
            referenceInputData.RealLatitude                      = latitude;
            referenceInputData.GateValve                         = gateValve;
            referenceInputData.OmniCarrierIds                    = omniCarrierIds;
            referenceInputData.InspectionDirNameSectLengTechTask = inspectionDirNameSectLengTechTask;
            referenceInputData.Notch                             = base.notch;
            referenceInputData.DataTypes                         = base.dataTypes;
            referenceInputData.Defectoscope                      = defectoscope;
        }


        private (Guid Id, string Name, double Probability)[] Search(string dataOmni, Dictionary<Guid, string> analyzedData)
        {
            var search = new FuzzySearchTask();
            search.SetData(analyzedData);
            return search.Search(dataOmni);
        }
        

        /// <summary>
        /// 
        /// </summary>
        private void ResultFuzzySearch(string omniFilePath)
        {
            try
            {
                var xOmni = XDocument.Load(omniFilePath);
                var contractor = xOmni.Element("RUN")?.Element("CONTRACT")?.Attribute("CUSTOMER")?.Value;
                var pipeline = xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPELINE_NAME")?.Value;
                var route = xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPELINE_SITE")?.Value;
                var diameter = float.TryParse(xOmni.Element("RUN")?.Element("PIPELINE")?.Attribute("PIPE_DIAMETR")?.Value, out var dim) ? dim * 25.4F : float.NaN;
                
                //************************* routes ***********************************
                lRoute.Text += route == null ? "" : $": {route}";
                toolTip.SetToolTip(lRoute, route);
                var routes = GetAllRoutes();
                cbRoute.DataSource = new BindingSource(routes, null);
                cbRoute.DisplayMember = "Value";
                cbRoute.ValueMember = "Key";
                cbRoute.SelectedIndex = -1;
                //************************* routes ***********************************
                //************************* pipelines ********************************
                lPipeline.Text += pipeline == null ? "" : $": {pipeline}";
                toolTip.SetToolTip(lPipeline, pipeline);
                var pipelines = GetPipelines(Guid.Empty);
                cbPipeline.DataSource = new BindingSource(pipelines, null);
                cbPipeline.DisplayMember = "Value";
                cbPipeline.ValueMember = "Key";
                cbPipeline.SelectedIndex = -1;
                //************************* pipelines ********************************
                //************************* customers ********************************
                lCustomer.Text += contractor == null ? "" : $": {contractor}";
                toolTip.SetToolTip(lCustomer, contractor);
                var customers = GetCustomers(Guid.Empty);
                cbContractor.DataSource = new BindingSource(customers, null);
                cbContractor.DisplayMember = "Value";
                cbContractor.ValueMember = "Key";
                cbContractor.SelectedIndex = -1;
                //************************* customers ********************************
                //************************* diameter ********************************
                lDiameter.Text += float.IsNaN(diameter) ? "" : $": {diameter}";
                var res = GetDiameter(Guid.Empty) ?? 0;
                tbDiameter.Text = float.IsNaN(res)
                    ? (float.IsNaN(diameter) ? null : diameter.ToString())
                    : res.ToString();
                //************************* diameter ********************************
                //************************* Поиск ********************************
                if (route == null || pipeline == null) return;
                var routeSerch = Search(route, routes);
                var pipelineSearch = Search(pipeline, pipelines);
                var currentDataModel = new DataModel();
                var probability = double.MaxValue;
                foreach (var r in routeSerch)
                {
                    var dM = dataModel.First(q => q.Id == r.Id);
                    foreach (var pl in pipelineSearch)
                    {
                        var summProbability = r.Probability + pl.Probability;
                        if (dM.PipelineId == pl.Id && summProbability < probability)
                        {
                            currentDataModel = dM;
                            probability = summProbability;
                            break;
                        }
                    }
                }
                cbRoute.SelectedValue = currentDataModel.Id;
                cbPipeline.SelectedValue = currentDataModel.PipelineId;
                cbContractor.SelectedValue = currentDataModel.ContractorId;
                tbDiameter.Text = currentDataModel.DiameterMm.ToString();
                //************************* Поиск ********************************
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить заказчиков: " + ex.Message);
            }
        }

        //  Подгрузить участки
        private void GetRoutesOnPipeline(Guid pipelineGuid)
        {
            try
            {
                var routes = GetPdiRoutesOnPipelines(pipelineGuid);
                cbRoute.DataSource = new BindingSource(routes, null);
                cbRoute.DisplayMember = "Value";
                cbRoute.ValueMember = "Key";
                if (routes.Count != 1)
                    cbRoute.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить заказчиков: " + ex.Message);
            }
        }

        //  Подгрузить заказчисков
        private void SetCustomerValue(Guid routeGuid)
        {
            cbContractor.DataSource = new BindingSource(GetCustomers(routeGuid), null);
            cbContractor.DisplayMember = "Value";
            cbContractor.ValueMember = "Key";
        }

        //  Подгрузить трубы
        private void SetPipelineValue(Guid routeGuid)
        {
            cbPipeline.DataSource = new BindingSource(GetPipelines(routeGuid), null);
            cbPipeline.DisplayMember = "Value";
            cbPipeline.ValueMember = "Key";
        }

        private void SetDiameterValue(Guid routeGuid)
        {
            tbDiameter.Text = GetDiameter((Guid)cbRoute.SelectedValue).ToString();
        }

        //  Подгрузить диаметр
        private void cbPipeline_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cbPipeline.SelectedValue != null)
            {
                GetRoutesOnPipeline((Guid)cbPipeline.SelectedValue);
                if (cbRoute.SelectedItem == null) return;
                SetCustomerValue((Guid)cbRoute.SelectedValue);
                SetDiameterValue((Guid)cbRoute.SelectedValue);
            }
        }

        private void cbRoute_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cbRoute.SelectedValue != null)
            {
                SetPipelineValue((Guid)cbRoute.SelectedValue);
                SetCustomerValue((Guid)cbRoute.SelectedValue);
                SetDiameterValue((Guid)cbRoute.SelectedValue);
            }
        }

        private void tbName_Click(object sender, EventArgs e)
        {
            try
            {
                InputLanguage.CurrentInputLanguage = InputLanguage.FromCulture(new System.Globalization.CultureInfo("en-US"));
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не удалось сменить язык: " + ex.Message);
            }
        }

        private void tbRoute_TextChanged(object sender, EventArgs e)
        {
            var routes = GetRoutesSearch(tbRoute.Text);
            var pipelines = GetPipelines(routes.Keys.ToArray());
            var customers = GetCustomers(routes.Keys.ToArray());
            cbRoute.DataSource = new BindingSource(routes, null);
            cbPipeline.DataSource = new BindingSource(pipelines, null);
            cbContractor.DataSource = new BindingSource(customers, null);
            cbRoute.DisplayMember = cbPipeline.DisplayMember = cbContractor.DisplayMember = "Value";
            cbRoute.ValueMember = cbPipeline.ValueMember = cbContractor.ValueMember = "Key";
            cbRoute.SelectedIndex = cbPipeline.SelectedIndex = cbContractor.SelectedIndex = -1;
        }

        public Dictionary<Guid, string> GetAllRoutes()
        {
            return dataModel.OrderBy(q => q.RouteName).ToDictionary(guid => guid.Id, name => name.RouteName);
        }

        public Dictionary<Guid, string> GetRoutesSearch(string routeSearch)
        {
            if (string.IsNullOrEmpty(routeSearch))
                return GetAllRoutes();
            return dataModel.Where(q => q.RouteName.IndexOf(routeSearch, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(q => q.RouteName).ToDictionary(guid => guid.Id, name => name.RouteName);
        }

        public Dictionary<Guid, string> GetPdiRoutesOnPipelines(Guid pipelineGuid)
        {
            return dataModel.Where(q => q.PipelineId == pipelineGuid).OrderBy(q => q.RouteName).ToDictionary(k => k.Id, v => v.RouteName);
        }

        private IEnumerable<DataModel> GetDirectoryDataModel(Guid[] routesGuid)
        {
            foreach (var routeGuid in routesGuid)
                yield return dataModel.First(q => q.Id == routeGuid);
        }

        public Dictionary<Guid, string> GetPipelines(params Guid[] routesGuid)
        {
            if (routesGuid.Length == 0) return new Dictionary<Guid, string>();
            var pipelines = routesGuid.FirstOrDefault() == Guid.Empty ? dataModel.OrderBy(q => q.PipelineName) : GetDirectoryDataModel(routesGuid).OrderBy(q => q.PipelineName);
            var pipelinesDictionary = new Dictionary<Guid, string>();
            foreach (var pipeline in pipelines)
            {
                if (!pipelinesDictionary.ContainsKey(pipeline.PipelineId))
                    pipelinesDictionary.Add(pipeline.PipelineId, pipeline.PipelineName);
            }
            return pipelinesDictionary;
        }

        public Dictionary<Guid, string> GetCustomers(params Guid[] routesGuid)
        {
            if (routesGuid.Length == 0) return new Dictionary<Guid, string>();
            var contractors = routesGuid.First() == Guid.Empty ? dataModel.OrderBy(q => q.ContractorName) : GetDirectoryDataModel(routesGuid).OrderBy(q => q.ContractorName);
            var contractorDictionary = new Dictionary<Guid, string>();
            foreach (var contractor in contractors)
            {
                if (!contractorDictionary.ContainsKey(contractor.ContractorId))
                    contractorDictionary.Add(contractor.ContractorId, contractor.ContractorName);
            }
            return contractorDictionary;
        }

        private float? GetDiameter(Guid routeGuid)
        {
            if (routeGuid == Guid.Empty) return float.NaN;
            return dataModel.First(q => q.Id == routeGuid).DiameterMm ?? float.NaN;
        }

        private void cbRoute_TextChanged(object sender, EventArgs e)
        {
            toolTip.SetToolTip(cbRoute, cbRoute.Text);
        }

        private void cbPipeline_TextChanged(object sender, EventArgs e)
        {
            toolTip.SetToolTip(cbPipeline, cbPipeline.Text);
        }

        private void cbContractor_TextChanged(object sender, EventArgs e)
        {
            toolTip.SetToolTip(cbContractor, cbPipeline.Text);
        }
    }
}

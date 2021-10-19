using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml.Linq;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.ClientApp
{
    public partial class DataInputForm : Form
    {
        private ReferenceInputData referenceInputData;
        private string omniFilePath;
        private double latitude;

        public DataInputForm(ReferenceInputData referenceInputData, string omniFilePath)
        {
            InitializeComponent();
            this.referenceInputData = referenceInputData;
            this.omniFilePath = omniFilePath;

            cbСontractor.KeyPress += (sender, e) => e.Handled = true;
            cbPipeline.KeyPress += (sender, e) => e.Handled = true;
            cbRoute.KeyPress += (sender, e) => e.Handled = true;

            dtpDateWorkItem.Value = DateTime.Now;
            TopMost = true;

            ResultFuzzySearch();
        }

        //  Проверка заполнения всех полей
        private bool CheckAllFields()
        {
            return tbName.Text.Equals("") ||
                   tbDefectoscope.Text.Equals("") ||
                   cbСontractor.Text.Equals("") ||
                   cbPipeline.Text.Equals("") ||
                   cbRoute.Text.Equals("") ||
                   tbResponsibleForPass.Text.Equals("") ||
                   tbEndOfTriggerChamber.Text.Equals("") ||
                   tbStartOfReceptionChamber.Text.Equals("");
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
            if (cbСontractor.SelectedValue == null)
            {
                errorProvider.SetError(cbСontractor, "Не выбрано значение!");
                return;
            }
            if (cbPipeline.SelectedValue == null)
            {
                errorProvider.SetError(cbPipeline, "Не выбрано значение!");
                return;
            }
            if (cbRoute.SelectedValue == null)
            {
                errorProvider.SetError(cbRoute, "Не выбрано значение!");
                return;
            }
            if (tbName.TextLength != 5)
            {
                errorProvider.SetError(tbName, "Код прогона должен содержать 5 символов!");
                return;
            }
            if ( !double.TryParse( tbEndOfTriggerChamber.Text, out var endOfTriggerChamber ) )
            {
                errorProvider.SetError( tbEndOfTriggerChamber, "Недопустимые символы!" );
                return;
            }
            if ( !double.TryParse( tbStartOfReceptionChamber.Text, out var startOfReceptionChamber ) )
            {
                errorProvider.SetError( tbStartOfReceptionChamber, "Недопустимые символы!" );
                return;
            }
            if (!double.TryParse(tbEndOfTriggerChamber.Text, out var receptionChamber))
            {
                errorProvider.SetError(tbEndOfTriggerChamber, "Недопустимые символы!");
                return;
            }
            if (!double.TryParse(tbStartOfReceptionChamber.Text, out var triggerChamber))
            {
                errorProvider.SetError(tbStartOfReceptionChamber, "Недопустимые символы!");
                return;
            }

            double.TryParse(tbDiameter.Text, out var diameter);

            var contractorKey = ((KeyValuePair<Guid, string>)((BindingSource) cbСontractor.DataSource).Current).Key;
            var contractorValue = ((KeyValuePair<Guid, string>)((BindingSource)cbСontractor.DataSource).Current).Value;

            var pipeLineKey = ((KeyValuePair<Guid, string>)((BindingSource)cbPipeline.DataSource).Current).Key;
            var pipeLineValue = ((KeyValuePair<Guid, string>)((BindingSource)cbPipeline.DataSource).Current).Value;

            var routeKey = ((KeyValuePair<Guid, string>)((BindingSource)cbRoute.DataSource).Current).Key;
            var routeValue = ((KeyValuePair<Guid, string>)((BindingSource)cbRoute.DataSource).Current).Value;

            referenceInputData.WorkItemName = tbName.Text;
            referenceInputData.Contractor = new KeyValue<Guid, string>(contractorKey, contractorValue);
            referenceInputData.PipeLine = new KeyValue<Guid, string>(pipeLineKey, pipeLineValue);
            referenceInputData.Route = new KeyValue<Guid, string>(routeKey, routeValue);
            referenceInputData.DateWorkItem = dtpDateWorkItem.Value.ToShortDateString();
            referenceInputData.Diameter = diameter;
            referenceInputData.FlawDetector = tbDefectoscope.Text;
            referenceInputData.ResponsibleWorkItem = tbResponsibleForPass.Text;
            referenceInputData.ReceptionChamber = receptionChamber;
            referenceInputData.TriggerChamber = triggerChamber;
            referenceInputData.RealLatitude = latitude;

            Close();
        }
        
        //  Ввод имени прогона
        private void tbName_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (new Regex("[0-9A-Za-z]{1}").IsMatch(e.KeyChar.ToString()) || e.KeyChar == '\b'/*BackSpase*/)
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
            if (tbName.Text.Length > 4 && e.KeyChar != '\b')
                e.Handled = true;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private KeyValuePair<Guid, string> ? FuzzySearchNew( string dataOMNI, Dictionary<Guid, string> analyzedData  )
        {
            var datas = new List<Tuple<string, string, Guid>>();

            foreach ( var item in analyzedData )
                datas.Add( new Tuple<string, string, Guid>( item.Value, "", item.Key ) );

            var search = new DistanceAlferov();
            search.SetData( datas );
            var resultSearch = search.Search( targetStr: dataOMNI );

            KeyValuePair<Guid, string>? found = null;
            var rating = double.MaxValue;

            foreach( var itemSearch in resultSearch)
                if(itemSearch.Item4 <= rating )
                {
                    found = new KeyValuePair<Guid, string>( key: itemSearch.Item3, value: itemSearch.Item1 );
                    rating = itemSearch.Item4;
                }

            return found ;
        }


        private DateTime SetDateTime(string dateWorkItem)
        {
            try
            {
                return DateTime.ParseExact(dateWorkItem, "HH:mm:ss:fff,dd.MM.yyyy", System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                return DateTime.Now;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private async void ResultFuzzySearch()
        {
            try
            {
                var xOmni = XDocument.Load( omniFilePath );
                var contractor       = xOmni.Element( "RUN" )?.Element( "CONTRACT" )?.Attribute( "CUSTOMER" )?.Value;
                var pipeline         = xOmni.Element( "RUN" )?.Element( "PIPELINE" )?.Attribute( "PIPELINE_NAME" )?.Value;
                var route            = xOmni.Element( "RUN" )?.Element( "PIPELINE" )?.Attribute( "PIPELINE_SITE" )?.Value;
                var diameter         = float.TryParse( xOmni.Element( "RUN" )?.Element( "PIPELINE" )?.Attribute( "PIPE_DIAMETR" )?.Value, out var dim )? dim * 25.4F : float.NaN ;
                var responsible      = xOmni.Element( "RUN" )?.Element( "CONTRACT" )?.Attribute( "RESPONSIBLE_ENGINEER" )?.Value;
                var runIdentificator = xOmni.Element( "RUN" )?.Element( "CONTRACT" )?.Attribute( "RUN_IDENTIFICATOR" )?.Value;
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
                    latitude = latDeg + latMin/60 + latSec/3600;
                }

                if (defectoskopNumber != null)
                    tbDefectoscope.Text = defectoskopNumber;
                //if (defectoskopNumber != null)
                //    dtpDateWorkItem.Value = SetDateTime(dateWorkItem);

                //************************* customers ********************************
                var customers = await SharingEvents.SharingEvents.OnGetPdiCustomers();
                cbСontractor.DataSource = new BindingSource( customers, null );
                cbСontractor.DisplayMember = "Value";
                cbСontractor.ValueMember   = "Key";


                if( !string.IsNullOrEmpty(contractor) )
                {
                    var found = FuzzySearchNew( contractor, customers );
                    if ( !string.IsNullOrEmpty( found?.Value ) )
                        cbСontractor.SelectedItem = found.Value /*cbСontractor.Items.IndexOf( found ) по MSDN одно и тоже*/;
                }
                //************************* customers ********************************

                //************************* pipelines ********************************
                if( cbСontractor.SelectedValue == null )
                    return;

                var pipelines = await SharingEvents.SharingEvents.OnGetPdiPipelines((Guid)cbСontractor.SelectedValue);
                cbPipeline.DataSource = new BindingSource( pipelines, null );
                cbPipeline.DisplayMember = "Value";
                cbPipeline.ValueMember = "Key";

                if( !string.IsNullOrEmpty( pipeline ) )
                {
                    var found = FuzzySearchNew( pipeline, pipelines );
                    if( !string.IsNullOrEmpty( found?.Value ) )
                        cbPipeline.SelectedItem = found.Value /*cbСontractor.Items.IndexOf( found ) по MSDN одно и тоже*/;
                }
                //************************* pipelines ********************************

                //************************* routes ********************************
                var routes = await SharingEvents.SharingEvents.OnGetPdiRoutes((Guid)cbСontractor.SelectedValue, (Guid)cbPipeline.SelectedValue);
                cbRoute.DataSource = new BindingSource( routes, null );
                cbRoute.DisplayMember = "Value";
                cbRoute.ValueMember = "Key";

                if( !string.IsNullOrEmpty( route ) )
                {
                    var found = FuzzySearchNew( route, routes );
                    if( !string.IsNullOrEmpty( found?.Value ) )
                        cbRoute.SelectedItem = found.Value /*cbСontractor.Items.IndexOf( found ) по MSDN одно и тоже*/;
                }
                //************************* routes ********************************

                var res = await SharingEvents.SharingEvents.OnGetPdiDiameter((Guid)cbRoute.SelectedValue);
                if ( float.IsNaN( res ) )
                    if( float.IsNaN( diameter ) )
                        MessageBox.Show( @"Ошибка, не удалось получить диаметр", "ERROR", MessageBoxButtons.OK,MessageBoxIcon.Error );
                    else
                        tbDiameter.Text = diameter.ToString();
                else
                    tbDiameter.Text = res.ToString();


                if( !string.IsNullOrEmpty( responsible ) )
                    tbResponsibleForPass.Text = responsible;

                if ( !string.IsNullOrEmpty( runIdentificator ) )
                    tbName.Text = runIdentificator.ToUpper();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить заказчиков: " + ex.Message);
            }
        }

        //  Подгрузить заказчисков
        //private async void GetCustomers()
        //{
        //    try
        //    {
        //        cbСontractor.DataSource = new BindingSource(await connect.GetPDICustomers(), null);
        //        cbСontractor.DisplayMember = "Value";
        //        cbСontractor.ValueMember = "Key";

        //        if(cbСontractor.SelectedValue == null) return;
        //        cbPipeline.DataSource = new BindingSource(await connect.GetPDIPipelines((Guid)cbСontractor.SelectedValue), null);
        //        cbPipeline.DisplayMember = "Value";
        //        cbPipeline.ValueMember = "Key";

        //        cbRoute.DataSource = new BindingSource(await connect.GetPDIRoutes((Guid)cbСontractor.SelectedValue, 
        //                                                                      (Guid)cbPipeline.SelectedValue), null);
        //        cbRoute.DisplayMember = "Value";
        //        cbRoute.ValueMember = "Key";

        //        tbDiameter.Text = ( await connect.GetPDIDiameter( (Guid)cbRoute.SelectedValue ) ).ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show(@"Ошибка, не удалось получить заказчиков: " + ex.Message);
        //    }
        //}

        //  Подгрузить трубы
        private async void GetPipelines(Guid customerGuid)
        {
            try
            {
                cbPipeline.DataSource = new BindingSource(await SharingEvents.SharingEvents.OnGetPdiPipelines(customerGuid), null);
                cbPipeline.DisplayMember = "Value";
                cbPipeline.ValueMember = "Key";

                cbRoute.DataSource = new BindingSource(await SharingEvents.SharingEvents.OnGetPdiRoutes((Guid)cbСontractor.SelectedValue,
                    (Guid)cbPipeline.SelectedValue), null);
                cbRoute.DisplayMember = "Value";
                cbRoute.ValueMember = "Key";

                tbDiameter.Text = (await SharingEvents.SharingEvents.OnGetPdiDiameter((Guid)cbRoute.SelectedValue)).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить трубные секции: " + ex.Message);
            }
        }
        
        //  Подгрузить участки
        private async void GetSegments(Guid customerGuid, Guid pipelineGuid)
        {
            try
            {
                cbRoute.DataSource = new BindingSource(await SharingEvents.SharingEvents.OnGetPdiRoutes(customerGuid, pipelineGuid), null);
                cbRoute.DisplayMember = "Value";
                cbRoute.ValueMember = "Key";
                tbDiameter.Text = (await SharingEvents.SharingEvents.OnGetPdiDiameter((Guid)cbRoute.SelectedValue)).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить участки: " + ex.Message);
            }
        }

        //  Подгрузить диаметр
        private async void GetDiameter( Guid routeGuid )
        {
            try
            {
                tbDiameter.Text = (await SharingEvents.SharingEvents.OnGetPdiDiameter(routeGuid)).ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Ошибка, не удалось получить диаметр: " + ex.Message);
            }
        }

        private void cbCustomer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if(cbСontractor.SelectedValue != null)
                GetPipelines((Guid)cbСontractor.SelectedValue);
        }

        private void cbPipeline_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (cbСontractor.SelectedValue != null && cbPipeline.SelectedValue != null)
                GetSegments((Guid)cbСontractor.SelectedValue, (Guid)cbPipeline.SelectedValue);
        }

        private void cbRoute_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if ( cbRoute.SelectedValue != null)
                GetDiameter( (Guid)cbRoute.SelectedValue );
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
    }
}

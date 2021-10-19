using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Diascan.Agent.ClientApp
{
    public partial class InformationOnProgram : Form
    {
        public InformationOnProgram()
        {
            InitializeComponent();
            
            var listInfo = SharingEvents.SharingEvents.OnGetInfo();

            if (listInfo != null)
            {
                var res = FileVersionInfo.GetVersionInfo(listInfo[0].FullName).ProductVersion;        // fileInfoClientApp получение версию ипрограммы

                var versionProgram                = string.IsNullOrEmpty(res) ? string.Empty : res;
                var dateVersionProgram            = listInfo[0].CreationTime;
                var dateUpdateDirectory           = listInfo[1].LastWriteTime;                        // fileInfoDirectoryDataModel Дата обновления справочника
                var impendingUpdatesDirectoryDate = listInfo[1].LastWriteTime + TimeSpan.FromDays(1); // fileInfoDirectoryDataModel Следующая проверка обновления справочника
                var dateUpdateCarrierData         = listInfo[2].LastWriteTime;                        // fileInfoCarrierData Дата обновления справочника
                var impendingUpdatesCarrierData   = listInfo[2].LastWriteTime + TimeSpan.FromDays(1); // fileInfoCarrierData Следующая проверка обновления справочника
                var dateUpdateCarriers            = listInfo[3].LastWriteTime;                        // fileInfoCarriers Дата обновления справочника
                var impendingUpdatesCarriers      = listInfo[3].LastWriteTime + TimeSpan.FromDays(1); // fileInfoCarriers Следующая проверка обновления справочника

                labelVersion.Text = $@"ПО ""ПДИ"" версия: {versionProgram} от {dateVersionProgram:d};";
                labelDateUpdateDirectory.Text = $@"Дата обновления справочника: {dateUpdateDirectory:d};";
                labelIimpendingUpdatesDirectoryDate.Text = $@"Следующая проверка обновления справочника: {impendingUpdatesDirectoryDate:d} в {impendingUpdatesDirectoryDate.Hour}:{impendingUpdatesDirectoryDate.Minute};";
                labelDateUpdateCarrierData .Text = $@"Дата обновления ""Описания носителей датчиков"": {dateUpdateCarrierData:d};";
                labelIimpendingUpdatesCarrierData.Text = $@"Следующая проверка обновления ""Описания носителей датчиков"": {impendingUpdatesCarrierData:d} в {impendingUpdatesCarrierData.Hour}:{impendingUpdatesCarrierData.Minute};";
                labelDateUpdateCarriers.Text = $@"Дата обновления ""Описания носителей датчиков"": {dateUpdateCarriers:d};";
                labelIimpendingUpdatesCarriers.Text = $@"Следующая проверка обновления ""Описания носителей датчиков"": {impendingUpdatesCarriers:d} в {impendingUpdatesCarriers.Hour}:{impendingUpdatesCarriers.Minute};";
                labelСopyrightCompany.Text = $@"© АО ""Транснефть-Диаскан"", {DateTime.Now.Year}. Все права защищены.";
            }
        }
    }
}

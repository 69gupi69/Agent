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
            
            var listInfo = SharingEvents.SharingEvents.OnGetInfo().Result;

            if ( listInfo != null )
            {
                var res = FileVersionInfo.GetVersionInfo( listInfo[0].FullName ).ProductVersion;        /// fileInfoClientApp получение версиипрограммы

                var versionProgram                = string.IsNullOrEmpty( res ) ? string.Empty : res;
                var dateVersionProgram            = listInfo[0].CreationTime;
                var dateUpdateDirectory           = listInfo[1].LastWriteTime;                          /// fileInfoDirectoryDataModel Дата обновления справочника
                var impendingUpdatesDirectoryDate = listInfo[1].LastWriteTime + TimeSpan.FromDays( 1 ); /// fileInfoDirectoryDataModel Следующая проверка обновления справочника

                labelVersion.Text                        = $@"ПО ""ПДИ"" версия {versionProgram} от {dateVersionProgram.Day}.{dateVersionProgram.Month}.{dateVersionProgram.Year};";
                labelDateUpdateDirectory.Text            = $@"Дата обновления справочника {dateUpdateDirectory.Day}.{dateUpdateDirectory.Month}.{dateUpdateDirectory.Year};";
                labelIimpendingUpdatesDirectoryDate.Text = $@"Следующая проверка обновления справочника {impendingUpdatesDirectoryDate.Day}.{impendingUpdatesDirectoryDate.Month}.{impendingUpdatesDirectoryDate.Year} в {impendingUpdatesDirectoryDate.Hour}:{impendingUpdatesDirectoryDate.Minute};";
                labelСopyrightCompany.Text               = $@"© АО ""Транснефть-Диаскан"", {DateTime.Now.Year}. Все права защищены.";
            }
        }
    }
}

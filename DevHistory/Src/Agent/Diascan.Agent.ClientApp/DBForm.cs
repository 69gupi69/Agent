using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diascan.Agent.Types;

namespace Diascan.Agent.ClientApp
{
    public partial class DBForm : Form
    {
        private bool closeForm;
        private BindingList<HeaderCalculation> headersCalculation;

        public DBForm()
        {
            InitializeComponent();
            PerformanceManagementToolStripMenuItem.Visible = false;
            UpdateButtons();
            InitEvents();
        }

        private void InitEvents()
        {
            SharingEvents.SharingEvents.WarnMessage += WarnMessage;
            SharingEvents.SharingEvents.ErrorMessage += ErrorMessage;
            SharingEvents.SharingEvents.DeleteRow += DeleteRow;
        }

        private async void DBForm_Shown(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            headersCalculation = await SharingEvents.SharingEvents.OnFormLoad();
            dgvDataBase.DataSource = headersCalculation;
            headersCalculation.ListChanged += DataSourceChanged;
            UpdateButtons();
            Cursor = Cursors.Default;
        }

        private void DataSourceChanged(object sender, ListChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void UpdateButtons()
        {
            if (headersCalculation == null || headersCalculation.Count <= 0)
            {
                btnCanceled.Enabled = false;
                btnExportToExcel.Enabled = false;
            }
            else
            {
                btnCanceled.Enabled = true;
                btnExportToExcel.Enabled = true;
            }
        }


        // Отменить расчет
        private void Canceled_Click(object sender, EventArgs e)
        {
            DeleteRow();
        }

        private void Canseled(Guid globalId)
        {
            try
            {
                SharingEvents.SharingEvents.OnCanseled(globalId);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не удалось удалить запись. Ошибка: " + ex.Message);
            }
        }

        // Сохранить в Excel
        private void btnExportToExcel_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvDataBase.SelectedRows;
            foreach (DataGridViewRow row in selectedRows)
            {
                var headerCalculation = (HeaderCalculation)row.DataBoundItem;
                ExportToJsonAndExcel(headerCalculation.Id, headerCalculation.Path);
                return;
            }
        }

        //  экспорт в Jeson
        private void ExportToJsonAndExcel(int id, string path)
        {
            using (var saveFile = new SaveFileDialog())
            {
                saveFile.Filter = "Text files(*.Json, *.xls)|*.*";
                saveFile.InitialDirectory = path;
                saveFile.RestoreDirectory = true;
                saveFile.FileName =
                    $"{new DirectoryInfo(path).Name}_{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}";

                if (saveFile.ShowDialog() == DialogResult.Cancel)
                    return;

                try
                {
                    SharingEvents.SharingEvents.OnExportToJeson(id, saveFile.FileName + ".Json");
                    SharingEvents.SharingEvents.OnExportToExcel(id, saveFile.FileName + ".xls");
                    Process.Start(saveFile.FileName + ".xls");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Не удалось создать файл отчета. Ошибка: " + ex.Message);
                }
            }
        }


        private void ExportToExcel(int id, string path)
        {
            using (var saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text files(*.xls)|*.xls";
                saveFileDialog.InitialDirectory = path;
                saveFileDialog.FileName =
                    $"{new DirectoryInfo(path).Name}_{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}";

                if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                    return;

                try
                {
                    SharingEvents.SharingEvents.OnExportToExcel(id, saveFileDialog.FileName);
                    Process.Start(saveFileDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Не удалось создать файл отчета. Ошибка: " + ex.Message);
                }
            }
        }

        //  экспорт в Jeson
        private void ExportToJson(int id, string path)
        {
            using (var saveFileJsonDialog = new SaveFileDialog())
            {
                saveFileJsonDialog.Filter = "Text files(*.Json)|*.Json";
                saveFileJsonDialog.InitialDirectory = path;
                saveFileJsonDialog.FileName =
                    $"{new DirectoryInfo(path).Name}_{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}";

                if (saveFileJsonDialog.ShowDialog() == DialogResult.Cancel)
                    return;

                try
                {
                    SharingEvents.SharingEvents.OnExportToJeson(id, saveFileJsonDialog.FileName);
                    Process.Start(saveFileJsonDialog.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Не удалось создать файл отчета. Ошибка: " + ex.Message);
                }
            }
        }

        // Закрытие формы
        private void DBForm_FormClosing(object sender, FormClosingEventArgs e)
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

        //  Открыть форму по ярлыку в трее
        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowInTaskbar = true;
            Visible = true;
            WindowState = FormWindowState.Normal;
        }

        // Закрытие формы в трее
        private void CloseMenu_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            closeForm = true;
            Close();
        }

        //  открыть папку с прогоном
        private void dgvDataBase_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || (string)dgvDataBase.Rows[e.RowIndex].Cells[2].Value == "") return;
            var pathFolder = (string)dgvDataBase.Rows[e.RowIndex].Cells[2].Value;
            if (Directory.Exists(pathFolder))
                Process.Start("explorer.exe", pathFolder);
        }

        //  добавить новый расчет
        private void btnAddNewCalculation_Click(object sender, EventArgs e)
        {
            ShowInputForm(false);
        }

        private void bCiw_Click(object sender, EventArgs e)
        {
            ShowInputForm(true);
        }


        private void ShowInputForm(bool local)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text files(*.omni)|*.omni";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                openFileDialog.FileName = "";
                if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                    return;

                var referenceInputData = new ReferenceInputData();
                try
                {
                    Form form;
                    if (local)
                        form = new CiwInputForm(referenceInputData, openFileDialog.FileName);
                    else
                        form = new DataInputForm(referenceInputData, openFileDialog.FileName);
                    form.ShowDialog();
                    form.Dispose();

                    if (referenceInputData.WorkItemName != null)
                        SharingEvents.SharingEvents.OnSentPdi(openFileDialog.FileName, referenceInputData);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка добавления СМР расчета: " + ex.Message);
                }
            }
        }

        //  сообщения из сервиса об предупреждение
        public void WarnMessage(string message)
        {
            Task.Run(() =>
            {
                MessageBox.Show(message, "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            });
        }

        //  сообщения из сервиса об ошибке
        public void ErrorMessage(string message)
        {
            Task.Run(() =>
            {
                MessageBox.Show(message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.ServiceNotification);
            });
        }


        //  удалить расчет из таблицы на форме
        public void DeleteRow(string message, Guid globalId)
        {
            Task.Run(() =>{ Canseled(globalId); });
        }

        private void DeleteRow()
        {
            foreach (DataGridViewRow row in dgvDataBase.SelectedRows)
            {
                var headerCalculation = (HeaderCalculation)row.DataBoundItem;
                Canseled(headerCalculation.GlobalId);
            }
        }

        //  форма изменения адреса сервера
        private void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new AddressesForm())
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия формы: " + ex.Message);
            }
        }



        /// <summary>
        /// Обновление справочника
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolUpdatingDirectories_Click(object sender, EventArgs e)
        {
            try
            {
                SharingEvents.SharingEvents.OnDirectoryUpdate();
                SharingEvents.SharingEvents.OnDirectoryCarrierDataUpdate();
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не удалось обновить справочник. Ошибка: " + ex.Message);
            }
        }


        /// <summary>
        /// Вывод диалогового окна о версии программы и дате обновлении справочника 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void toolSMInformationOnProgram_Click(object sender, EventArgs e)
        {
            try
            {
                using (var form = new InformationOnProgram())
                {
                    form.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия формы: " + ex.Message);
            }
        }

        private void dgvDataBase_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
                DeleteRow();
        }

        private void TempLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(string.Concat(Application.StartupPath, @"\templog.log"));
        }
    }
}

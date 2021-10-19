using System;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.ClientApp
{
    public partial class DBForm : Form
    {
        private Timer timerMain;
        private bool closeForm;

        public DBForm()
        {
            InitializeComponent();

#if !DEBUG
            btnGetJeson.Visible = false;
#else
#endif
            openFileDialog.Filter = "Text files(*.omni)|*.omni";
            saveFileDialog.Filter = "Text files(*.xls)|*.xls";
            saveFileJsonDialog.Filter = "Text files(*.Json)|*.Json";

            SharingEvents.SharingEvents.WarnMessage += WarnMessage;
            SharingEvents.SharingEvents.DeleteRow += DeleteRow;


            //// Проверка первой строки
            //if (dtDataBase.Rows.Count <= 0)
            //{
            //    btnCanceled.Enabled = false;
            //    btnExportToExcel.Enabled = false;
            //    btnGetJeson.Enabled = false;
            //    btnCalculate.Enabled = false;
            //    cbDataFilter.Enabled = false;
            //    menuStrip.Enabled = false;
            //}
            //for (var i = 0; i < dgvDataBase.Columns.Count; i++)
            //    dgvDataBase.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
        }

        private void DBForm_Shown(object sender, EventArgs e)
        {
            timerMain = new Timer();
            timerMain.Tick += UpdateTimer;
            timerMain.Interval = 1000;
            timerMain.Start();
        }

//  Обновление данных на форме
        private async void UpdateTimer(object myObject, EventArgs myEventArgs)
        {
            var rows = await SharingEvents.SharingEvents.OnGetCalculation();

            Array.Reverse(rows);

            var rowsCount = dgvDataBase.RowCount;
            for (var i = 1; i <= rows.Length - rowsCount; i++)
                dtDataBase.Rows.Add();

            var k = 0;
            foreach (var row in rows)
            {
                if (cbDataFilter.Checked && row.DateTime.AddMonths(1) < DateTime.Now)
                    continue;
                dtDataBase.Rows[k][0] = row.Id;
                dtDataBase.Rows[k][1] = row.RunCode;
                dtDataBase.Rows[k][2] = row.Path;
                dtDataBase.Rows[k][3] = row.State;
                dtDataBase.Rows[k][4] = row.Status;
                dtDataBase.Rows[k][5] = row.Restart ? "Требуется перезапуск" : "Перезапуск не требуется";
                dtDataBase.Rows[k][6] = row.DateTime;
                k++;
            }

            if (dtDataBase.Rows.Count > 0)
            {
                btnCanceled.Enabled = true;
                btnExportToExcel.Enabled = true;
                return;
            }

            btnCanceled.Enabled = false;
            btnExportToExcel.Enabled = false;
        }

        //  Отменить расчет
        private void Canceled_Click(object sender, EventArgs e)
        {
            DeleteRow();
        }

        private async void Canseled(int id)
        {
            try
            {
                await SharingEvents.SharingEvents.OnCanseled(id);
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
                var id = int.Parse((string)dgvDataBase.Rows[row.Index].Cells[0].Value);
                var path = (string)dgvDataBase.Rows[row.Index].Cells[1].Value;
                ExportToExcel(id, path);
                return;
            }
        }

        private async void ExportToExcel(int id, string path)
        {
            saveFileDialog.InitialDirectory = path;
            saveFileDialog.FileName =
                $"{new DirectoryInfo(path).Name}_{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}";

            if (saveFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            try
            {
                await SharingEvents.SharingEvents.OnExportToExcel(id, saveFileDialog.FileName);
                Process.Start(saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не удалось создать файл отчета. Ошибка: " + ex.Message);
            }
        }

        //  Закрытие формы
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
            closeForm = true;
            Close();
        }

        //  открыть папку с прогоном
        private void dgvDataBase_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if ((string)dgvDataBase.Rows[e.RowIndex].Cells[2].Value == "" || e.RowIndex < 0) return;
            var pathFolder = (string)dgvDataBase.Rows[e.RowIndex].Cells[2].Value;
            if (Directory.Exists(pathFolder))
                Process.Start("explorer.exe", pathFolder);
        }

        //  добавить новый расчет
        private void btnAddNewCalculation_Click(object sender, EventArgs e)
        {
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

            openFileDialog.FileName = "";
            if (openFileDialog.ShowDialog() == DialogResult.Cancel)
                return;

            var omniFileName = Helper.GetUniversalName(openFileDialog.FileName);

            var referenceInputData = new ReferenceInputData();
            try
            {
                new DataInputForm(referenceInputData, omniFileName).ShowDialog();
                if (referenceInputData.WorkItemName != null)
                    SharingEvents.SharingEvents.OnSentPdi(omniFileName, referenceInputData);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка добавления расчета: " + ex.Message);
            }
        }

        //  сообщения из сервиса
        public async Task WarnMessage(string message)
        {
            await Task.Run(() => { MessageBox.Show(message); });
        }

        //  удалить расчет из таблицы на форме
        public async Task DeleteRow(string message, int cancelElementId)
        {
            await Task.Run(() =>
            {
                MessageBox.Show(message);
                BeginInvoke(new Action(() =>
                {
                    foreach (DataRow row in dtDataBase.Rows)
                        if (row[0].ToString() == cancelElementId.ToString())
                        {
                            row.Delete();
                            break;
                        }
                }));
            });
        }

        private void DeleteRow()
        {
            var selectedRows = dgvDataBase.SelectedRows;
            var lastRowIndex = 0;
            foreach (DataGridViewRow row in selectedRows)
            {
                var id = int.Parse((string)dgvDataBase.Rows[row.Index].Cells[0].Value);
                Canseled(id);
                lastRowIndex = row.Index;
                dtDataBase.Rows[row.Index].Delete();
            }

            if (lastRowIndex == dtDataBase.Rows.Count && dtDataBase.Rows.Count != 0)
                dgvDataBase.Rows[lastRowIndex - 1].Selected = true;

            if (dtDataBase.Rows.Count != 0) return;
            btnExportToExcel.Enabled = false;
            btnCanceled.Enabled = false;
        }

        //  форма изменения адреса сервера
        private async void addressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                await Task.Run(() => { new AddressesForm().ShowDialog(); });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка открытия формы: " + ex.Message);
            }
        }

        //  экспорт в Jeson
        private async void ExportToJson(int id, string path)
        {
            saveFileJsonDialog.InitialDirectory = path;
            saveFileJsonDialog.FileName =
                $"{new DirectoryInfo(path).Name}_{DateTime.Now:dd.MM.yyyy}_{DateTime.Now.ToString("T").Replace(":", ".")}";

            if (saveFileJsonDialog.ShowDialog() == DialogResult.Cancel)
                return;

            try
            {
                await SharingEvents.SharingEvents.OnExportToJeson(id, saveFileDialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show(@"Не удалось создать файл отчета. Ошибка: " + ex.Message);
            }
        }

        // Сохранить в Jeson
        private void btnGetJeson_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvDataBase.SelectedRows;
            foreach (DataGridViewRow row in selectedRows)
            {
                var id = int.Parse((string)dgvDataBase.Rows[row.Index].Cells[0].Value);
                var path = (string)dgvDataBase.Rows[row.Index].Cells[1].Value;
                ExportToJson(id, path);
                return;
            }
        }

        /// <summary>
        /// Обновление справочника
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void toolSMDirectoryUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                await SharingEvents.SharingEvents.OnDirectoryUpdate();
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

                new InformationOnProgram().ShowDialog();
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
    }
}

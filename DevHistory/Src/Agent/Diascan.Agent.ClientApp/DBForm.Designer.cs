namespace Diascan.Agent.ClientApp
{
    partial class DBForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DBForm));
            this.dgvDataBase = new System.Windows.Forms.DataGridView();
            this.dsDataBase = new System.Data.DataSet();
            this.dtDataBase = new System.Data.DataTable();
            this.dcId = new System.Data.DataColumn();
            this.dcRunCode = new System.Data.DataColumn();
            this.dcPath = new System.Data.DataColumn();
            this.dcState = new System.Data.DataColumn();
            this.dcStatus = new System.Data.DataColumn();
            this.dcRestart = new System.Data.DataColumn();
            this.dcDateTime = new System.Data.DataColumn();
            this.btnCanceled = new System.Windows.Forms.Button();
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuCloseItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnExportToExcel = new System.Windows.Forms.Button();
            this.btnCalculate = new System.Windows.Forms.Button();
            this.cbDataFilter = new System.Windows.Forms.CheckBox();
            this.menuStrip = new System.Windows.Forms.MenuStrip();
            this.editToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addressesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolSMDirectoryUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolSMInformationOnProgram = new System.Windows.Forms.ToolStripMenuItem();
            this.PerformanceManagementToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.TempLogToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bCiw = new System.Windows.Forms.Button();
            this.dgvtbcId = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Column2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcRunCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcPath = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcState = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcRestart = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dgvtbcDateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataBase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsDataBase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDataBase)).BeginInit();
            this.contextMenuStrip.SuspendLayout();
            this.menuStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvDataBase
            // 
            this.dgvDataBase.AllowUserToAddRows = false;
            this.dgvDataBase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvDataBase.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvDataBase.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.dgvDataBase.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvDataBase.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.dgvtbcId,
            this.Column1,
            this.Column2,
            this.dgvtbcRunCode,
            this.dgvtbcPath,
            this.dgvtbcState,
            this.dgvtbcStatus,
            this.dgvtbcRestart,
            this.dgvtbcDateTime});
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.dgvDataBase.DefaultCellStyle = dataGridViewCellStyle1;
            this.dgvDataBase.Location = new System.Drawing.Point(12, 27);
            this.dgvDataBase.Name = "dgvDataBase";
            this.dgvDataBase.ReadOnly = true;
            this.dgvDataBase.RowHeadersVisible = false;
            this.dgvDataBase.RowHeadersWidthSizeMode = System.Windows.Forms.DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders;
            this.dgvDataBase.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvDataBase.Size = new System.Drawing.Size(1083, 503);
            this.dgvDataBase.TabIndex = 0;
            this.dgvDataBase.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgvDataBase_CellDoubleClick);
            this.dgvDataBase.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dgvDataBase_KeyDown);
            // 
            // dsDataBase
            // 
            this.dsDataBase.DataSetName = "NewDataSet";
            this.dsDataBase.Tables.AddRange(new System.Data.DataTable[] {
            this.dtDataBase});
            // 
            // dtDataBase
            // 
            this.dtDataBase.Columns.AddRange(new System.Data.DataColumn[] {
            this.dcId,
            this.dcRunCode,
            this.dcPath,
            this.dcState,
            this.dcStatus,
            this.dcRestart,
            this.dcDateTime});
            this.dtDataBase.TableName = "DataBase";
            // 
            // dcId
            // 
            this.dcId.AllowDBNull = false;
            this.dcId.Caption = "ID";
            this.dcId.ColumnName = "ID";
            this.dcId.DefaultValue = "";
            // 
            // dcRunCode
            // 
            this.dcRunCode.AllowDBNull = false;
            this.dcRunCode.Caption = "Название (код) прогона";
            this.dcRunCode.ColumnName = "RunCode";
            this.dcRunCode.DefaultValue = "";
            // 
            // dcPath
            // 
            this.dcPath.AllowDBNull = false;
            this.dcPath.Caption = "Расположение";
            this.dcPath.ColumnName = "Path";
            this.dcPath.DefaultValue = "";
            // 
            // dcState
            // 
            this.dcState.AllowDBNull = false;
            this.dcState.Caption = "Состояние";
            this.dcState.ColumnName = "State";
            this.dcState.DefaultValue = "";
            // 
            // dcStatus
            // 
            this.dcStatus.AllowDBNull = false;
            this.dcStatus.Caption = "Статус отчета";
            this.dcStatus.ColumnName = "Status";
            this.dcStatus.DefaultValue = "";
            // 
            // dcRestart
            // 
            this.dcRestart.AllowDBNull = false;
            this.dcRestart.Caption = "Необходимость перезапуска";
            this.dcRestart.ColumnName = "Restart";
            this.dcRestart.DefaultValue = "";
            // 
            // dcDateTime
            // 
            this.dcDateTime.AllowDBNull = false;
            this.dcDateTime.Caption = "Время добавления расчета";
            this.dcDateTime.ColumnName = "DateTime";
            this.dcDateTime.DefaultValue = "";
            // 
            // btnCanceled
            // 
            this.btnCanceled.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCanceled.Location = new System.Drawing.Point(798, 536);
            this.btnCanceled.Name = "btnCanceled";
            this.btnCanceled.Size = new System.Drawing.Size(101, 26);
            this.btnCanceled.TabIndex = 1;
            this.btnCanceled.Text = "Отменить";
            this.btnCanceled.UseVisualStyleBackColor = true;
            this.btnCanceled.Click += new System.EventHandler(this.Canceled_Click);
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipText = "Анализатор диагностической информации";
            this.notifyIcon.ContextMenuStrip = this.contextMenuStrip;
            this.notifyIcon.Icon = global::Diascan.Agent.ClientApp.Properties.Resources.favicon_16x16;
            this.notifyIcon.Text = "Анализатор диагностической информации";
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_MouseDoubleClick);
            // 
            // contextMenuStrip
            // 
            this.contextMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuCloseItem});
            this.contextMenuStrip.Name = "contextMenuStrip";
            this.contextMenuStrip.Size = new System.Drawing.Size(109, 26);
            // 
            // toolStripMenuCloseItem
            // 
            this.toolStripMenuCloseItem.Name = "toolStripMenuCloseItem";
            this.toolStripMenuCloseItem.Size = new System.Drawing.Size(108, 22);
            this.toolStripMenuCloseItem.Text = "Выход";
            this.toolStripMenuCloseItem.Click += new System.EventHandler(this.CloseMenu_Click);
            // 
            // btnExportToExcel
            // 
            this.btnExportToExcel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnExportToExcel.Location = new System.Drawing.Point(119, 536);
            this.btnExportToExcel.Name = "btnExportToExcel";
            this.btnExportToExcel.Size = new System.Drawing.Size(101, 26);
            this.btnExportToExcel.TabIndex = 3;
            this.btnExportToExcel.Text = "Экспорт данных";
            this.btnExportToExcel.UseVisualStyleBackColor = true;
            this.btnExportToExcel.Click += new System.EventHandler(this.btnExportToExcel_Click);
            // 
            // btnCalculate
            // 
            this.btnCalculate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCalculate.Location = new System.Drawing.Point(12, 536);
            this.btnCalculate.Name = "btnCalculate";
            this.btnCalculate.Size = new System.Drawing.Size(101, 26);
            this.btnCalculate.TabIndex = 4;
            this.btnCalculate.Text = "Расчет";
            this.btnCalculate.UseVisualStyleBackColor = true;
            this.btnCalculate.Click += new System.EventHandler(this.btnAddNewCalculation_Click);
            // 
            // cbDataFilter
            // 
            this.cbDataFilter.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbDataFilter.AutoSize = true;
            this.cbDataFilter.Location = new System.Drawing.Point(905, 542);
            this.cbDataFilter.Name = "cbDataFilter";
            this.cbDataFilter.Size = new System.Drawing.Size(190, 17);
            this.cbDataFilter.TabIndex = 5;
            this.cbDataFilter.Text = "Скрыть расчеты старше месяца";
            this.cbDataFilter.UseVisualStyleBackColor = true;
            // 
            // menuStrip
            // 
            this.menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.editToolStripMenuItem});
            this.menuStrip.Location = new System.Drawing.Point(0, 0);
            this.menuStrip.Name = "menuStrip";
            this.menuStrip.Size = new System.Drawing.Size(1107, 24);
            this.menuStrip.TabIndex = 6;
            this.menuStrip.Text = "menuStrip";
            // 
            // editToolStripMenuItem
            // 
            this.editToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addressesToolStripMenuItem,
            this.toolSMDirectoryUpdate,
            this.toolSMInformationOnProgram,
            this.PerformanceManagementToolStripMenuItem,
            this.TempLogToolStripMenuItem});
            this.editToolStripMenuItem.Name = "editToolStripMenuItem";
            this.editToolStripMenuItem.Size = new System.Drawing.Size(79, 20);
            this.editToolStripMenuItem.Text = "Настройки";
            // 
            // addressesToolStripMenuItem
            // 
            this.addressesToolStripMenuItem.Name = "addressesToolStripMenuItem";
            this.addressesToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.addressesToolStripMenuItem.Text = "Адрес сервера";
            this.addressesToolStripMenuItem.Click += new System.EventHandler(this.addressToolStripMenuItem_Click);
            // 
            // toolSMDirectoryUpdate
            // 
            this.toolSMDirectoryUpdate.Name = "toolSMDirectoryUpdate";
            this.toolSMDirectoryUpdate.Size = new System.Drawing.Size(267, 22);
            this.toolSMDirectoryUpdate.Text = "Обновление справочников";
            this.toolSMDirectoryUpdate.Click += new System.EventHandler(this.toolUpdatingDirectories_Click);
            // 
            // toolSMInformationOnProgram
            // 
            this.toolSMInformationOnProgram.Name = "toolSMInformationOnProgram";
            this.toolSMInformationOnProgram.Size = new System.Drawing.Size(267, 22);
            this.toolSMInformationOnProgram.Text = "Информация";
            this.toolSMInformationOnProgram.Click += new System.EventHandler(this.toolSMInformationOnProgram_Click);
            // 
            // PerformanceManagementToolStripMenuItem
            // 
            this.PerformanceManagementToolStripMenuItem.Name = "PerformanceManagementToolStripMenuItem";
            this.PerformanceManagementToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.PerformanceManagementToolStripMenuItem.Text = "Управление производительностью";
            // 
            // TempLogToolStripMenuItem
            // 
            this.TempLogToolStripMenuItem.Name = "TempLogToolStripMenuItem";
            this.TempLogToolStripMenuItem.Size = new System.Drawing.Size(267, 22);
            this.TempLogToolStripMenuItem.Text = "Открыть templog.log";
            this.TempLogToolStripMenuItem.Click += new System.EventHandler(this.TempLogToolStripMenuItem_Click);
            // 
            // bCiw
            // 
            this.bCiw.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.bCiw.Location = new System.Drawing.Point(226, 536);
            this.bCiw.Name = "bCiw";
            this.bCiw.Size = new System.Drawing.Size(101, 26);
            this.bCiw.TabIndex = 7;
            this.bCiw.Text = "Расчет СМР";
            this.bCiw.UseVisualStyleBackColor = true;
            this.bCiw.Click += new System.EventHandler(this.bCiw_Click);
            // 
            // dgvtbcId
            // 
            this.dgvtbcId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvtbcId.DataPropertyName = "Id";
            this.dgvtbcId.FillWeight = 50F;
            this.dgvtbcId.HeaderText = "№";
            this.dgvtbcId.MinimumWidth = 50;
            this.dgvtbcId.Name = "dgvtbcId";
            this.dgvtbcId.ReadOnly = true;
            this.dgvtbcId.Width = 50;
            // 
            // Column1
            // 
            this.Column1.DataPropertyName = "GlobalId";
            this.Column1.HeaderText = "GlobalId";
            this.Column1.Name = "Column1";
            this.Column1.ReadOnly = true;
            this.Column1.Visible = false;
            // 
            // Column2
            // 
            this.Column2.DataPropertyName = "Сompleted";
            this.Column2.HeaderText = "Сompleted";
            this.Column2.Name = "Column2";
            this.Column2.ReadOnly = true;
            this.Column2.Visible = false;
            // 
            // dgvtbcRunCode
            // 
            this.dgvtbcRunCode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvtbcRunCode.DataPropertyName = "RunCode";
            this.dgvtbcRunCode.FillWeight = 160F;
            this.dgvtbcRunCode.HeaderText = "Назавание (код) прогона";
            this.dgvtbcRunCode.MinimumWidth = 160;
            this.dgvtbcRunCode.Name = "dgvtbcRunCode";
            this.dgvtbcRunCode.ReadOnly = true;
            this.dgvtbcRunCode.Width = 160;
            // 
            // dgvtbcPath
            // 
            this.dgvtbcPath.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.dgvtbcPath.DataPropertyName = "Path";
            this.dgvtbcPath.FillWeight = 20.40816F;
            this.dgvtbcPath.HeaderText = "Расположение";
            this.dgvtbcPath.MinimumWidth = 200;
            this.dgvtbcPath.Name = "dgvtbcPath";
            this.dgvtbcPath.ReadOnly = true;
            // 
            // dgvtbcState
            // 
            this.dgvtbcState.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvtbcState.DataPropertyName = "State";
            this.dgvtbcState.FillWeight = 190F;
            this.dgvtbcState.HeaderText = "Состояние расчета";
            this.dgvtbcState.MinimumWidth = 190;
            this.dgvtbcState.Name = "dgvtbcState";
            this.dgvtbcState.ReadOnly = true;
            this.dgvtbcState.Width = 190;
            // 
            // dgvtbcStatus
            // 
            this.dgvtbcStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvtbcStatus.DataPropertyName = "Status";
            this.dgvtbcStatus.FillWeight = 101F;
            this.dgvtbcStatus.HeaderText = "Статус отчета";
            this.dgvtbcStatus.MinimumWidth = 101;
            this.dgvtbcStatus.Name = "dgvtbcStatus";
            this.dgvtbcStatus.ReadOnly = true;
            this.dgvtbcStatus.Width = 101;
            // 
            // dgvtbcRestart
            // 
            this.dgvtbcRestart.DataPropertyName = "Restart";
            this.dgvtbcRestart.FillWeight = 180F;
            this.dgvtbcRestart.HeaderText = "Необходимость перезапуска ВИП";
            this.dgvtbcRestart.MinimumWidth = 180;
            this.dgvtbcRestart.Name = "dgvtbcRestart";
            this.dgvtbcRestart.ReadOnly = true;
            // 
            // dgvtbcDateTime
            // 
            this.dgvtbcDateTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.dgvtbcDateTime.DataPropertyName = "DateTime";
            this.dgvtbcDateTime.FillWeight = 170F;
            this.dgvtbcDateTime.HeaderText = "Дата добавления расчета";
            this.dgvtbcDateTime.MinimumWidth = 170;
            this.dgvtbcDateTime.Name = "dgvtbcDateTime";
            this.dgvtbcDateTime.ReadOnly = true;
            this.dgvtbcDateTime.Width = 170;
            // 
            // DBForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1107, 571);
            this.Controls.Add(this.bCiw);
            this.Controls.Add(this.menuStrip);
            this.Controls.Add(this.cbDataFilter);
            this.Controls.Add(this.btnCalculate);
            this.Controls.Add(this.btnExportToExcel);
            this.Controls.Add(this.btnCanceled);
            this.Controls.Add(this.dgvDataBase);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip;
            this.MinimumSize = new System.Drawing.Size(1123, 610);
            this.Name = "DBForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ПО ПДИ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DBForm_FormClosing);
            this.Shown += new System.EventHandler(this.DBForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDataBase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dsDataBase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dtDataBase)).EndInit();
            this.contextMenuStrip.ResumeLayout(false);
            this.menuStrip.ResumeLayout(false);
            this.menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvDataBase;
        private System.Data.DataSet dsDataBase;
        private System.Data.DataTable dtDataBase;

        private System.Data.DataColumn dcId;
        private System.Data.DataColumn dcRunCode;
        private System.Data.DataColumn dcPath;
        private System.Data.DataColumn dcState;
        private System.Data.DataColumn dcStatus;
        private System.Data.DataColumn dcRestart;
        private System.Data.DataColumn dcDateTime;
        
        private System.Windows.Forms.Button btnCanceled;
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuCloseItem;
        private System.Windows.Forms.Button btnExportToExcel;
        private System.Windows.Forms.Button btnCalculate;
        private System.Windows.Forms.CheckBox cbDataFilter;
        private System.Windows.Forms.MenuStrip menuStrip;
        private System.Windows.Forms.ToolStripMenuItem editToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addressesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolSMDirectoryUpdate;
        private System.Windows.Forms.ToolStripMenuItem toolSMInformationOnProgram;
        private System.Windows.Forms.ToolStripMenuItem PerformanceManagementToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem TempLogToolStripMenuItem;
        private System.Windows.Forms.Button bCiw;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcId;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column1;
        private System.Windows.Forms.DataGridViewTextBoxColumn Column2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcRunCode;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcPath;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcState;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcStatus;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcRestart;
        private System.Windows.Forms.DataGridViewTextBoxColumn dgvtbcDateTime;
    }
}


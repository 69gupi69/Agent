namespace Diascan.Agent.ClientApp
{
    partial class DataInputForm
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
            this.btnOK = new System.Windows.Forms.Button();
            this.lName = new System.Windows.Forms.Label();
            this.lCustomer = new System.Windows.Forms.Label();
            this.lStation = new System.Windows.Forms.Label();
            this.lResponsibleForPass = new System.Windows.Forms.Label();
            this.tbResponsibleForPass = new System.Windows.Forms.TextBox();
            this.lSkipDate = new System.Windows.Forms.Label();
            this.lPipeline = new System.Windows.Forms.Label();
            this.lDiameter = new System.Windows.Forms.Label();
            this.lDefectoscope = new System.Windows.Forms.Label();
            this.tbDefectoscope = new System.Windows.Forms.TextBox();
            this.cbСontractor = new System.Windows.Forms.ComboBox();
            this.cbRoute = new System.Windows.Forms.ComboBox();
            this.cbPipeline = new System.Windows.Forms.ComboBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.tbDiameter = new System.Windows.Forms.TextBox();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.dtpDateWorkItem = new System.Windows.Forms.DateTimePicker();
            this.tbStartOfReceptionChamber = new System.Windows.Forms.TextBox();
            this.lEndOfTriggerChamber = new System.Windows.Forms.Label();
            this.tbEndOfTriggerChamber = new System.Windows.Forms.TextBox();
            this.lStartOfReceptionChamber = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(480, 224);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 24;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // lName
            // 
            this.lName.AutoSize = true;
            this.lName.Location = new System.Drawing.Point(289, 14);
            this.lName.Name = "lName";
            this.lName.Size = new System.Drawing.Size(128, 13);
            this.lName.TabIndex = 10;
            this.lName.Text = "Название (код) прогона";
            // 
            // lCustomer
            // 
            this.lCustomer.AutoSize = true;
            this.lCustomer.Location = new System.Drawing.Point(9, 14);
            this.lCustomer.Name = "lCustomer";
            this.lCustomer.Size = new System.Drawing.Size(55, 13);
            this.lCustomer.TabIndex = 0;
            this.lCustomer.Text = "Заказчик";
            // 
            // lStation
            // 
            this.lStation.AutoSize = true;
            this.lStation.Location = new System.Drawing.Point(9, 109);
            this.lStation.Name = "lStation";
            this.lStation.Size = new System.Drawing.Size(49, 13);
            this.lStation.TabIndex = 4;
            this.lStation.Text = "Участок";
            // 
            // lResponsibleForPass
            // 
            this.lResponsibleForPass.AutoSize = true;
            this.lResponsibleForPass.Location = new System.Drawing.Point(289, 109);
            this.lResponsibleForPass.Name = "lResponsibleForPass";
            this.lResponsibleForPass.Size = new System.Drawing.Size(145, 13);
            this.lResponsibleForPass.TabIndex = 14;
            this.lResponsibleForPass.Text = "Ответственный за пропуск";
            // 
            // tbResponsibleForPass
            // 
            this.tbResponsibleForPass.Location = new System.Drawing.Point(292, 125);
            this.tbResponsibleForPass.Name = "tbResponsibleForPass";
            this.tbResponsibleForPass.Size = new System.Drawing.Size(263, 20);
            this.tbResponsibleForPass.TabIndex = 15;
            // 
            // lSkipDate
            // 
            this.lSkipDate.AutoSize = true;
            this.lSkipDate.Location = new System.Drawing.Point(9, 211);
            this.lSkipDate.Name = "lSkipDate";
            this.lSkipDate.Size = new System.Drawing.Size(83, 13);
            this.lSkipDate.TabIndex = 8;
            this.lSkipDate.Text = "Дата пропуска";
            // 
            // lPipeline
            // 
            this.lPipeline.AutoSize = true;
            this.lPipeline.Location = new System.Drawing.Point(9, 62);
            this.lPipeline.Name = "lPipeline";
            this.lPipeline.Size = new System.Drawing.Size(73, 13);
            this.lPipeline.TabIndex = 2;
            this.lPipeline.Text = "Трубопровод";
            // 
            // lDiameter
            // 
            this.lDiameter.AutoSize = true;
            this.lDiameter.Location = new System.Drawing.Point(9, 159);
            this.lDiameter.Name = "lDiameter";
            this.lDiameter.Size = new System.Drawing.Size(53, 13);
            this.lDiameter.TabIndex = 6;
            this.lDiameter.Text = "Диаметр";
            // 
            // lDefectoscope
            // 
            this.lDefectoscope.AutoSize = true;
            this.lDefectoscope.Location = new System.Drawing.Point(289, 62);
            this.lDefectoscope.Name = "lDefectoscope";
            this.lDefectoscope.Size = new System.Drawing.Size(91, 13);
            this.lDefectoscope.TabIndex = 12;
            this.lDefectoscope.Text = "Дефектоскоп №";
            // 
            // tbDefectoscope
            // 
            this.tbDefectoscope.Location = new System.Drawing.Point(292, 79);
            this.tbDefectoscope.Name = "tbDefectoscope";
            this.tbDefectoscope.Size = new System.Drawing.Size(263, 20);
            this.tbDefectoscope.TabIndex = 13;
            // 
            // cbСontractor
            // 
            this.cbСontractor.FormattingEnabled = true;
            this.cbСontractor.Location = new System.Drawing.Point(12, 30);
            this.cbСontractor.Name = "cbСontractor";
            this.cbСontractor.Size = new System.Drawing.Size(238, 21);
            this.cbСontractor.TabIndex = 1;
            this.cbСontractor.SelectionChangeCommitted += new System.EventHandler(this.cbCustomer_SelectionChangeCommitted);
            // 
            // cbRoute
            // 
            this.cbRoute.FormattingEnabled = true;
            this.cbRoute.Location = new System.Drawing.Point(12, 125);
            this.cbRoute.Name = "cbRoute";
            this.cbRoute.Size = new System.Drawing.Size(238, 21);
            this.cbRoute.TabIndex = 5;
            this.cbRoute.SelectionChangeCommitted += new System.EventHandler(this.cbRoute_SelectionChangeCommitted);
            // 
            // cbPipeline
            // 
            this.cbPipeline.FormattingEnabled = true;
            this.cbPipeline.Location = new System.Drawing.Point(12, 78);
            this.cbPipeline.Name = "cbPipeline";
            this.cbPipeline.Size = new System.Drawing.Size(238, 21);
            this.cbPipeline.TabIndex = 3;
            this.cbPipeline.SelectionChangeCommitted += new System.EventHandler(this.cbPipeline_SelectionChangeCommitted);
            // 
            // tbName
            // 
            this.tbName.Location = new System.Drawing.Point(292, 31);
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(263, 20);
            this.tbName.TabIndex = 11;
            this.tbName.Click += new System.EventHandler(this.tbName_Click);
            this.tbName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbName_KeyPress);
            // 
            // tbDiameter
            // 
            this.tbDiameter.Location = new System.Drawing.Point(12, 176);
            this.tbDiameter.Name = "tbDiameter";
            this.tbDiameter.ReadOnly = true;
            this.tbDiameter.Size = new System.Drawing.Size(238, 20);
            this.tbDiameter.TabIndex = 7;
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // dtpDateWorkItem
            // 
            this.dtpDateWorkItem.Location = new System.Drawing.Point(12, 227);
            this.dtpDateWorkItem.Name = "dtpDateWorkItem";
            this.dtpDateWorkItem.Size = new System.Drawing.Size(238, 20);
            this.dtpDateWorkItem.TabIndex = 9;
            // 
            // tbStartOfReceptionChamber
            // 
            this.tbStartOfReceptionChamber.Location = new System.Drawing.Point(292, 176);
            this.tbStartOfReceptionChamber.Name = "tbStartOfReceptionChamber";
            this.tbStartOfReceptionChamber.Size = new System.Drawing.Size(125, 20);
            this.tbStartOfReceptionChamber.TabIndex = 23;
            // 
            // lEndOfTriggerChamber
            // 
            this.lEndOfTriggerChamber.AutoSize = true;
            this.lEndOfTriggerChamber.Location = new System.Drawing.Point(427, 159);
            this.lEndOfTriggerChamber.Name = "lEndOfTriggerChamber";
            this.lEndOfTriggerChamber.Size = new System.Drawing.Size(124, 13);
            this.lEndOfTriggerChamber.TabIndex = 20;
            this.lEndOfTriggerChamber.Text = "Длина камеры приема";
            // 
            // tbEndOfTriggerChamber
            // 
            this.tbEndOfTriggerChamber.Location = new System.Drawing.Point(430, 176);
            this.tbEndOfTriggerChamber.Name = "tbEndOfTriggerChamber";
            this.tbEndOfTriggerChamber.Size = new System.Drawing.Size(125, 20);
            this.tbEndOfTriggerChamber.TabIndex = 21;
            // 
            // lStartOfReceptionChamber
            // 
            this.lStartOfReceptionChamber.AutoSize = true;
            this.lStartOfReceptionChamber.Location = new System.Drawing.Point(289, 159);
            this.lStartOfReceptionChamber.Name = "lStartOfReceptionChamber";
            this.lStartOfReceptionChamber.Size = new System.Drawing.Size(115, 13);
            this.lStartOfReceptionChamber.TabIndex = 22;
            this.lStartOfReceptionChamber.Text = "Длина камеры пуска";
            // 
            // DataInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(570, 266);
            this.Controls.Add(this.lStartOfReceptionChamber);
            this.Controls.Add(this.tbStartOfReceptionChamber);
            this.Controls.Add(this.lEndOfTriggerChamber);
            this.Controls.Add(this.tbEndOfTriggerChamber);
            this.Controls.Add(this.dtpDateWorkItem);
            this.Controls.Add(this.tbDiameter);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.cbPipeline);
            this.Controls.Add(this.cbRoute);
            this.Controls.Add(this.cbСontractor);
            this.Controls.Add(this.lPipeline);
            this.Controls.Add(this.lDiameter);
            this.Controls.Add(this.lDefectoscope);
            this.Controls.Add(this.tbDefectoscope);
            this.Controls.Add(this.lResponsibleForPass);
            this.Controls.Add(this.tbResponsibleForPass);
            this.Controls.Add(this.lSkipDate);
            this.Controls.Add(this.lStation);
            this.Controls.Add(this.lCustomer);
            this.Controls.Add(this.lName);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "DataInputForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Паспорт диагностической информации";
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Label lName;
        private System.Windows.Forms.Label lCustomer;
        private System.Windows.Forms.Label lStation;
        private System.Windows.Forms.Label lResponsibleForPass;
        private System.Windows.Forms.Label lSkipDate;
        private System.Windows.Forms.Label lPipeline;
        private System.Windows.Forms.Label lDiameter;
        private System.Windows.Forms.Label lDefectoscope;
        private System.Windows.Forms.TextBox tbName;
        private System.Windows.Forms.TextBox tbDefectoscope;
        private System.Windows.Forms.TextBox tbResponsibleForPass;
        private System.Windows.Forms.ComboBox cbСontractor;
        private System.Windows.Forms.ComboBox cbRoute;
        private System.Windows.Forms.ComboBox cbPipeline;
        private System.Windows.Forms.TextBox tbDiameter;
        private System.Windows.Forms.ErrorProvider errorProvider;
        private System.Windows.Forms.DateTimePicker dtpDateWorkItem;
        private System.Windows.Forms.Label lStartOfReceptionChamber;
        private System.Windows.Forms.TextBox tbStartOfReceptionChamber;
        private System.Windows.Forms.Label lEndOfTriggerChamber;
        private System.Windows.Forms.TextBox tbEndOfTriggerChamber;
    }
}
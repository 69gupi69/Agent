﻿namespace Diascan.Agent.ClientApp
{
    partial class BaseInput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BaseInput));
            this.lStartOfReceptionChamber = new System.Windows.Forms.Label();
            this.tbStartOfReceptionChamber = new System.Windows.Forms.TextBox();
            this.lEndOfTriggerChamber = new System.Windows.Forms.Label();
            this.tbEndOfTriggerChamber = new System.Windows.Forms.TextBox();
            this.tbName = new System.Windows.Forms.TextBox();
            this.lDefectoscope = new System.Windows.Forms.Label();
            this.tbDefectoscope = new System.Windows.Forms.TextBox();
            this.lResponsibleForPass = new System.Windows.Forms.Label();
            this.tbResponsibleForPass = new System.Windows.Forms.TextBox();
            this.lName = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.dtpDateWorkItem = new System.Windows.Forms.DateTimePicker();
            this.lSkipDate = new System.Windows.Forms.Label();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // lStartOfReceptionChamber
            // 
            this.lStartOfReceptionChamber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lStartOfReceptionChamber.AutoSize = true;
            this.lStartOfReceptionChamber.Location = new System.Drawing.Point(280, 191);
            this.lStartOfReceptionChamber.Name = "lStartOfReceptionChamber";
            this.lStartOfReceptionChamber.Size = new System.Drawing.Size(135, 13);
            this.lStartOfReceptionChamber.TabIndex = 27;
            this.lStartOfReceptionChamber.Text = "Длина камеры пуска (м.)";
            // 
            // tbStartOfReceptionChamber
            // 
            this.tbStartOfReceptionChamber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbStartOfReceptionChamber.Location = new System.Drawing.Point(286, 207);
            this.tbStartOfReceptionChamber.Name = "tbStartOfReceptionChamber";
            this.tbStartOfReceptionChamber.Size = new System.Drawing.Size(125, 20);
            this.tbStartOfReceptionChamber.TabIndex = 28;
            // 
            // lEndOfTriggerChamber
            // 
            this.lEndOfTriggerChamber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lEndOfTriggerChamber.AutoSize = true;
            this.lEndOfTriggerChamber.Location = new System.Drawing.Point(421, 191);
            this.lEndOfTriggerChamber.Name = "lEndOfTriggerChamber";
            this.lEndOfTriggerChamber.Size = new System.Drawing.Size(144, 13);
            this.lEndOfTriggerChamber.TabIndex = 29;
            this.lEndOfTriggerChamber.Text = "Длина камеры приема (м.)";
            // 
            // tbEndOfTriggerChamber
            // 
            this.tbEndOfTriggerChamber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbEndOfTriggerChamber.Location = new System.Drawing.Point(429, 207);
            this.tbEndOfTriggerChamber.Name = "tbEndOfTriggerChamber";
            this.tbEndOfTriggerChamber.Size = new System.Drawing.Size(121, 20);
            this.tbEndOfTriggerChamber.TabIndex = 30;
            // 
            // tbName
            // 
            this.tbName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbName.Location = new System.Drawing.Point(286, 44);
            this.tbName.MaxLength = 5;
            this.tbName.Name = "tbName";
            this.tbName.Size = new System.Drawing.Size(263, 20);
            this.tbName.TabIndex = 22;
            this.tbName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbName_KeyPress);
            // 
            // lDefectoscope
            // 
            this.lDefectoscope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lDefectoscope.AutoSize = true;
            this.lDefectoscope.Location = new System.Drawing.Point(280, 82);
            this.lDefectoscope.Name = "lDefectoscope";
            this.lDefectoscope.Size = new System.Drawing.Size(91, 13);
            this.lDefectoscope.TabIndex = 23;
            this.lDefectoscope.Text = "Дефектоскоп №";
            // 
            // tbDefectoscope
            // 
            this.tbDefectoscope.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbDefectoscope.Location = new System.Drawing.Point(286, 98);
            this.tbDefectoscope.Name = "tbDefectoscope";
            this.tbDefectoscope.Size = new System.Drawing.Size(263, 20);
            this.tbDefectoscope.TabIndex = 24;
            // 
            // lResponsibleForPass
            // 
            this.lResponsibleForPass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lResponsibleForPass.AutoSize = true;
            this.lResponsibleForPass.Location = new System.Drawing.Point(283, 139);
            this.lResponsibleForPass.Name = "lResponsibleForPass";
            this.lResponsibleForPass.Size = new System.Drawing.Size(145, 13);
            this.lResponsibleForPass.TabIndex = 25;
            this.lResponsibleForPass.Text = "Ответственный за пропуск";
            // 
            // tbResponsibleForPass
            // 
            this.tbResponsibleForPass.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.tbResponsibleForPass.Location = new System.Drawing.Point(286, 155);
            this.tbResponsibleForPass.Name = "tbResponsibleForPass";
            this.tbResponsibleForPass.Size = new System.Drawing.Size(263, 20);
            this.tbResponsibleForPass.TabIndex = 26;
            // 
            // lName
            // 
            this.lName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lName.AutoSize = true;
            this.lName.Location = new System.Drawing.Point(283, 28);
            this.lName.Name = "lName";
            this.lName.Size = new System.Drawing.Size(128, 13);
            this.lName.TabIndex = 21;
            this.lName.Text = "Название (код) прогона";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnOK.Location = new System.Drawing.Point(478, 308);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 31;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // dtpDateWorkItem
            // 
            this.dtpDateWorkItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.dtpDateWorkItem.Location = new System.Drawing.Point(286, 256);
            this.dtpDateWorkItem.Name = "dtpDateWorkItem";
            this.dtpDateWorkItem.Size = new System.Drawing.Size(263, 20);
            this.dtpDateWorkItem.TabIndex = 33;
            // 
            // lSkipDate
            // 
            this.lSkipDate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lSkipDate.AutoSize = true;
            this.lSkipDate.Location = new System.Drawing.Point(280, 240);
            this.lSkipDate.Name = "lSkipDate";
            this.lSkipDate.Size = new System.Drawing.Size(83, 13);
            this.lSkipDate.TabIndex = 32;
            this.lSkipDate.Text = "Дата пропуска";
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // BaseInput
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 343);
            this.Controls.Add(this.dtpDateWorkItem);
            this.Controls.Add(this.lSkipDate);
            this.Controls.Add(this.lStartOfReceptionChamber);
            this.Controls.Add(this.tbStartOfReceptionChamber);
            this.Controls.Add(this.lEndOfTriggerChamber);
            this.Controls.Add(this.tbEndOfTriggerChamber);
            this.Controls.Add(this.tbName);
            this.Controls.Add(this.lDefectoscope);
            this.Controls.Add(this.tbDefectoscope);
            this.Controls.Add(this.lResponsibleForPass);
            this.Controls.Add(this.tbResponsibleForPass);
            this.Controls.Add(this.lName);
            this.Controls.Add(this.btnOK);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BaseInput";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected System.Windows.Forms.Label lStartOfReceptionChamber;
        protected System.Windows.Forms.TextBox tbStartOfReceptionChamber;
        protected System.Windows.Forms.Label lEndOfTriggerChamber;
        protected System.Windows.Forms.TextBox tbEndOfTriggerChamber;
        protected System.Windows.Forms.TextBox tbName;
        protected System.Windows.Forms.Label lDefectoscope;
        protected System.Windows.Forms.TextBox tbDefectoscope;
        protected System.Windows.Forms.Label lResponsibleForPass;
        protected System.Windows.Forms.TextBox tbResponsibleForPass;
        protected System.Windows.Forms.Label lName;
        protected System.Windows.Forms.Button btnOK;
        protected System.Windows.Forms.DateTimePicker dtpDateWorkItem;
        protected System.Windows.Forms.Label lSkipDate;
        protected System.Windows.Forms.ErrorProvider errorProvider;
    }
}

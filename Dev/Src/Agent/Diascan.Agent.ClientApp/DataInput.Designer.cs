namespace Diascan.Agent.ClientApp
{
    partial class DataInput
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
            this.lCustomer = new System.Windows.Forms.Label();
            this.lDiameter = new System.Windows.Forms.Label();
            this.cbContractor = new System.Windows.Forms.ComboBox();
            this.tbDiameter = new System.Windows.Forms.TextBox();
            this.cbPipeline = new System.Windows.Forms.ComboBox();
            this.lPipeline = new System.Windows.Forms.Label();
            this.lRoute = new System.Windows.Forms.Label();
            this.cbRoute = new System.Windows.Forms.ComboBox();
            this.tbRoute = new System.Windows.Forms.TextBox();
            this.gbRoute = new System.Windows.Forms.GroupBox();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.gbRoute.SuspendLayout();
            this.SuspendLayout();
            // 
            // lCustomer
            // 
            this.lCustomer.AutoSize = true;
            this.lCustomer.Location = new System.Drawing.Point(12, 178);
            this.lCustomer.Name = "lCustomer";
            this.lCustomer.Size = new System.Drawing.Size(55, 13);
            this.lCustomer.TabIndex = 4;
            this.lCustomer.Text = "Заказчик";
            // 
            // lDiameter
            // 
            this.lDiameter.AutoSize = true;
            this.lDiameter.Location = new System.Drawing.Point(9, 230);
            this.lDiameter.Name = "lDiameter";
            this.lDiameter.Size = new System.Drawing.Size(53, 13);
            this.lDiameter.TabIndex = 6;
            this.lDiameter.Text = "Диаметр";
            // 
            // cbContractor
            // 
            this.cbContractor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbContractor.FormattingEnabled = true;
            this.cbContractor.Location = new System.Drawing.Point(12, 207);
            this.cbContractor.Name = "cbContractor";
            this.cbContractor.Size = new System.Drawing.Size(259, 21);
            this.cbContractor.TabIndex = 5;
            this.cbContractor.TextChanged += new System.EventHandler(this.cbContractor_TextChanged);
            // 
            // tbDiameter
            // 
            this.tbDiameter.Location = new System.Drawing.Point(12, 256);
            this.tbDiameter.Name = "tbDiameter";
            this.tbDiameter.ReadOnly = true;
            this.tbDiameter.Size = new System.Drawing.Size(259, 20);
            this.tbDiameter.TabIndex = 7;
            // 
            // cbPipeline
            // 
            this.cbPipeline.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbPipeline.FormattingEnabled = true;
            this.cbPipeline.Location = new System.Drawing.Point(12, 155);
            this.cbPipeline.Name = "cbPipeline";
            this.cbPipeline.Size = new System.Drawing.Size(259, 21);
            this.cbPipeline.TabIndex = 3;
            this.cbPipeline.SelectionChangeCommitted += new System.EventHandler(this.cbPipeline_SelectionChangeCommitted);
            this.cbPipeline.TextChanged += new System.EventHandler(this.cbPipeline_TextChanged);
            // 
            // lPipeline
            // 
            this.lPipeline.AutoSize = true;
            this.lPipeline.Location = new System.Drawing.Point(9, 139);
            this.lPipeline.Name = "lPipeline";
            this.lPipeline.Size = new System.Drawing.Size(73, 13);
            this.lPipeline.TabIndex = 2;
            this.lPipeline.Text = "Трубопровод";
            // 
            // lRoute
            // 
            this.lRoute.AutoSize = true;
            this.lRoute.Location = new System.Drawing.Point(7, 43);
            this.lRoute.MaximumSize = new System.Drawing.Size(260, 0);
            this.lRoute.Name = "lRoute";
            this.lRoute.Size = new System.Drawing.Size(49, 13);
            this.lRoute.TabIndex = 0;
            this.lRoute.Text = "Участок";
            // 
            // cbRoute
            // 
            this.cbRoute.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbRoute.FormattingEnabled = true;
            this.cbRoute.Location = new System.Drawing.Point(6, 84);
            this.cbRoute.Name = "cbRoute";
            this.cbRoute.Size = new System.Drawing.Size(259, 21);
            this.cbRoute.TabIndex = 1;
            this.cbRoute.SelectionChangeCommitted += new System.EventHandler(this.cbRoute_SelectionChangeCommitted);
            this.cbRoute.TextChanged += new System.EventHandler(this.cbRoute_TextChanged);
            // 
            // tbRoute
            // 
            this.tbRoute.Location = new System.Drawing.Point(6, 20);
            this.tbRoute.Name = "tbRoute";
            this.tbRoute.Size = new System.Drawing.Size(259, 20);
            this.tbRoute.TabIndex = 21;
            this.tbRoute.TextChanged += new System.EventHandler(this.tbRoute_TextChanged);
            // 
            // gbRoute
            // 
            this.gbRoute.Controls.Add(this.tbRoute);
            this.gbRoute.Controls.Add(this.cbRoute);
            this.gbRoute.Controls.Add(this.lRoute);
            this.gbRoute.Location = new System.Drawing.Point(6, 14);
            this.gbRoute.Name = "gbRoute";
            this.gbRoute.Size = new System.Drawing.Size(272, 120);
            this.gbRoute.TabIndex = 23;
            this.gbRoute.TabStop = false;
            this.gbRoute.Text = "Поиск по участкам";
            // 
            // DataInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 343);
            this.Controls.Add(this.lCustomer);
            this.Controls.Add(this.cbContractor);
            this.Controls.Add(this.cbPipeline);
            this.Controls.Add(this.lPipeline);
            this.Controls.Add(this.gbRoute);
            this.Controls.Add(this.tbDiameter);
            this.Controls.Add(this.lDiameter);
            this.Name = "DataInputForm";
            this.Text = "Паспорт диагностической информации";
            this.Controls.SetChildIndex(this.lName, 0);
            this.Controls.SetChildIndex(this.lResponsibleForPass, 0);
            this.Controls.SetChildIndex(this.lDefectoscope, 0);
            this.Controls.SetChildIndex(this.lEndOfTriggerChamber, 0);
            this.Controls.SetChildIndex(this.lStartOfReceptionChamber, 0);
            this.Controls.SetChildIndex(this.lSkipDate, 0);
            this.Controls.SetChildIndex(this.lDiameter, 0);
            this.Controls.SetChildIndex(this.tbDiameter, 0);
            this.Controls.SetChildIndex(this.gbRoute, 0);
            this.Controls.SetChildIndex(this.lPipeline, 0);
            this.Controls.SetChildIndex(this.cbPipeline, 0);
            this.Controls.SetChildIndex(this.cbContractor, 0);
            this.Controls.SetChildIndex(this.lCustomer, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.tbResponsibleForPass, 0);
            this.Controls.SetChildIndex(this.tbDefectoscope, 0);
            this.Controls.SetChildIndex(this.tbName, 0);
            this.Controls.SetChildIndex(this.tbEndOfTriggerChamber, 0);
            this.Controls.SetChildIndex(this.tbStartOfReceptionChamber, 0);
            this.Controls.SetChildIndex(this.dtpDateWorkItem, 0);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.gbRoute.ResumeLayout(false);
            this.gbRoute.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lCustomer;
        private System.Windows.Forms.Label lDiameter;
        private System.Windows.Forms.ComboBox cbContractor;
        private System.Windows.Forms.TextBox tbDiameter;
        private System.Windows.Forms.ComboBox cbPipeline;
        private System.Windows.Forms.Label lPipeline;
        private System.Windows.Forms.GroupBox gbRoute;
        private System.Windows.Forms.TextBox tbRoute;
        private System.Windows.Forms.ComboBox cbRoute;
        private System.Windows.Forms.Label lRoute;
        private System.Windows.Forms.ToolTip toolTip;
    }
}

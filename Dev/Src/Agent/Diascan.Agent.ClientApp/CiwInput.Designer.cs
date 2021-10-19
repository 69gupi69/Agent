namespace Diascan.Agent.ClientApp
{
    partial class CiwInput : BaseInput
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
            this.lDiameter = new System.Windows.Forms.Label();
            this.lContractor = new System.Windows.Forms.Label();
            this.lPipeline = new System.Windows.Forms.Label();
            this.lRoute = new System.Windows.Forms.Label();
            this.tbContractor = new System.Windows.Forms.TextBox();
            this.tbPipeline = new System.Windows.Forms.TextBox();
            this.tbRoute = new System.Windows.Forms.TextBox();
            this.cbDiameter = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // lDiameter
            // 
            this.lDiameter.AutoSize = true;
            this.lDiameter.Location = new System.Drawing.Point(12, 168);
            this.lDiameter.MaximumSize = new System.Drawing.Size(258, 0);
            this.lDiameter.Name = "lDiameter";
            this.lDiameter.Size = new System.Drawing.Size(53, 13);
            this.lDiameter.TabIndex = 49;
            this.lDiameter.Text = "Диаметр";
            // 
            // lContractor
            // 
            this.lContractor.AutoSize = true;
            this.lContractor.Location = new System.Drawing.Point(12, 117);
            this.lContractor.MaximumSize = new System.Drawing.Size(258, 0);
            this.lContractor.Name = "lContractor";
            this.lContractor.Size = new System.Drawing.Size(55, 13);
            this.lContractor.TabIndex = 47;
            this.lContractor.Text = "Заказчик";
            // 
            // lPipeline
            // 
            this.lPipeline.AutoSize = true;
            this.lPipeline.Location = new System.Drawing.Point(12, 67);
            this.lPipeline.MaximumSize = new System.Drawing.Size(258, 0);
            this.lPipeline.Name = "lPipeline";
            this.lPipeline.Size = new System.Drawing.Size(73, 13);
            this.lPipeline.TabIndex = 46;
            this.lPipeline.Text = "Трубопровод";
            // 
            // lRoute
            // 
            this.lRoute.AutoSize = true;
            this.lRoute.Location = new System.Drawing.Point(14, 16);
            this.lRoute.MaximumSize = new System.Drawing.Size(258, 0);
            this.lRoute.Name = "lRoute";
            this.lRoute.Size = new System.Drawing.Size(49, 13);
            this.lRoute.TabIndex = 45;
            this.lRoute.Text = "Участок";
            // 
            // tbContractor
            // 
            this.tbContractor.Location = new System.Drawing.Point(12, 145);
            this.tbContractor.Name = "tbContractor";
            this.tbContractor.Size = new System.Drawing.Size(259, 20);
            this.tbContractor.TabIndex = 44;
            // 
            // tbPipeline
            // 
            this.tbPipeline.Location = new System.Drawing.Point(12, 94);
            this.tbPipeline.Name = "tbPipeline";
            this.tbPipeline.Size = new System.Drawing.Size(258, 20);
            this.tbPipeline.TabIndex = 43;
            // 
            // tbRoute
            // 
            this.tbRoute.Location = new System.Drawing.Point(12, 44);
            this.tbRoute.Name = "tbRoute";
            this.tbRoute.Size = new System.Drawing.Size(258, 20);
            this.tbRoute.TabIndex = 42;
            // 
            // cbDiameter
            // 
            this.cbDiameter.FormattingEnabled = true;
            this.cbDiameter.Location = new System.Drawing.Point(12, 193);
            this.cbDiameter.Name = "cbDiameter";
            this.cbDiameter.Size = new System.Drawing.Size(259, 21);
            this.cbDiameter.TabIndex = 50;
            // 
            // CiwInputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(566, 343);
            this.Controls.Add(this.cbDiameter);
            this.Controls.Add(this.lDiameter);
            this.Controls.Add(this.lContractor);
            this.Controls.Add(this.lPipeline);
            this.Controls.Add(this.lRoute);
            this.Controls.Add(this.tbContractor);
            this.Controls.Add(this.tbPipeline);
            this.Controls.Add(this.tbRoute);
            this.Name = "CiwInputForm";
            this.Text = "CIWInputForm";
            this.Controls.SetChildIndex(this.lName, 0);
            this.Controls.SetChildIndex(this.lResponsibleForPass, 0);
            this.Controls.SetChildIndex(this.lDefectoscope, 0);
            this.Controls.SetChildIndex(this.lEndOfTriggerChamber, 0);
            this.Controls.SetChildIndex(this.lStartOfReceptionChamber, 0);
            this.Controls.SetChildIndex(this.lSkipDate, 0);
            this.Controls.SetChildIndex(this.btnOK, 0);
            this.Controls.SetChildIndex(this.tbResponsibleForPass, 0);
            this.Controls.SetChildIndex(this.tbDefectoscope, 0);
            this.Controls.SetChildIndex(this.tbName, 0);
            this.Controls.SetChildIndex(this.tbEndOfTriggerChamber, 0);
            this.Controls.SetChildIndex(this.tbStartOfReceptionChamber, 0);
            this.Controls.SetChildIndex(this.dtpDateWorkItem, 0);
            this.Controls.SetChildIndex(this.tbRoute, 0);
            this.Controls.SetChildIndex(this.tbPipeline, 0);
            this.Controls.SetChildIndex(this.tbContractor, 0);
            this.Controls.SetChildIndex(this.lRoute, 0);
            this.Controls.SetChildIndex(this.lPipeline, 0);
            this.Controls.SetChildIndex(this.lContractor, 0);
            this.Controls.SetChildIndex(this.lDiameter, 0);
            this.Controls.SetChildIndex(this.cbDiameter, 0);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lDiameter;
        private System.Windows.Forms.Label lContractor;
        private System.Windows.Forms.Label lPipeline;
        private System.Windows.Forms.Label lRoute;
        private System.Windows.Forms.TextBox tbContractor;
        private System.Windows.Forms.TextBox tbPipeline;
        private System.Windows.Forms.TextBox tbRoute;
        private System.Windows.Forms.ComboBox cbDiameter;
    }
}

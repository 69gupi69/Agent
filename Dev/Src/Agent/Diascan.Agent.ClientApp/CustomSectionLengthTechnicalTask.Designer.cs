namespace Diascan.Agent.ClientApp
{
    partial class CustomSectionLengthTechnicalTask
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
            this.btnСancel = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            this.cbGateValve = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.AccessibleName = "";
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(246, 59);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 0;
            this.btnOK.Tag = "";
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnСancel
            // 
            this.btnСancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnСancel.Location = new System.Drawing.Point(327, 59);
            this.btnСancel.Name = "btnСancel";
            this.btnСancel.Size = new System.Drawing.Size(75, 23);
            this.btnСancel.TabIndex = 1;
            this.btnСancel.Text = "Отмена";
            this.btnСancel.UseVisualStyleBackColor = true;
            this.btnСancel.Click += new System.EventHandler(this.btnСancel_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // cbGateValve
            // 
            this.cbGateValve.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cbGateValve.AutoSize = true;
            this.cbGateValve.Location = new System.Drawing.Point(246, 23);
            this.cbGateValve.Name = "cbGateValve";
            this.cbGateValve.Size = new System.Drawing.Size(122, 30);
            this.cbGateValve.TabIndex = 37;
            this.cbGateValve.Text = "Наличие приемной\r\nзадвижки";
            this.cbGateValve.UseVisualStyleBackColor = true;
            // 
            // CustomSectionLengthTechnicalTask
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(441, 92);
            this.Controls.Add(this.cbGateValve);
            this.Controls.Add(this.btnСancel);
            this.Controls.Add(this.btnOK);
            this.Icon = global::Diascan.Agent.ClientApp.Properties.Resources.favicon_16x16;
            this.Name = "CustomSectionLengthTechnicalTask";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Длина участка по ТЗ (м) (фактическая длина)";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.CustomSectionLengthTechnicalTask_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnСancel;
        private System.Windows.Forms.ErrorProvider errorProvider;
        public System.Windows.Forms.CheckBox cbGateValve;
    }
}

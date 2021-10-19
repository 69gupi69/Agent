namespace Diascan.Agent.ClientApp
{
    partial class InformationOnProgram
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose( bool disposing )
        {
            if( disposing && ( components != null ) )
            {
                components.Dispose();
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelDateUpdateDirectory = new System.Windows.Forms.Label();
            this.labelIimpendingUpdatesDirectoryDate = new System.Windows.Forms.Label();
            this.labelСopyrightCompany = new System.Windows.Forms.Label();
            this.labelIimpendingUpdatesCarrierData = new System.Windows.Forms.Label();
            this.labelDateUpdateCarrierData = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Location = new System.Drawing.Point(12, 9);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(44, 13);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Версия";
            // 
            // labelDateUpdateDirectory
            // 
            this.labelDateUpdateDirectory.AutoSize = true;
            this.labelDateUpdateDirectory.Location = new System.Drawing.Point(12, 32);
            this.labelDateUpdateDirectory.Name = "labelDateUpdateDirectory";
            this.labelDateUpdateDirectory.Size = new System.Drawing.Size(151, 13);
            this.labelDateUpdateDirectory.TabIndex = 0;
            this.labelDateUpdateDirectory.Text = "Дата обновления каталога: ";
            // 
            // labelIimpendingUpdatesDirectoryDate
            // 
            this.labelIimpendingUpdatesDirectoryDate.AutoSize = true;
            this.labelIimpendingUpdatesDirectoryDate.Location = new System.Drawing.Point(12, 57);
            this.labelIimpendingUpdatesDirectoryDate.Name = "labelIimpendingUpdatesDirectoryDate";
            this.labelIimpendingUpdatesDirectoryDate.Size = new System.Drawing.Size(223, 13);
            this.labelIimpendingUpdatesDirectoryDate.TabIndex = 0;
            this.labelIimpendingUpdatesDirectoryDate.Text = "Предстоящие даты обновления каталога: ";
            // 
            // labelСopyrightCompany
            // 
            this.labelСopyrightCompany.AutoSize = true;
            this.labelСopyrightCompany.Location = new System.Drawing.Point(12, 139);
            this.labelСopyrightCompany.Name = "labelСopyrightCompany";
            this.labelСopyrightCompany.Size = new System.Drawing.Size(94, 13);
            this.labelСopyrightCompany.TabIndex = 0;
            this.labelСopyrightCompany.Text = "Авторское право";
            // 
            // labelIimpendingUpdatesCarrierData
            // 
            this.labelIimpendingUpdatesCarrierData.AutoSize = true;
            this.labelIimpendingUpdatesCarrierData.Location = new System.Drawing.Point(12, 111);
            this.labelIimpendingUpdatesCarrierData.Name = "labelIimpendingUpdatesCarrierData";
            this.labelIimpendingUpdatesCarrierData.Size = new System.Drawing.Size(342, 13);
            this.labelIimpendingUpdatesCarrierData.TabIndex = 1;
            this.labelIimpendingUpdatesCarrierData.Text = "Предстоящие даты обновления \"Описания носителей датчиков\": ";
            // 
            // labelDateUpdateCarrierData
            // 
            this.labelDateUpdateCarrierData.AutoSize = true;
            this.labelDateUpdateCarrierData.Location = new System.Drawing.Point(12, 86);
            this.labelDateUpdateCarrierData.Name = "labelDateUpdateCarrierData";
            this.labelDateUpdateCarrierData.Size = new System.Drawing.Size(270, 13);
            this.labelDateUpdateCarrierData.TabIndex = 2;
            this.labelDateUpdateCarrierData.Text = "Дата обновления \"Описания носителей датчиков\": ";
            // 
            // InformationOnProgram
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 161);
            this.Controls.Add(this.labelIimpendingUpdatesCarrierData);
            this.Controls.Add(this.labelDateUpdateCarrierData);
            this.Controls.Add(this.labelСopyrightCompany);
            this.Controls.Add(this.labelIimpendingUpdatesDirectoryDate);
            this.Controls.Add(this.labelDateUpdateDirectory);
            this.Controls.Add(this.labelVersion);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "InformationOnProgram";
            this.Text = "Информация";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelDateUpdateDirectory;
        private System.Windows.Forms.Label labelIimpendingUpdatesDirectoryDate;
        private System.Windows.Forms.Label labelСopyrightCompany;
        private System.Windows.Forms.Label labelIimpendingUpdatesCarrierData;
        private System.Windows.Forms.Label labelDateUpdateCarrierData;
    }
}
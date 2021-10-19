namespace Diascan.Agent.ClientApp
{
    partial class InspectionsDirPath
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
            this.labInspectionsDirPath = new System.Windows.Forms.Label();
            this.tbInspectionsDirPath = new System.Windows.Forms.TextBox();
            this.btnInspectionsDirPath = new System.Windows.Forms.Button();
            this.btnСancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.errorProvider = new System.Windows.Forms.ErrorProvider(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).BeginInit();
            this.SuspendLayout();
            // 
            // labInspectionsDirPath
            // 
            this.labInspectionsDirPath.AutoSize = true;
            this.labInspectionsDirPath.Location = new System.Drawing.Point(12, 17);
            this.labInspectionsDirPath.Name = "labInspectionsDirPath";
            this.labInspectionsDirPath.Size = new System.Drawing.Size(34, 13);
            this.labInspectionsDirPath.TabIndex = 0;
            this.labInspectionsDirPath.Text = "Путь:";
            // 
            // tbInspectionsDirPath
            // 
            this.tbInspectionsDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbInspectionsDirPath.Location = new System.Drawing.Point(46, 14);
            this.tbInspectionsDirPath.Name = "tbInspectionsDirPath";
            this.tbInspectionsDirPath.Size = new System.Drawing.Size(421, 20);
            this.tbInspectionsDirPath.TabIndex = 1;
            // 
            // btnInspectionsDirPath
            // 
            this.btnInspectionsDirPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnInspectionsDirPath.FlatAppearance.BorderSize = 0;
            this.btnInspectionsDirPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Pixel, ((byte)(204)));
            this.btnInspectionsDirPath.Location = new System.Drawing.Point(472, 12);
            this.btnInspectionsDirPath.Name = "btnInspectionsDirPath";
            this.btnInspectionsDirPath.Size = new System.Drawing.Size(37, 25);
            this.btnInspectionsDirPath.TabIndex = 2;
            this.btnInspectionsDirPath.Text = "...";
            this.btnInspectionsDirPath.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnInspectionsDirPath.UseVisualStyleBackColor = true;
            this.btnInspectionsDirPath.Click += new System.EventHandler(this.btnInspectionsDirPath_Click);
            // 
            // btnСancel
            // 
            this.btnСancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnСancel.Location = new System.Drawing.Point(423, 50);
            this.btnСancel.Name = "btnСancel";
            this.btnСancel.Size = new System.Drawing.Size(75, 23);
            this.btnСancel.TabIndex = 4;
            this.btnСancel.Text = "Отмена";
            this.btnСancel.UseVisualStyleBackColor = true;
            this.btnСancel.Click += new System.EventHandler(this.btnСancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(342, 50);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 3;
            this.btnOK.Text = "ОК";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // errorProvider
            // 
            this.errorProvider.ContainerControl = this;
            // 
            // InspectionsDirPath
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(510, 85);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnСancel);
            this.Controls.Add(this.btnInspectionsDirPath);
            this.Controls.Add(this.tbInspectionsDirPath);
            this.Controls.Add(this.labInspectionsDirPath);
            this.Icon = global::Diascan.Agent.ClientApp.Properties.Resources.favicon_16x16;
            this.MaximumSize = new System.Drawing.Size(3840, 124);
            this.MinimumSize = new System.Drawing.Size(526, 124);
            this.Name = "InspectionsDirPath";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Укажите путь к инспекции";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InspectionsDirPath_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.errorProvider)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labInspectionsDirPath;
        private System.Windows.Forms.TextBox tbInspectionsDirPath;
        private System.Windows.Forms.Button btnInspectionsDirPath;
        private System.Windows.Forms.Button btnСancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}

using System.Drawing;
using DiagnosticInformationAnalyzer.Properties;


namespace DiagnosticInformationAnalyzer
{
    partial class DiagnosticInformationAnalyzerIcon
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DiagnosticInformationAnalyzerIcon));
            this.notifyIcon = new System.Windows.Forms.NotifyIcon(this.components);
            this.CMS = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tSMICompareHashesRunnerCRC32 = new System.Windows.Forms.ToolStripMenuItem();
            this.tSS = new System.Windows.Forms.ToolStripSeparator();
            this.tSMIExit = new System.Windows.Forms.ToolStripMenuItem();
            this.tSMICompareHashesRunnerMD5 = new System.Windows.Forms.ToolStripMenuItem();
            this.tSMICompareHashesRunnerSHA256 = new System.Windows.Forms.ToolStripMenuItem();
            this.CMS.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon
            // 
            this.notifyIcon.BalloonTipText = "Анализатор диагностической информации";
            this.notifyIcon.ContextMenuStrip = this.CMS;
            this.notifyIcon.Icon = Resources.favicon_16x16;
            this.notifyIcon.Text = "Анализатор диагностической информации";
            this.notifyIcon.Visible = true;
            // 
            // CMS
            // 
            this.CMS.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tSMICompareHashesRunnerCRC32,
            this.tSMICompareHashesRunnerMD5,
            this.tSMICompareHashesRunnerSHA256,
            this.tSS,
            this.tSMIExit});
            this.CMS.Name = "CMS";
            this.CMS.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.CMS.Size = new System.Drawing.Size(196, 98);
            // 
            // tSMICompareHashesRunnerCRC32
            // 
            this.tSMICompareHashesRunnerCRC32.Name = "tSMICompareHashesRunnerCRC32";
            this.tSMICompareHashesRunnerCRC32.Size = new System.Drawing.Size(195, 22);
            this.tSMICompareHashesRunnerCRC32.Text = "Сравнить хеш CRC32";
            // 
            // tSS
            // 
            this.tSS.Name = "tSS";
            this.tSS.Size = new System.Drawing.Size(192, 6);
            // 
            // tSMIExit
            // 
            this.tSMIExit.Name = "tSMIExit";
            this.tSMIExit.Size = new System.Drawing.Size(195, 22);
            this.tSMIExit.Text = "Выход";
            // 
            // tSMICompareHashesRunnerMD5
            // 
            this.tSMICompareHashesRunnerMD5.Name = "tSMICompareHashesRunnerMD5";
            this.tSMICompareHashesRunnerMD5.Size = new System.Drawing.Size(195, 22);
            this.tSMICompareHashesRunnerMD5.Text = "Сравнить хеш MD5";
            // 
            // tSMICompareHashesRunnerSHA256
            // 
            this.tSMICompareHashesRunnerSHA256.Name = "tSMICompareHashesRunnerSHA256";
            this.tSMICompareHashesRunnerSHA256.Size = new System.Drawing.Size(195, 22);
            this.tSMICompareHashesRunnerSHA256.Text = "Сравнить хеш SHA256";
            // 
            // DiagnosticInformationAnalyzerIcon
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(120, 0);
            this.Name = "DiagnosticInformationAnalyzerIcon";
            this.ShowInTaskbar = false;
            this.Text = "DiagnosticInformationAnalyzerIcon";
            this.CMS.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon;
        private System.Windows.Forms.ContextMenuStrip CMS;
        private System.Windows.Forms.ToolStripMenuItem tSMICompareHashesRunnerCRC32;
        private System.Windows.Forms.ToolStripSeparator tSS;
        private System.Windows.Forms.ToolStripMenuItem tSMIExit;
        private System.Windows.Forms.ToolStripMenuItem tSMICompareHashesRunnerMD5;
        private System.Windows.Forms.ToolStripMenuItem tSMICompareHashesRunnerSHA256;
    }
}


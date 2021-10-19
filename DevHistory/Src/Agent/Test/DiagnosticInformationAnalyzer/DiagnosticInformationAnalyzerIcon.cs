using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using Diascan.Agent.ComputeHashes;

namespace DiagnosticInformationAnalyzer
{
    public partial class DiagnosticInformationAnalyzerIcon : Form
    {
        private ComputeHashes computeHashes;

        public DiagnosticInformationAnalyzerIcon()
        {
            InitializeComponent();
            WindowState = FormWindowState.Minimized;
            Visible = false;
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000);
            
            computeHashes = new ComputeHashes();
            tSMIExit.Click                      += Stop;
            tSMICompareHashesRunnerCRC32.Click  += CompareHashesRunnerCRC32;
            tSMICompareHashesRunnerMD5.Click    += CompareHashesRunnerMD5;
            tSMICompareHashesRunnerSHA256.Click += CompareHashesRunnerSHA256;
        }

        private bool GetPathSelectingDirectory( out string path )
        {

            SHDocVw.ShellWindows shellWindows = new SHDocVw.ShellWindows();

            path = String.Empty;
            bool res = false;

            foreach ( SHDocVw.InternetExplorer windows in shellWindows )
            {
                string filename = Path.GetFileNameWithoutExtension( windows.FullName ).ToLower();

                if ( filename.Equals("explorer") )
                {
                    Shell32.FolderItems items = ( (Shell32.IShellFolderViewDual2)windows.Document ).SelectedItems();

                    foreach ( Shell32.FolderItem item in items )
                        if (Directory.Exists(item.Path))
                            if (!File.Exists(item.Path))
                                if (Directory.GetFiles(item.Path).Length > 0)
                                {
                                    path = item.Path;
                                    res = true;
                                }
                }
            }
            return res;
        }

        private void CompareHashesRunnerCRC32(Object obj, EventArgs eventArgs)
        {
            string path = String.Empty;

            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            if ( GetPathSelectingDirectory( out path ) )
                computeHashes.GetHashes( path, enHasheType.Crc32 );
            sWatch.Stop();
            Console.WriteLine("time :{0}", sWatch.ElapsedMilliseconds);
        }

        private void CompareHashesRunnerMD5(Object obj, EventArgs eventArgs)
        {
            string path = String.Empty;
            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();
            if ( GetPathSelectingDirectory( out path ) )
                computeHashes.GetHashes( path, enHasheType.Md5 );
            sWatch.Stop();
            Console.WriteLine("time :{0}", sWatch.ElapsedMilliseconds);
        }

        private void CompareHashesRunnerSHA256(Object obj, EventArgs eventArgs)
        {
            string path = String.Empty;

            Stopwatch sWatch = new Stopwatch();
            sWatch.Start();

            if ( GetPathSelectingDirectory( out path ) )
                computeHashes.GetHashes( path, enHasheType.Sha256 );

            sWatch.Stop();
            Console.WriteLine("time :{0}", sWatch.ElapsedMilliseconds);
        }

        /// <summary>
        /// Выход
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="eventArgs"></param>
        public void Stop(Object obj, EventArgs eventArgs)
        {
            Visible = false;
            notifyIcon.Visible = false;
            tSMIExit.Click                      -= Stop;
            tSMICompareHashesRunnerCRC32.Click  -= CompareHashesRunnerCRC32;
            tSMICompareHashesRunnerMD5.Click    -= CompareHashesRunnerMD5;
            tSMICompareHashesRunnerSHA256.Click -= CompareHashesRunnerSHA256;
            CMS.Dispose();
            notifyIcon.ContextMenuStrip.Dispose();
            MessageBox.Show("Indicator упал", "Собщение", MessageBoxButtons.OK, MessageBoxIcon.Information);
            notifyIcon.Dispose();
            Application.Exit();
        }
    }
}

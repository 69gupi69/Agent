using System;
using System.IO;
using System.Windows.Forms;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess.DataBaseAccess
{
    public class BaseDbAccess
    {
        protected LiteDatabase dataModelDatabase;
        protected string fullDbFilePath;

        public void DeleteDb()
        {
            if(File.Exists(fullDbFilePath))
                File.Delete(fullDbFilePath);
        }

        public void CloseConnection()
        {
            dataModelDatabase.Dispose();
        }

        protected void InitDataBase(string dbName)
        {
            fullDbFilePath = FullDbFilePath(dbName);
            dataModelDatabase = new LiteDatabase(fullDbFilePath);
        }

        private string FullDbFilePath(string dbName)
        {
            return string.Concat(Application.StartupPath, @"\", dbName);
        }
    }
}

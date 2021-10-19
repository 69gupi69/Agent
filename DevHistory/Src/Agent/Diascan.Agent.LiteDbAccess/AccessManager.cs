using System;
using System.Windows.Forms;
using Diascan.Agent.LiteDbAccess.DataBaseAccess;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class AccessManager
    {
        public CarrierDataModelAccess CarrierDataModelAccess { get; }
        public DiascanAgentAccess DiascanAgentAccess { get; }
        public DirectoryDataModelAccess DirectoryDataModelAccess { get; }


        public AccessManager()
        {
            CarrierDataModelAccess = new CarrierDataModelAccess();
            DiascanAgentAccess = new DiascanAgentAccess();
            DirectoryDataModelAccess = new DirectoryDataModelAccess();
        }
    }
}

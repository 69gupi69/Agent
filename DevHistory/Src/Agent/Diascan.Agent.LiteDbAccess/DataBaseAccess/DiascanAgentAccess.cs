using System;
using System.Windows.Forms;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess.DataBaseAccess
{
    public class DiascanAgentAccess : BaseDbAccess
    {
        public CalculationAccess CalculationAccess { private set; get; }

        public void OpenConnection()
        {
            InitDataBase(Resources.DiascanAgent);
            CalculationAccess = new CalculationAccess(dataModelDatabase);
        }
    }
}

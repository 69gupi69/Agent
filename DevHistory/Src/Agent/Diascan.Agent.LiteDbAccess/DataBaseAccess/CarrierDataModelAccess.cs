using System;
using System.Windows.Forms;
using Diascan.Agent.DirectoryDataModel;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess.DataBaseAccess
{
    public class CarrierDataModelAccess : BaseDbAccess
    {
        public CarrierAccess CarrierAccess { private set; get; }

        public void OpenConnection()
        {
            InitDataBase(Resources.CarrierData);
            CarrierAccess = new CarrierAccess(dataModelDatabase);
        }
    }
}

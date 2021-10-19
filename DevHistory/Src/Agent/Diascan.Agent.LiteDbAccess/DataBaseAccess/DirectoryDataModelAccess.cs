using System;
using System.Windows.Forms;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess.DataBaseAccess
{
    public class DirectoryDataModelAccess : BaseDbAccess
    {
        public ContractorAccess ContractorAccess { private set; get; }
        public ContractorRouteRefAccess ContractorRouteRefAccess { private set; get; }
        public PipelineAccess PipelineAccess { private set; get; }
        public RouteAccess RouteAccess { private set; get; }

        public void OpenConnection()
        {
            InitDataBase(Resources.DataModel);
            ContractorAccess = new ContractorAccess(dataModelDatabase);
            ContractorRouteRefAccess = new ContractorRouteRefAccess(dataModelDatabase);
            PipelineAccess = new PipelineAccess(dataModelDatabase);
            RouteAccess = new RouteAccess(dataModelDatabase);
        }
    }
}

using System;
using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class ContractorRouteRefAccess : BaseTableAccess<ContractorRouteRef>
    {
        public ContractorRouteRefAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<ContractorRouteRef>("ContractorRouteRef");
        }
    }
}

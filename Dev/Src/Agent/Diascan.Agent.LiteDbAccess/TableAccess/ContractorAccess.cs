using System;
using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class ContractorAccess : BaseTableAccess<Contractor>
    {
        public ContractorAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<Contractor>("Contractors");
        }
    }
}

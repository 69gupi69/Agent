using System;
using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using Diascan.Agent.Types;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class CarrierAccess : BaseTableAccess<CarrierData>
    {
        public CarrierAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<CarrierData>("CarrierData");
        }
    }
}

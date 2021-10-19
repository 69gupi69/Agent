using System;
using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class RouteAccess : BaseTableAccess<Route>
    {
        public RouteAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<Route>("Routes");
        }
    }
}

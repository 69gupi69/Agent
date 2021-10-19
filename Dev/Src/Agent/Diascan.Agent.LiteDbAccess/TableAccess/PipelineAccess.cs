using System;
using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class PipelineAccess : BaseTableAccess<Pipeline>
    {
        public PipelineAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<Pipeline>("Pipelines");
        }
    }
}

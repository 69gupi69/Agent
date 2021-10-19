using System.Collections.Generic;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.LiteDbAccess
{
    public class BaseTableAccess<T>
    {
        protected LiteCollection<T> data;
        protected LiteDatabase liteDatabase;

        protected BaseTableAccess(LiteDatabase liteDatabase)
        {
            this.liteDatabase = liteDatabase;
        }

        public IEnumerable<T> GetAll()
        {
            return data.FindAll();
        }
    }
}

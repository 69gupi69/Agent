using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.LiteDbAccess.Properties;
using LiteDB;
using System.Windows.Forms;
using Diascan.Agent.Types;

namespace Diascan.Agent.LiteDbAccess
{
    public class SessionAccess : BaseTableAccess<Session>
    {
        public SessionAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<Session>("Session");
        }

        public void Insert(Session session)
        {
            data.Insert(session);
        }

        public void Update(Session session)
        {
            lock (data)
                data.Update(session);
        }

        public Session GetCalculationById(int id)
        {
            return data.Find(q => q.Id == id).FirstOrDefault();
        }

        public Session GetCalculationByGlobalId(Guid globalId)
        {
            return data.Find(q => q.GlobalID == globalId).FirstOrDefault();
        }

        public void DeleteById(int id)
        {
            data.Delete(id);
            liteDatabase.Shrink();
        }

        public void DeleteByGlobalId(Guid globalId)
        {
            data.Delete(q => q.GlobalID == globalId);
            liteDatabase.Shrink();
        }
    }
}

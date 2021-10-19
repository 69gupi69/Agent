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
    public class CalculationAccess : BaseTableAccess<Calculation>
    {
        public CalculationAccess(LiteDatabase liteDatabase) : base(liteDatabase)
        {
            data = liteDatabase.GetCollection<Calculation>("Calculation");
        }

        public void Insert(Calculation calculation)
        {
            data.Insert(calculation);
        }

        public void Update(Calculation calculation)
        {
            lock (data)
                data.Update(calculation);
        }

        public Calculation GetCalculationById(int id)
        {
            return data.Find(q => q.Id == id).FirstOrDefault();
        }

        public Calculation GetCalculationByGlobalId(Guid globalId)
        {
            return data.Find(q => q.GlobalId == globalId).FirstOrDefault();
        }

        public void DeleteById(int id)
        {
            data.Delete(id);
            liteDatabase.Shrink();
        }

        public void DeleteByGlobalId(Guid globalId)
        {
            data.Delete(q => q.GlobalId == globalId);
            liteDatabase.Shrink();
        }
    }
}

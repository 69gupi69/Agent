using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diascan.Agent.ModelDB
{
    public class SelectiveMailingParameters
    {
        //Массив идентификаторов пользователей
        public Guid[] UserIds { get; set; }
        //Массив идентификаторов расчётов
        public Guid[] CalculationIds { get; set; } 
    }
}

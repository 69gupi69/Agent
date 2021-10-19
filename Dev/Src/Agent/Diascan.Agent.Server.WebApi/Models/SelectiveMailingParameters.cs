using System;

namespace Diascan.Agent.Server.WebApi.Models
{
    public class SelectiveMailingParameters
    {
        //Массив идентификаторов пользователей
        public Guid[] UserIds { get; set; }
        //Массив идентификаторов расчётов
        public Guid[] CalculationIds { get; set; }
    }
}

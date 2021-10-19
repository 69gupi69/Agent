using System;
using Newtonsoft.Json;

namespace Diascan.Agent.Types
{
    public class SessionUniHead
    {
        [JsonProperty("TotalCount")]
        public int TotalCount { get; set; }
    
        [JsonProperty("Id")]
        public Guid Id { get; set; }
    
        [JsonProperty("ContractorId")]
        public Guid ContractorId { get; set; }
    
        [JsonProperty("PipeLineId")]
        public Guid PipeLineId { get; set; }
    
        [JsonProperty("RouteId")]
        public Guid RouteId { get; set; }
    
        [JsonProperty("Name")]
        public string Name { get; set; }
        /// <summary>
        /// Имя учетной записи пользователя
        /// </summary>
        [JsonProperty("AccountUserName")]
        public string AccountUserName { get; set; }
        /// <summary>
        /// Имя компьютера
        /// </summary>
        [JsonProperty("ComputerName")]
        public string ComputerName { get; set; }
    
        [JsonProperty("ContractorName")]
        public string ContractorName { get; set; }
    
        [JsonProperty("PipeLineName")]
        public string PipeLineName { get; set; }
    
        [JsonProperty("RouteName")]
        public string RouteName { get; set; }
    
        [JsonProperty("ResponsibleWorkItem")]
        public string ResponsibleWorkItem { get; set; }
        /// <summary>
        /// Дата пропуска
        /// </summary>
        [JsonProperty("DateWorkItem")]
        public DateTime DateWorkItem { get; set; }
        /// <summary>
        /// Дата старта расчета данных
        /// </summary>
        [JsonProperty("StartDateDataCalculation")]
        public DateTime StartDateDataCalculation { get; set; }
    }
}

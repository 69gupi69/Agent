using System;
using Newtonsoft.Json;

namespace Diascan.Agent.Server.WebApi.Models
{
    public class UserHeaders
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; }

        [JsonProperty("FirstName")]
        public string FirstName { get; set; }

        [JsonProperty("SecondName")]
        public string SecondName { get; set; }

        [JsonProperty("LastName")]
        public string LastName { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("ContractorName")]
        public string ContractorName { get; set; }

        [JsonProperty("PositionName")]
        public string PositionName { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Diascan.Agent.ModelDB
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

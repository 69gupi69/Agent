using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Diascan.Agent.Types
{
    public class Analysis
    {
        public Guid Id { get; set; }
        public Calculation Calculation { get; set; }
        public DateTime Time { get; set; }
    }
}

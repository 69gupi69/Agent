using System;
using Diascan.Agent.Types.ModelCalculationDiagData;

namespace Diascan.Agent.Types
{
    public class Analysis
    {
        public Guid Id { get; set; }
        public Calculation Calculation { get; set; }
        public DateTime Time { get; set; }
    }
}

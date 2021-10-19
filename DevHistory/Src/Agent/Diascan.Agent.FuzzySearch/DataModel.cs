using System;

namespace Diascan.Agent.FuzzySearch
{
    public class DataModel
    {
        public Guid Id { get; set; }
        public string RouteName { get; set; }
        public Guid PipelineId { get; set; }
        public string PipelineName { get; set; }
        public Guid ContractorId { get; set; }
        public string ContractorName { get; set; }
        public float? DiameterMm { get; set; }
    }
}

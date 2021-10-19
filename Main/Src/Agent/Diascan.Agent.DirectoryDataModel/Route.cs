using System;

namespace Diascan.Agent.DirectoryDataModel
{
    public class Route
    {
        public Guid     Id { get; set; }
        public string   Name { get; set; }
        public Pipeline PipelineId { get; set; }
        public float?   DiameterMm { get; set; }
    }
}
using System;

namespace Diascan.Agent.DirectoryDataModel
{
    public class ContractorRouteRef
    {
        public Guid Id { get; set; }
        public Guid ContractorId { get; set; }
        public Guid RouteId { get; set; }
    }
}

using System;

namespace Diascan.Agent.DirectoryDataModel
{
    public class ContractorRouteRef
    {
        public Guid Id { get; set; }
        public Contractor ContractorId { get; set; }
        public Route RouteId { get; set; }
    }
}
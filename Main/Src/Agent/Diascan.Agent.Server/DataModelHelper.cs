using System;
using System.Collections.Generic;
using System.Linq;
using Diascan.Agent.DirectoryDataModel;
using LiteDB;

namespace Diascan.Agent.Manager
{
    public class DataModelHelper
    {
        private static LiteCollection<Contractor> contractorCollection;
        private static LiteCollection<ContractorRouteRef> contractorRouteRefCollection;
        private static LiteCollection<Pipeline> pipelineCollection;
        private static LiteCollection<Route> routeCollection;
        private static LiteDatabase dbDataModel;

        public DataModelHelper(string pathToExecuteFile)
        {
            dbDataModel = new LiteDatabase(pathToExecuteFile + @"\DirectoryDataModel.db");

            contractorCollection         = dbDataModel.GetCollection<Contractor>("Contractors");
            contractorRouteRefCollection = dbDataModel.GetCollection<ContractorRouteRef>("ContractorRouteRef");
            routeCollection              = dbDataModel.GetCollection<Route>("Routes");
            pipelineCollection           = dbDataModel.GetCollection<Pipeline>("Pipelines");

            BsonMapper.Global.Entity<ContractorRouteRef>()
                .DbRef(x => x.ContractorId, "Contractors")
                .DbRef(x => x.RouteId, "Routes");
            BsonMapper.Global.Entity<Route>()
                .DbRef(x => x.PipelineId, "Pipelines");
        }

        public Dictionary<Guid, string> GetAllCustomers()
        {

            return contractorCollection.FindAll().OrderBy(q => q.Name).ToDictionary(guid => guid.Id, name => name.Name);
        }

        public Dictionary<Guid, string> GetPipelines(Guid guidCustomer)
        {
            var pipelineList = new List<Pipeline>();
            var routesIdInContractorRouteRef = contractorRouteRefCollection.Find(q => q.ContractorId.Id == guidCustomer).Select(q => q.RouteId.Id);
            foreach (var eachId in routesIdInContractorRouteRef)
            {
                var pipelineId = routeCollection.Find(q => q.Id == eachId).FirstOrDefault()?.PipelineId;
                if (pipelineId == null) continue;
                var pipelines = pipelineCollection.Find(q => q.Id == pipelineId.Id);
                foreach (var pipeline in pipelines)
                    if (pipelineList.All(q => q.Id != pipeline.Id))
                        pipelineList.Add(pipeline);
            }
            return pipelineList.OrderBy(q => q.Name).ToDictionary(key => key.Id, name => name.Name);
        }

        public Dictionary<Guid, string> GetRoutes(Guid customerGuid, Guid pipelineGuid)
        {
            var routeList = new List<Route>();
            var routesIdInContractorRouteRef = contractorRouteRefCollection.Find(q => q.ContractorId.Id == customerGuid).Select(q => q.RouteId.Id);
            var routesIdInRoute = routeCollection.Find(q => q.PipelineId.Id == pipelineGuid).Select(q => q.Id);
            var routesIdCompareResult = routesIdInContractorRouteRef.Intersect(routesIdInRoute);
            foreach (var routeId in routesIdCompareResult)
            {
                var route = routeCollection.Find(q => q.Id == routeId).FirstOrDefault();
                routeList.Add(route);
            }
            return routeList.OrderBy(q => q.Name).ToDictionary(key => key.Id, name => name.Name);
        }

        public float GetDiameter( Guid routeGuid )
        {
            var diameterMm = routeCollection.Find(q => q.Id == routeGuid).FirstOrDefault().DiameterMm;
            if (diameterMm != null)
                return diameterMm.Value;
            return float.NaN;
        }

        public void Dispose()
        {
            dbDataModel.Dispose();
        }
    }
}

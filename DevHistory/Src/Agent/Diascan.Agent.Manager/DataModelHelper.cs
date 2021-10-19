using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Threading.Tasks;
using Diascan.Agent.DirectoryDataModel;
using Diascan.Agent.FuzzySearch;
using Diascan.Agent.LiteDbAccess;
using Diascan.Agent.Manager.Properties;

namespace Diascan.Agent.Manager
{
    public class DataModelHelper : IDisposable
    {
        private AccessManager accessManager;
        public DataModelHelper(AccessManager accessManager)
        {
            this.accessManager = accessManager;
            this.accessManager.DirectoryDataModelAccess.OpenConnection();
        }

        public DataModel[] GetAllDataModel()
        {
            var contractorCol = accessManager.DirectoryDataModelAccess.ContractorAccess.GetAll().ToArray();
            var contractorRouteRefCol = accessManager.DirectoryDataModelAccess.ContractorRouteRefAccess.GetAll().ToArray();
            var pipelineCol = accessManager.DirectoryDataModelAccess.PipelineAccess.GetAll().ToArray();
            var routes = accessManager.DirectoryDataModelAccess.RouteAccess.GetAll().ToArray();

            var dataModelCollection = new DataModel[routes.Length];
            Parallel.For(0, routes.Length, new ParallelOptions() { MaxDegreeOfParallelism = 3 }, index =>
            {
                var route = routes[index];
                var pipeline = pipelineCol.FirstOrDefault(q => q.Id == route.PipelineId) ?? new Pipeline(route.PipelineId, "Трубопровод не найден!");

                var contractorRouteRef = contractorRouteRefCol.FirstOrDefault(q => q.RouteId == route.Id);

                var contractor = contractorRouteRef == null
                    ? new Contractor(Guid.Empty, "Заказчик не найден!")
                    : contractorCol.First(q => q.Id == contractorRouteRef.ContractorId);
                dataModelCollection[index] = new DataModel()
                {
                    Id = route.Id,
                    RouteName = route.Name,
                    PipelineId = pipeline.Id,
                    PipelineName = pipeline.Name,
                    ContractorId = contractor.Id,
                    ContractorName = contractor.Name,
                    DiameterMm = route.DiameterMm,
                };
            });
            return dataModelCollection;
        }

        public float[] GetAllDiameters()
        {
            var diameters = Diameters().Distinct().ToArray();
            Array.Sort(diameters);
            return diameters;
        }

        private IEnumerable<float> Diameters()
        {
            var routes = accessManager.DirectoryDataModelAccess.RouteAccess.GetAll().ToArray();
            foreach (var route in routes)
            {
                if (route.DiameterMm == null) continue;
                yield return route.DiameterMm.Value;
            }
        }


        public void Dispose()
        {
            accessManager.DirectoryDataModelAccess.CloseConnection();
        }
    }
}

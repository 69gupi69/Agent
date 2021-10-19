using System.Collections.Generic;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;


namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    public interface IDiagData
    {
         bool State { get; set; }
         DataType DataType { get; set; }
         int SensorCount { get; set; }
         int NumberSensorsBlock { get; set; }
         double ProcessedDist { get; set; }
        double MaxDistance { get; set; }
        double DistanceLength { get; set; }
        double AreaLdi { get; set; }
        [JsonIgnore]
        double AreaContiguous { get; set; }
        double StartDist { get; set; }
        double StopDist { get; set; }
        Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
        [JsonIgnore]
        List<int> SensorsСontiguous { get; set; }
        List<OverSpeedInfo> SpeedInfos { get; set; }
        List<FileHashed> Files { get; set; }
        Dictionary<int, List<SensorRange>> HaltingSensors { get; set; }
        Dictionary<Range<double>, List<int>> ResultSensorDistances { get; set; }
    }
}

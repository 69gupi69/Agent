using System.Collections.Generic;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    public class DiagData : IDiagData
    {
        #region Property Interface
        public bool State { get; set; }
        public DataType DataType { get; set; }
        public int SensorCount { get; set; }
        public int NumberSensorsBlock { get; set; }
        public double ProcessedDist { get; set; }
        public double MaxDistance { get; set; }
        public double DistanceLength { get; set; }
        public double AreaLdi { get; set; }
        [JsonIgnore]
        public double AreaContiguous { get; set; }
        public double StartDist { get; set; }
        public double StopDist { get; set; }
        public Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
        [JsonIgnore]
        public List<int> SensorsСontiguous { get; set; }
        public List<OverSpeedInfo> SpeedInfos { get; set; }
        public List<FileHashed> Files { get; set; }
        public Dictionary<int, List<SensorRange>> HaltingSensors { get; set; }
        public Dictionary<Range<double>, List<int>> ResultSensorDistances { get; set; }
        #endregion

        public DiagData() { }
        public DiagData(DataType dataType, FileHashed[] files)
        {
            DataType              = dataType;
            State                 = false;
            ProcessedDist         = float.MinValue;
            Files                 = new List<FileHashed>();
            SpeedInfos            = new List<OverSpeedInfo>();
            HaltingSensors        = new Dictionary<int, List<SensorRange>>();
            ResultSensorDistances = new Dictionary<Range<double>, List<int>>();
            Files.AddRange(files);
        }
    }
}

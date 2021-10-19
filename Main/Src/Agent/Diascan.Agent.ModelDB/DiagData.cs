using System.Collections.Generic;
using Diascan.NDT.Enums;

namespace Diascan.Agent.ModelDB
{
    public class DiagData
    {
        
        public bool     State           { get; set; }
        public DataType DataType        { get; set; }
        public int      SensorCount     { get; set; }
        public double   ProcessedDist   { get; set; }
        public double   MaxDistance     { get; set; }
        public double   DistanceLength  { get; set; }
        public double   AreaLdi         { get; set; }
        public double   StartDist       { get; set; }
        public double   EndDist         { get; set; }
        public Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
        public List<OverSpeedInfo>                  SpeedInfos            { get; set; }
        public List<File>                           Files                 { get; set; }
        public Dictionary<int, List<Range<double>>> HaltingSensors        { get; set; }
        public Dictionary<Range<double>, List<int>> ResultSensorDistances { get; set; }

        public DiagData() { }
        public DiagData(DataType dataType, List<File> files)
        {
            State                 = false;
            DataType              = dataType;
            ProcessedDist         = float.MinValue;
            Files                 = files;
            SpeedInfos            = new List<OverSpeedInfo>();
            HaltingSensors        = new Dictionary<int, List<Range<double>>>();
            ResultSensorDistances = new Dictionary<Range<double>, List<int>>();
        }
    }

    public class CdmDiagData : DiagData
    {
        public enCdmDirectionName DirectionName { get; set; }
        public int   Id         { get; set; }
        public float Angle      { get; set; }
        public float EntryAngle { get; set; }


        public CdmDiagData() { }

        public CdmDiagData(DataType dataType, List<File> files, int id, float angle, float entryAngle, enCdmDirectionName dirName): base(dataType, files)
        {
            DirectionName = dirName;
            Id            = id;
            Angle         = angle;
            EntryAngle    = entryAngle;
        }
    }
}

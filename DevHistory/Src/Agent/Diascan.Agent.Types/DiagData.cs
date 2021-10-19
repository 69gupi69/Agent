using System.Collections.Generic;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders.CDM;
using DiCore.Lib.NDT.DataProviders.CDPA;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.Types
{
    public class DiagData
    {
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

        public DiagData() { }
        public DiagData(DataType dataType, FileHashed[] files)
        {
            State = false;
            DataType = dataType;
            ProcessedDist = float.MinValue;
            Files = new List<FileHashed>();
            Files.AddRange(files);
            SpeedInfos = new List<OverSpeedInfo>();
            HaltingSensors = new Dictionary<int, List<SensorRange>>();
            ResultSensorDistances = new Dictionary<Range<double>, List<int>>();
        }
    }

    public class CdmDiagData : DiagData
    {
        public enCdmDirectionName DirectionName { get; set; }
        public int Id { get; set; }
        public float Angle { get; set; }
        public float EntryAngle { get; set; }


        public CdmDiagData() { }

        public CdmDiagData(DataType dataType, FileHashed[] files, int id, float angle, float entryAngle, enCdmDirectionName dirName) : base(dataType, files)
        {
            DirectionName = dirName;
            Id = id;
            Angle = angle;
            EntryAngle = entryAngle;
        }

        public CdmDirection ToCdmDirection()
        {
            return new CdmDirection
            {
                Angle = this.Angle,
                EntryAngle = this.EntryAngle,
                DirectionName = (enDirectionName)this.DirectionName,
                Id = this.Id
            };
        }

        public DataTypesExt DirectionName2DataTypesExt()
        {
            switch (DirectionName)
            {
                case enCdmDirectionName.Cdc: return DataTypesExt.Cdc;
                case enCdmDirectionName.Cdf: return DataTypesExt.Cdf;
                case enCdmDirectionName.Cdg: return DataTypesExt.Cdg;
                case enCdmDirectionName.Cdh: return DataTypesExt.Cdh;
                case enCdmDirectionName.Cdl: return DataTypesExt.Cdl;
                case enCdmDirectionName.Cds: return DataTypesExt.Cds;
                default:                     return DataTypesExt.None;
            }
        }
    }

    public class CDpaDiagData : DiagData
    {
        public enCdmDirectionName DirectionName { get; set; }
        public int Id { get; set; }
        public float Angle { get; set; }
        public float EntryAngle { get; set; }


        public CDpaDiagData() { }

        public CDpaDiagData(DataType dataType, FileHashed[] files, int id, float angle, float entryAngle, enCdmDirectionName dirName) : base(dataType, files)
        {
            DirectionName = dirName;
            Id = id;
            Angle = angle;
            EntryAngle = entryAngle;
        }

        public DataTypesExt DirectionName2DataTypesExt()
        {
            switch (DirectionName)
            {
                case enCdmDirectionName.Cdc: return DataTypesExt.Cdc;
                case enCdmDirectionName.Cdf: return DataTypesExt.Cdf;
                case enCdmDirectionName.Cdg: return DataTypesExt.Cdg;
                case enCdmDirectionName.Cdh: return DataTypesExt.Cdh;
                case enCdmDirectionName.Cdl: return DataTypesExt.Cdl;
                case enCdmDirectionName.Cds: return DataTypesExt.Cds;
                default: return DataTypesExt.None;
            }
        }

        public CDpaDirection ToCDpaDirection()
        {
            return new CDpaDirection
            {
                Angle = this.Angle,
                EntryAngle = this.EntryAngle,
                DirectionName = (enDirectionName)this.DirectionName,
                Id = this.Id
            };
        }
    }
}

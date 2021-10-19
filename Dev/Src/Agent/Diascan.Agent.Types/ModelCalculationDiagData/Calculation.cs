using System;
using System.Collections.Generic;
using System.Windows;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    public class Calculation
    {
        public Guid GlobalId { get; set; }
        [JsonIgnore]
        public int Id { get; set; }

        public DataLocation DataLocation { get; set; }
        public ReferenceInputData DataOutput { get; set; }
        [JsonIgnore]
        public enCalculationStateTypes State { get; set; }
        public NavigationInfo NavigationInfo { get; set; }
        public List<IDiagData> DiagDataList { get; set; }
        public List<Rect> Frames { get; set; }
        public bool FramesTypeCds { get; set; }
        public bool CdChange { get; set; }
        public bool IsNeedRestart { get; set; }

        [JsonIgnore]
        public enWorkState WorkState { get; set; }
        [JsonIgnore]
        public double CdTailDistProgress { get; set; } = double.MinValue;
        [JsonIgnore]
        public double ProgressHashes { get; set; } = 0.0d; // Програесс хешей
        [JsonIgnore]
        public double ProgressCdlTail { get; set; } = 0.0d; // Програесс "хвостов"
        [JsonIgnore]
        public double ProgressNavData { get; set; } = 0.0d;  // Програесс навигации
        [JsonIgnore]
        public List<CarrierData> Carriers { get; set; } // список носителей датчиков

        public Calculation()
        {
            DiagDataList   = new List<IDiagData>();
            Carriers       = new List<CarrierData>();
            NavigationInfo = new NavigationInfo();
            DataOutput     = new ReferenceInputData();
            Frames         = new List<Rect>();
        }
    }
}

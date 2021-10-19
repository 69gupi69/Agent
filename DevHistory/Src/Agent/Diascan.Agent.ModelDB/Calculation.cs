using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;

namespace Diascan.Agent.ModelDB
{
    //public class Calculation
    //{
    //    public Guid GlobalId { get; set; }
    //    [JsonIgnore]
    //    public int Id { get; set; }
    //    [JsonIgnore]                        
    //    public string SourcePath { get; set; }
    //    [JsonIgnore]                        
    //    public string OmniFilePath { get; set; }
    //    public ReferenceInputData DataOutput { get; set; }
    //    [JsonIgnore]
    //    public CalculationStateTypes State { get; set; }
    //    public NavigationInfo NavigationInfo { get; set; }
    //    public List<DiagData> DiagDataList { get; set; }
    //    public DateTime TimeAddCalculation { get; set; }
    //    public List<Rect> Frames { get; set; }
    //    public bool FramesTypeCds { get; set; }
    //    public bool CdChange { get; set; }
    //    public List<RestartCriterion> RestartReport { get; set; }
    //    [JsonIgnore]
    //    public enWorkState WorkState { get; set; }
    //    [JsonIgnore]
    //    public double CdTailDistProgress { get; set; } = double.MinValue;
    //    [JsonIgnore]
    //    public string ProgressHashes { get; set; } = "0%"; // Програесс хешей
    //    [JsonIgnore]
    //    public string ProgressCdlTail { get; set; } // Програесс "хвостов"
    //    [JsonIgnore]
    //    public double ProgressNavData { get; set; } = 0;  // Програесс навигации
    //    [JsonIgnore]
    //    public List<CarrierData> Carriers { get; set; } // список носителей датчиков

    //    public Calculation()
    //    {
    //        TimeAddCalculation = new DateTime();
    //        DiagDataList = new List<DiagData>();
    //        RestartReport = new List<RestartCriterion>();
    //        NavigationInfo = new NavigationInfo();
    //    }

    //    public Calculation(string path): this()
    //    {
    //        DataOutput = new ReferenceInputData();
    //        Frames = new List<Rect>();
    //        SourcePath = path;
    //    }
    //}
}

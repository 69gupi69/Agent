using System;
using System.Collections.Generic;
using Diascan.NDT.Enums;

namespace Diascan.Agent.Types
{
    public class AnalysisCalculation
    {
        public Dictionary<enCommonDataType, DiagDataMain> DiagDataMain { get; set; }
        public List<RestartCriterion> RestartReport { get; set; }
        public List<AnalysisType> AnalisysTypeCollection { get; set; }

        public AnalysisCalculation()
        {
            DiagDataMain = new Dictionary<enCommonDataType, DiagDataMain>();
            AnalisysTypeCollection = new List<AnalysisType>();
            RestartReport = new List<RestartCriterion>();
        }
    }


    public class AnalysisType
    {
        public double AreaFail { get; set; }
        public double AreaKpp { get; set; }
        public bool DoubleAngle { get; set; }
        public double AreaContiguous { get; set; }
        public int NumberSensorsBlock { get; set; }
        public DataType DataType { get; set; }
        public enCdmDirectionName CdmType { get; set; }
        public double AreaLdi { get; set; }
        public Dictionary<int, List<RowDataAllTypes>> SensorsСontiguous { get; set; }
        public List<RowDataAllTypes> RowDataSorted { get; set; }

        public AnalysisType()
        {
            SensorsСontiguous = new Dictionary<int, List<RowDataAllTypes>>();
            RowDataSorted = new List<RowDataAllTypes>();
        }
    }
}

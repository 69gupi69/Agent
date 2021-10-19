using System.Collections.Generic;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.Types
{
    public class DiagDataMain
    {
        public bool AreaType { get; set; }
        public double Area { get; set; }
        public string NameTypeData { get; set; }
        public List<SpeedInfos> SpeedInfos { get; set; }
        public Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
    }
}

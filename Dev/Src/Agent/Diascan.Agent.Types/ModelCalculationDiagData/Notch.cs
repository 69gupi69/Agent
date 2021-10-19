using System.Collections.Generic;

namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    /// <summary>
    /// Просечка сенсора
    /// </summary>
    public class Notch
    {
        /// <summary>
        /// Сенсор индекс
        /// </summary>
        public int SensorIndex { get; set; }
        /// <summary>
        /// Сканы
        /// </summary>
        public List<Scan> Scans { get; set; }
    }
}

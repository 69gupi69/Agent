using DiCore.Lib.NDT.DataProviders.EMA;

namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    /// <summary>
    /// Правило анализа датчика
    /// </summary>
    public class SensorAnalysisRule
    {
        /// <summary>
        /// B-Scan
        /// </summary>
        public EmaRuleEnum BScan { get; set; }

        /// <summary>
        /// Излучающие датчики
        /// </summary>
        public int EmittingSensor { get; set; }
    }
}

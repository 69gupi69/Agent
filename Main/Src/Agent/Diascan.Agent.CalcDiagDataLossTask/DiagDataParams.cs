using Diascan.NDT.Enums;

namespace Diascan.Agent.CalcDiagDataLossTask
{
    public class DiagDataParams
    {
        public double Distance { get; }
        public float LowerValue { get; }
        public float UpperValue { get; }
        public float AllowedSensorsError { get; }
        public int IgnoreAreasCount { get; set; }
        public float MinCdSignalCount { get; }
        public DataType DataType { get; }

        public DiagDataParams(DataType dataType, double distance, float lowerValue, float upperValue, float allowedSensorsError, int ignoreAreasCount, float minCdSignalCount = 0)
        {
            Distance = distance;
            LowerValue = lowerValue;
            UpperValue = upperValue;
            AllowedSensorsError = allowedSensorsError;
            DataType = dataType;
            IgnoreAreasCount = ignoreAreasCount;
            MinCdSignalCount = minCdSignalCount;
        }
    }

    public static class ConstDiagDataParams
    {
        private static readonly DiagDataParams wmParams = new DiagDataParams(DataType.Wm, 35d, 0.8f, 1.5f, 0.8f, 1);
        private static readonly DiagDataParams mflT1Params = new DiagDataParams(DataType.MflT1, 10d, 0.1f, 1.53f, 0.6f, 2);
        private static readonly DiagDataParams mflT11Params = new DiagDataParams(DataType.MflT11, 10d, 0.1f, 1.53f, 0.6f, 2);
        private static readonly DiagDataParams mflT3Params = new DiagDataParams(DataType.MflT3, 20d, 0.35f, 2f, 0.7f, 1);
        private static readonly DiagDataParams tfi4Params = new DiagDataParams(DataType.TfiT4, 20d, 0.3f, 1.8f, 0.7f, 1);
        private static readonly DiagDataParams tfi41Params = new DiagDataParams(DataType.TfiT41, 20d, 0.3f, 1.8f, 0.7f, 1);
        private static readonly DiagDataParams mpmParams = new DiagDataParams(DataType.Mpm, 35d, -2f, 4f, 0.3f, 1);
        private static readonly DiagDataParams cdParams = new DiagDataParams(DataType.Cdl | DataType.Cd360, 4d, 0.7f, 1.3f, 0.02f, 2, 2);

        public static DiagDataParams[] GetAllParams()
        {
            return new[]
            {
                wmParams,
                mflT1Params,
                mflT11Params,
                mflT3Params,
                tfi4Params,
                tfi41Params,
                mpmParams,
                cdParams
            };
        }
    }
}

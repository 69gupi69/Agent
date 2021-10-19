using System;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.CalcDiagDataLossTask
{
    /// <summary>
    /// Непрерывный участок расхождений по углу
    /// </summary>
    public class SolidSpotData
    {
        public bool                    IsSolidSpotAngelMoreStart; // вошли в непрерывного участок?
        public NavSolidSpotMetersTypes SolidSpotMetersType;       // Наличие непрерывных участков данных, критерий угол
        public NavSolidSpotAngleTypes  SolidSpotAngleType;        // Наличие непрерывных участков данных, критерий метор
        public int                     СounteDeltaPitchAngleSpot; // колличество расхождений по углу тангажа на участке
        public double                  AveDeltaPitchAngleSpot;     // среднее значение расхождений по углу 0.6 тангажа на участке
        public Range<float>            DiapasonDeltaAngle;        // начало и конец непреррывного участка расхождения по углу

        public SolidSpotData()
        {
            IsSolidSpotAngelMoreStart = false;
            SolidSpotMetersType       = NavSolidSpotMetersTypes.None;
            SolidSpotAngleType        = NavSolidSpotAngleTypes.None;
            DiapasonDeltaAngle        = new Range<float>();
            СounteDeltaPitchAngleSpot = 0;
            AveDeltaPitchAngleSpot    = 0;
        }
    }

    [Flags]
    public enum NavSolidSpotMetersTypes
    {
        None = 0,
        SolidExtentTwoMeters = 2, // непрерывных участков данных протяженностью от 2 м
        SolidExtentThreeMeters = 3, // непрерывных участков данных протяженностью от 3 м
        SolidExtentEightMeters = 6, // непрерывных участков данных протяженностью от 6 м
    }
    [Flags]
    public enum NavSolidSpotAngleTypes
    {
        None = 0,
        SolidExtentThreeTenthsAngle = 1, // непрерывных участков расхождения по углу > 0.3 градуса
        SolidExtentOneAngle = 2, // непрерывных участков расхождения по углу > 1 градусов
        SolidExtentThreeAngle = 3, // непрерывных участков расхождения по углу > 3 градусов
    }
}

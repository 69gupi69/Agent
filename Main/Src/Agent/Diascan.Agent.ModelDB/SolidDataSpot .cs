using System.Collections.Generic;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.ModelDB
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
}

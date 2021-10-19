using DiCore.Lib.NDT.DataProviders.NAV;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.CalcDiagDataLossTask
{
    public class NavCalcParams
    {
        public enNavType NavType;
        public double ScanTimeLengt;

        public NavCalcParamItem Last600Sec = new NavCalcParamItem(0);   //  600 секунд перед началом движения

        public NavCalcParamItem GravAccel = new NavCalcParamItem(0);  //  ускорение свободного падения
        public NavCalcParamItem LinearSpeed = new NavCalcParamItem(0);    //  линейная скорость
        public NavCalcParamItem EarthAngularSpeedRotation = new NavCalcParamItem(0);  //  Угловая скорость вращения земли

        public NavCalcParamItem DifferenceLatitudes = new NavCalcParamItem(0);    //  разница широт

        public NavCalcParamItem AccelSum = new NavCalcParamItem(0);   //  среднее ускорение
        public NavCalcParamItem AccelMax = new NavCalcParamItem(0);   //  максимальное ускорение

        public NavCalcParamItem AngularSpeedSum = new NavCalcParamItem(0);    //  средняя угловая скорость
        public NavCalcParamItem AngularSpeedMax = new NavCalcParamItem(0);    //  максимальная угловая скорость

        public NavCalcParamItem AverageRollAngle = new NavCalcParamItem(0);   //  средний угол крена
        public NavCalcParamItem AveragePitchAngle = new NavCalcParamItem(0);  //  средний угол тангажа

        public NavCalcParamItem MaxRollAngle = new NavCalcParamItem(0);   //  максимальный угол крена
        public NavCalcParamItem MaxPitchAngle = new NavCalcParamItem(0);  //  максимальный угол тангажа

        public NavCalcParamItem SquareDeviationAngularSpeedX = new NavCalcParamItem(0);   //  S{ωxj}
        public NavCalcParamItem SquareDeviationAngularSpeedY = new NavCalcParamItem(0);   //  S{ωyj}
        public NavCalcParamItem SquareDeviationAngularSpeedZ = new NavCalcParamItem(0);   //  S{ωzj}

        public NavCalcParamItem SquareDeviationAccelX = new NavCalcParamItem(0);  //  S{axj}
        public NavCalcParamItem SquareDeviationAccelY = new NavCalcParamItem(0);  //  S{ayj}
        public NavCalcParamItem SquareDeviationAccelZ = new NavCalcParamItem(0);  //  S{azj}

        public NavCalcParamItem PitchAngleAtMovement = new NavCalcParamItem(0); // расхождений по углу тангажа, > 4°
        public NavCalcParamItem PitchAngle6 = new NavCalcParamItem(0); // > 0,3°
        public NavCalcParamItem PitchAngle3 = new NavCalcParamItem(0); // > 1°
        public NavCalcParamItem PitchAngle2 = new NavCalcParamItem(0); // > 3°
    }

    public class NavCalcParamItem
    {
        public float Value { get; }
        public float Threshold { get; }

        public NavCalcParamItem(float value, float threshold)
        {
            Value = value;
            Threshold = threshold;
        }
        public NavCalcParamItem(float value)
        {
            Value = value;
            Threshold = 0;
        }
    }

    public static class ConstNavCalcParams
    {
        private static readonly NavCalcParams binsParams = 
            new NavCalcParams()
            {
                NavType = enNavType.Bins,
                ScanTimeLengt = 0.005,
                Last600Sec = new NavCalcParamItem(600),
                GravAccel = new NavCalcParamItem(9.8f, 0.3f),
                LinearSpeed = new NavCalcParamItem(0f, 0.002f),
                SquareDeviationAngularSpeedX = new NavCalcParamItem(0.05f),
                SquareDeviationAngularSpeedY = new NavCalcParamItem(0.05f),
                SquareDeviationAngularSpeedZ = new NavCalcParamItem(0.05f),
                SquareDeviationAccelX = new NavCalcParamItem(0.4f),
                SquareDeviationAccelY = new NavCalcParamItem(0.4f),
                SquareDeviationAccelZ = new NavCalcParamItem(0.4f),
                EarthAngularSpeedRotation = new NavCalcParamItem(0.0045f, 0.0015f),
                DifferenceLatitudes = new NavCalcParamItem(7f),
                AccelSum = new NavCalcParamItem(12.5f, 3.0f),
                AccelMax = new NavCalcParamItem(300f),
                AngularSpeedSum = new NavCalcParamItem(35f),
                AngularSpeedMax = new NavCalcParamItem(600f),
                AverageRollAngle = new NavCalcParamItem(3.5f),
                AveragePitchAngle = new NavCalcParamItem(2.4f),
                MaxRollAngle = new NavCalcParamItem(90f),
                MaxPitchAngle = new NavCalcParamItem(90f),
                PitchAngleAtMovement = new NavCalcParamItem(4f),
                PitchAngle6 = new NavCalcParamItem(0.3f, 0.6f),
                PitchAngle3 = new NavCalcParamItem(1f, 2f),
                PitchAngle2 = new NavCalcParamItem(3f, 5f)
            };

            private static readonly NavCalcParams adisParams =
            new NavCalcParams()
            {
                NavType = enNavType.Adis,
                ScanTimeLengt = 0.005,
                GravAccel = new NavCalcParamItem(10.0f, 0.8f),
                LinearSpeed = new NavCalcParamItem(0f, 0.002f),
                SquareDeviationAngularSpeedX = new NavCalcParamItem(0.7f),
                SquareDeviationAngularSpeedY = new NavCalcParamItem(0.7f),
                SquareDeviationAngularSpeedZ = new NavCalcParamItem(0.7f),
                SquareDeviationAccelX = new NavCalcParamItem(0.6f),
                SquareDeviationAccelY = new NavCalcParamItem(0.6f),
                SquareDeviationAccelZ = new NavCalcParamItem(0.6f),
                EarthAngularSpeedRotation = new NavCalcParamItem(0.5f),
                AccelSum = new NavCalcParamItem(12.2f, 3.8f),
                AccelMax = new NavCalcParamItem(300f),
                AngularSpeedSum = new NavCalcParamItem(45f),
                AngularSpeedMax = new NavCalcParamItem(600f),
                PitchAngleAtMovement = new NavCalcParamItem(4f)
            };
        //  BEP
        private static readonly NavCalcParams bepParams =
            new NavCalcParams()
            {
                NavType = enNavType.Bep,
                ScanTimeLengt = 0.1,
                GravAccel = new NavCalcParamItem(10.0f, 0.8f),
                LinearSpeed = new NavCalcParamItem(0f, 0.002f),
                SquareDeviationAngularSpeedX = new NavCalcParamItem(0f),
                SquareDeviationAngularSpeedY = new NavCalcParamItem(0.7f),
                SquareDeviationAngularSpeedZ = new NavCalcParamItem(0.7f),
                SquareDeviationAccelX = new NavCalcParamItem(0.6f),
                SquareDeviationAccelY = new NavCalcParamItem(0.6f),
                SquareDeviationAccelZ = new NavCalcParamItem(0.6f),
                EarthAngularSpeedRotation = new NavCalcParamItem(0.5f),
                AccelSum = new NavCalcParamItem(12.2f, 3.3f), 
                AccelMax = new NavCalcParamItem(300f),
                AngularSpeedSum = new NavCalcParamItem(35f),
                AngularSpeedMax = new NavCalcParamItem(600f),
                PitchAngleAtMovement = new NavCalcParamItem(4f)
            };

        public static NavCalcParams[] GetAllParams()
        {
            return new []
            {
                binsParams,
                adisParams,
                bepParams
            };
        }
    }
}

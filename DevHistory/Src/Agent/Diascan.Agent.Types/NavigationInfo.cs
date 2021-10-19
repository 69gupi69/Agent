using DiCore.Lib.NDT.DataProviders.NAV;

namespace Diascan.Agent.Types
{
    public class NavigationInfo
    {
        public enNavType NavType { get; set; }
        public NavigationStateTypes State { get; set; }
        public enNavCalcStateTypes NavCalcState { get; set; }

        public double LinearSpeed { get; set; }
        /// <summary>
        /// S{ωxj}
        /// </summary>
        public double SquareDeviationAccelX { get; set; }
        /// <summary>
        /// S{ωyj}
        /// </summary>
        public double SquareDeviationAccelY { get; set; }
        /// <summary>
        /// S{ωzj}
        /// </summary>
        public double SquareDeviationAccelZ { get; set; }
        /// <summary>
        /// S{axj}
        /// </summary>
        public double SquareDeviationAngularSpeedX { get; set; }
        /// <summary>
        /// S{ayj}
        /// </summary>
        public double SquareDeviationAngularSpeedY { get; set; }
        /// <summary>
        /// S{azj}
        /// </summary>
        public double SquareDeviationAngularSpeedZ { get; set; }
        /// <summary>
        /// g значение ускорения свободного падения на участке выставки в течение последней минуты перед началом движения
        /// </summary>
        public double GravitationalAcceleration { get; set; }
        /// <summary>
        /// ωₒ значения угловой скорости вращения Земли на участке выставки в течение последней минуты перед началом движения
        /// </summary>
        public double EarthAngularSpeedRotation { get; set; }
        /// <summary>
        /// |ψ - ψₒ| ψ - значения широты на участке выставки в течение последней минуты перед началом движения, ψₒ - истинная широта камеры пуска ВИП
        /// </summary>
        public double DifferenceLatitudes { get; set; }
        /// <summary>
        /// ac значение обобщенного ускорения в пропуске
        /// </summary>
        public double AccelSum { get; set; }
        /// <summary>
        /// am значение обобщенного ускорения в пропуске
        /// </summary>
        public double AccelMax { get; set; }
        /// <summary>
        /// ω c значение обобщенной угловой скорости в пропуске
        /// </summary>
        public double AngularSpeedSum { get; set; }
        /// <summary>
        /// ω m значение обобщенной угловой скорости в пропуске
        /// </summary>
        public double AngularSpeedMax { get; set; }
        /// <summary>
        /// Yc среднее значение расхождения по углу крена в пропуске
        /// </summary>
        public double AverageRollAngle { get; set; }
        /// <summary>
        /// Тета с среднее значение расхождения по углу тангажа в пропуске
        /// </summary>
        public double AveragePitchAngle { get; set; }
        /// <summary>
        /// Ym максимальное значение расхождения по углу крена в пропуске
        /// </summary>
        public double MaxRollAngle { get; set; }
        /// <summary>
        /// Тета m максимальное значение расхождения по углу тангажа в пропуске
        /// </summary>
        public double MaxPitchAngle { get; set; }
        public int PitchAngleAtMovement { get; set; } // CountDeltaTetaj колличество расхождений по углу 4
        public int PitchAngleSection6 { get; set; } // CountDeltaTetaj колличество расхождений по углу 0.3 тангажа на участке от 6
        public int PitchAngleSection3 { get; set; } // CountDeltaTetaj колличество расхождений по углу 1 тангажа на участке от 3
        public int PitchAngleSection2 { get; set; } // CountDeltaTetaj колличество расхождений по углу 3 тангажа на участке от 2
        /// <summary>
        /// Время выставки снаряда в трубе в секундах
        /// </summary>
        public double NavSetupTime { get; set; }
    }
}

using DiCore.Lib.NDT.DataProviders.NAV;

namespace Diascan.Agent.ModelDB
{
    public class NavigationInfo
    {
        public enNavType            NavType           {get; set;}
        public NavigationStateTypes NavigationState   {get; set; } //нужно добавить расхождения значений углов
        public NavCalcStateTypes NavCalcState { get; set; }

        public double LinearSpeed                      {get; set;}
        public double SquareDeviationAccelX            {get; set;} //  S{ωxj}
        public double SquareDeviationAccelY            {get; set;} //  S{ωyj}
        public double SquareDeviationAccelZ            {get; set;} //  S{ωzj}
        public double SquareDeviationAngularSpeedX     {get; set;} //  S{axj}
        public double SquareDeviationAngularSpeedY     {get; set;} //  S{ayj}
        public double SquareDeviationAngularSpeedZ     {get; set;} //  S{azj}
        public double GravitationalAcceleration        {get; set;} // g значение ускорения свободного падения на участке выставки в течение последней минуты перед началом движения
        public double EarthAngularSpeedRotation        {get; set;} // ωₒ значения угловой скорости вращения Земли на участке выставки в течение последней минуты перед началом движения
        public double DifferenceLatitudes              {get; set;} // |ψ - ψₒ| ψ - значения широты на участке выставки в течение последней минуты перед началом движения, ψₒ - истинная широта камеры пуска ВИП
        public double AccelSum                 {get; set;} // ac значение обобщенного ускорения в пропуске
        public double AccelMax                  {get; set;} // am значение обобщенного ускорения в пропуске
        public double AngularSpeedSum                 {get; set;} // ω c значение обобщенной угловой скорости в пропуске
        public double AngularSpeedMax                  {get; set;} // ω m значение обобщенной угловой скорости в пропуске
        public double AverageRollAngle                {get; set;} // Yc среднее значение расхождения по углу крена в пропуске
        public double AveragePitchAngle               {get; set;} // Тета с среднее значение расхождения по углу тангажа в пропуске
        public double MaxRollAngle                {get; set;} // Ym максимальное значение расхождения по углу крена в пропуске
        public double MaxPitchAngle               {get; set;} // Teta m максимальное значение расхождения по углу тангажа в пропуске
        public int PitchAngleAtMovement { get; set;} // CountDeltaTetaj колличество расхождений по углу 4
        public int PitchAngleSection6 { get; set;} // CountDeltaTetaj колличество расхождений по углу 0.3 тангажа на участке от 6
        public int PitchAngleSection3 { get; set;} // CountDeltaTetaj колличество расхождений по углу 1 тангажа на участке от 3
        public int PitchAngleSection2 { get; set;} // CountDeltaTetaj колличество расхождений по углу 3 тангажа на участке от 2
        public double NavSetupTime { get; set; }    // Время выставки снаряда в трубе в секундах
    }
}

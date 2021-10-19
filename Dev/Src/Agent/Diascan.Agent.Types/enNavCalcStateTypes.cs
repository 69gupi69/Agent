using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum enNavCalcStateTypes
    {
        StandardDeviations = 1 << 0, // среднеквадратические отклонения угловых скоростей и ускорений
        GravAccel = 1 << 1, // ускорение свободного падения
        EarthAngularSpeedRotation = 1 << 2, //  угловая скорость вращения земли
        DiffLatitude = 1 << 3, // |ψ-ψₒ|
        AccelSum = 1 << 4, // среднее ускорение
        AccelMax = 1 << 5, // максимальное ускорение
        AngularSpeedSum = 1 << 6, // средняя угловая скорость
        AngularSpeedMax = 1 << 7, // максимальная угловая скорость
        AverageRollPitchAngle = 1 << 8, // среднее значение расхождения по углу крена и тангажа в пропуске
        MaxRollPitchAngle = 1 << 9, // максимальное значение расхождения по углу крена и тангажа в пропуске
        PitchAngleAtMovement = 1 << 10, // значения расхождений по углу крена и тангажа в пропуске для j-го значения элементов промежуточных НД
        WrongNavSetupTime = 1 << 11 // контроль неприрывного времени выставки (t>=600)
    }
}

using System;

namespace Diascan.Agent.ModelDB
{
    [Flags]
    public enum CalculationStateTypes
    {
        None = 0,   //  
        FindTypes = 1 << 0,
        Hashed = 1 << 1,   //   Производится хеширование
        Speed = 1 << 2,   //   Производится хеширование
        Calculated = 1 << 3,  //    Производится расчет
        CdlTail = 1 << 4,  //   Производится расчет
        DefinitionOfRerun = 1 << 5, //  определение перепуска ВИП
        Transferred = 1 << 6,   //   Произведена передача данных
    }

    [Flags]
    public enum NavigationStateTypes
    {
        None = 0,
        NavigationData = 1 << 0, //  Наличие данных и этапа настройки навигации 
        CalcNavigation = 1 << 1,   //   Расчет по данным навигации
    }

    [Flags]
    public enum NavCalcStateTypes
    {
        StandardDeviations = 1 << 0, // среднеквадратические отклонения угловых скоростей и ускорений
        GravAccel = 1 << 1, //  ускорение свободного падения
        EarthAngularSpeedRotation = 1 << 2, //  угловая скорость вращения земли
        DiffLatitude = 1 << 3,  //  |ψ-ψₒ|
        AccelSum = 1 << 4,  //  среднее ускорение
        AccelMax = 1 << 5,  //  максимальное ускорение
        AngularSpeedSum = 1 << 6,   //  средняя угловая скорость
        AngularSpeedMax = 1 << 7,   //  максимальная угловая скорость
        AverageRollPitchAngle = 1 << 8, //  среднее значение расхождения по углу крена и тангажа в пропуске
        MaxRollPitchAngle = 1 << 9, //  максимальное значение расхождения по углу крена и тангажа в пропуске
        PitchAngleAtMovement = 1 << 10,  //  значения расхождений по углу крена и тангажа в пропуске для j-го значения элементов промежуточных НД
        WrongNavSetupTime = 1 << 11  //  контроль неприрывного времени выставки (t>=600)
    }

    [Flags]
    public enum NavSolidSpotMetersTypes
    {
        None                   = 0,
        SolidExtentTwoMeters   = 2, // непрерывных участков данных протяженностью от 2 м
        SolidExtentThreeMeters = 3, // непрерывных участков данных протяженностью от 3 м
        SolidExtentEightMeters = 6, // непрерывных участков данных протяженностью от 6 м
    }
    [Flags]
    public enum NavSolidSpotAngleTypes
    {
        None                        = 0,
        SolidExtentThreeTenthsAngle = 1, // непрерывных участков расхождения по углу > 0.3 градуса
        SolidExtentOneAngle         = 2, // непрерывных участков расхождения по углу > 1 градусов
        SolidExtentThreeAngle       = 3, // непрерывных участков расхождения по углу > 3 градусов
    }
}

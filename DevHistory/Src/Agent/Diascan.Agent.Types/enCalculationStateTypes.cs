using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum enCalculationStateTypes : long
    {
        None = 0,
        FindTypes = 1 << 0, // Поиск типов данных
        Hashe = 1 << 1, // Производится хеширование
        OverSpeed = 1 << 2, // Производится хеширование
        HaltingSensors = 1 << 3, // Производится расчет
        CdlTail = 1 << 4, // Производится расчет
        SplitDataTypeRange = 1 << 5, // Разделение ПДФ на ЛЧ и КПП
        Analysis = 1 << 6, // определение перепуска ВИП
        Sended = 1 << 7, // Произведена передача данных
    }
}

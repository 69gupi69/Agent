using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum NavigationStateTypes
    {
        None = 0,
        NavigationData = 1 << 0, // Наличие данных и этапа настройки навигации 
        CalcNavigation = 1 << 1, // Расчет по данным навигации
    }
}

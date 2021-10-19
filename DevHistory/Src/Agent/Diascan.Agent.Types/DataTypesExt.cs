using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum DataTypesExt
    {
        None = 0,
        Spm = 1 << 1,
        Mpm = 1 << 2,
        Wm = 1 << 3,
        MflT1 = 1 << 4,
        MflT11 = 1 << 5,
        MflT2 = 1 << 6,
        MflT22 = 1 << 7,
        MflT3 = 1 << 8,
        MflT31 = 1 << 9,
        MflT32 = 1 << 10,
        MflT33 = 1 << 11,
        MflT34 = 1 << 12,
        TfiT4 = 1 << 13,
        TfiT41 = 1 << 14,
        Cd360 = 1 << 15,
        Cdc = 1 << 16,
        Cds = 1 << 17,
        Cdh = 1 << 18,
        Cdl = 1 << 19,
        Cdg = 1 << 20,
        Cdf = 1 << 21,
        Ema = 1 << 22,
        CDPA = 1 << 23
    }
}

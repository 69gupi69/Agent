using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum enCommonDataType
    {
        None = 0,
        Spm = 1,
        Mpm = 2,
        Wm = 4,
        Mfl = 8,
        Cd = 16,
        Ema = 32
    }
}

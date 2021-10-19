using System;

namespace Diascan.Agent.Types
{
    [Flags]
    public enum enWorkState
    {
        None = 0,
        Sucsess = 1 << 0,
        Error = 1 << 1
    }
}

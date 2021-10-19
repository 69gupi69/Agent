﻿
namespace Diascan.Agent.ModelDB
{
    public class OverSpeedInfo
    {
        public Range<double> Distance { get; set; }
        public float Speed { get; set; }
        public double Area { get; set; }

        public OverSpeedInfo()
        {
            Distance = new Range<double>(0, 0);
        }
    }
}

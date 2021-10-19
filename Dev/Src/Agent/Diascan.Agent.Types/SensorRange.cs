namespace Diascan.Agent.Types
{
    public struct SensorRange
    {
        public double Begin { get; set; }
        public double End { get; set; }
        public double Area { get; set; }
        public enSensorRangeType RangeType { get; set; }
        public enPipeType PipeType { get; set; }
    }
}

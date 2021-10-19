namespace Diascan.Agent.ModelDB
{
    public class Frame
    {
        public float AngleBeg { get; set; }
        public float AngleEnd { get; set; }
        public double DistStart { get; set; }
        public double DistStop { get; set; }
        public double AngleCount => AngleEnd - AngleBeg;
        public double DistLength => DistStop - DistStart;

        public Frame() { }

        public Frame(float angleBeg, float angleEnd, double distStart, double distStop)
        {
            AngleBeg = angleBeg;
            AngleEnd = angleEnd;
            DistStart = distStart;
            DistStop = distStop;
        }
    }
}

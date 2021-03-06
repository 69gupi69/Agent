namespace Diascan.Agent.Types
{
    public class CarrierData
    {
        public bool Change { get; set; }
        public DataTypesExt Type { get; set; }
        public int Id { get; set; }
        public int Sensorcount { get; set; }
        public int CarrierDiameter { get; set; }
        public int NumberSensorsBlock { get; set; }
        public double SpeedMin { get; set; }
        public double SpeedMax { get; set; }
        public string Defectoscope { get; set; }
    }
}

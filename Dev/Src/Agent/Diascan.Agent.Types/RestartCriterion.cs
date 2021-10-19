namespace Diascan.Agent.Types
{
    public class RestartCriterion
    {
        public string DataType { get; set; }
        public string Criterion { get; set; }

        public RestartCriterion() { }

        public RestartCriterion(string dataType, string criterion)
        {
            DataType = dataType;
            Criterion = criterion;
        }
    }
}

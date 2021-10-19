using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Diascan.Agent.ModelDB;

namespace Diascan.Agent.Manager
{
    public class ControllerHelper
    {
        public Dictionary<enCdmDirectionName, List<DiagData>> GroupDiagDataByType(List<DiagData> diagDataList)
        {
            var result = new Dictionary<enCdmDirectionName, List<DiagData>>();
            foreach (var diagData in diagDataList)
            {
                if (diagData is CdmDiagData cdmDiagData)
                {
                    var dirName = cdmDiagData.DirectionName;

                    if (!result.ContainsKey(dirName))
                        result.Add(dirName, new List<DiagData>());

                    result[dirName].Add(cdmDiagData);
                }
                else
                {
                    if (!result.ContainsKey(enCdmDirectionName.None))
                        result.Add(enCdmDirectionName.None, new List<DiagData>());

                    result[enCdmDirectionName.None].Add(diagData);
                }
            }

            return result;
        }

        public List<int> GetCarrierId(string omniPath)
        {
            var xOmni = XDocument.Load(omniPath);
            var carriersCollection = xOmni.Element("RUN")?.Descendants().Select(q => q.Attribute("CARRIER_ID")?.Value);
            return (from carrier in carriersCollection where carrier != null select int.Parse(carrier)).ToList();
        }
    }
}
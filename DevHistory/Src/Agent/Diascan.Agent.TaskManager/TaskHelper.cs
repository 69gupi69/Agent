using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Diascan.Agent.Types;
using Diascan.NDT.Enums;

namespace Diascan.Agent.TaskManager
{
    public static class TaskHelper
    {
        public static Dictionary<enCdmDirectionName, List<DiagData>> GroupDiagDataByType(List<DiagData> diagDataList)
        {
            var result = new Dictionary<enCdmDirectionName, List<DiagData>>();
            foreach (var diagData in diagDataList)
            {
                if (diagData.DataType == DataType.Nav) continue;
                if (diagData is CdmDiagData cdmDiagData)
                {
                    var dirName = cdmDiagData.DirectionName;

                    if (!result.ContainsKey(dirName))
                        result.Add(dirName, new List<DiagData>());

                    result[dirName].Add(cdmDiagData);
                } else if (diagData is CDpaDiagData cdpaDiagData)
                {
                    var dirName = cdpaDiagData.DirectionName;

                    if (!result.ContainsKey(dirName))
                        result.Add(dirName, new List<DiagData>());

                    result[dirName].Add(cdpaDiagData);
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

        public static List<int> GetCarrierIds(string omniPath)
        {
            var xOmni = XDocument.Load(omniPath);
            var carriersCollection = xOmni.Element("RUN")?.Descendants().Select(q => q.Attribute("CARRIER_ID")?.Value);
            return (from carrier in carriersCollection where carrier != null select int.Parse(carrier)).ToList();
        }

        public static List<CarrierData> GetCarrierData(string omniFilePath, List<CarrierData> carrierDatas)
        {
            var ids = GetCarrierIds(omniFilePath);
            var result = new List<CarrierData>();

            foreach (var id in ids)
            {
                var defectoscopeName = carrierDatas.FirstOrDefault(item => item.Id == id)?.Defectoscope;
                if (defectoscopeName != string.Empty)
                    result.AddRange(carrierDatas.Where(item => item.Defectoscope == defectoscopeName).ToList());
            }
            return result;
        }
    }
}

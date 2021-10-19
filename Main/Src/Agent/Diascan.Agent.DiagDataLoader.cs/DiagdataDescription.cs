using Diascan.NDT.Enums;

namespace Diascan.Agent.DiagDataLoader
{
    public class DiagdataDescription
    {
        public string PointerFileExt { get; private set; }
        public string IndexFileExt { get; private set; }
        public string DataFileExt { get; private set; }
        public DataType DataType { get; private set; }
        public string DataDirSuffix { get; set; }

        public DiagdataDescription(string pointerFileExt, string indexFileExt, string dataFileExt, DataType dataType, string dataDirSuffix)
        {
            PointerFileExt = pointerFileExt;
            IndexFileExt = indexFileExt;
            DataFileExt = dataFileExt;
            DataType = dataType;
            DataDirSuffix = dataDirSuffix;
        }
    }
}

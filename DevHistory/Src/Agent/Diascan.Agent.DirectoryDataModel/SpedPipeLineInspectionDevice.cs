using System;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.DirectoryDataModel
{
    public enum TepePipeLineInspectionDevice
    {
        EMPTY,
        DMU,
        DKK,
        DKU,
        DKM,
        DVU,
        DKP,
        USK03,
        USK04,
        MSK,
        DMK,
        OPT,
        PRN
    }

    public class SpeedPipeLineInspectionDevice
    {
        public Guid                         Id           { get; set; }
        public TepePipeLineInspectionDevice TypePLID     { get; set; }
        public string                       Name         { get; set; }
        public Range<float>                 SpeedsRanges { get; set; }


        public SpeedPipeLineInspectionDevice()
        {
            Id           = Guid.NewGuid();
            TypePLID     = TepePipeLineInspectionDevice.EMPTY;
            Name         = String.Empty;
            SpeedsRanges = new Range<float>();
        }

        public SpeedPipeLineInspectionDevice( TepePipeLineInspectionDevice typePLID, float begin, float end )
        {
            Id       = Guid.NewGuid();
            TypePLID = typePLID;
            switch ( TypePLID )
            {
                case TepePipeLineInspectionDevice.EMPTY:Name = String.Empty; break;
                case TepePipeLineInspectionDevice.DMU  :Name = "ДМУ";        break;
                case TepePipeLineInspectionDevice.DKK  :Name = "ДКК";        break;
                case TepePipeLineInspectionDevice.DKU  :Name = "ДКУ";        break;
                case TepePipeLineInspectionDevice.DKM  :Name = "ДКМ";        break;
                case TepePipeLineInspectionDevice.DVU  :Name = "ДВУ";        break;
                case TepePipeLineInspectionDevice.DKP  :Name = "ДКП";        break;
                case TepePipeLineInspectionDevice.USK03:Name = "УСК.03";     break;
                case TepePipeLineInspectionDevice.USK04:Name = "УСК.04";     break;
                case TepePipeLineInspectionDevice.MSK  :Name = "МСК";        break;
                case TepePipeLineInspectionDevice.DMK  :Name = "ДМК";        break;
                case TepePipeLineInspectionDevice.OPT  :Name = "ОПТ";        break;
                case TepePipeLineInspectionDevice.PRN  :Name = "ПРН";        break;
            }
            SpeedsRanges = new Range<float>( begin, end );
        }
    }
}

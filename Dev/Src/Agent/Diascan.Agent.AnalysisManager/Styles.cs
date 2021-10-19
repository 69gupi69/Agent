using Aspose.Cells;

namespace Diascan.Agent.AnalysisManager
{
    public class Styles
    {
        public static Style BaseStyle(int size = 10)
        {
            var style = new Style();
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Thin;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Thin;
            style.Font.Name = "Franklin Gothic Book";
            style.Font.Size = size;
            style.Pattern = BackgroundType.Solid;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Center;
            style.IsTextWrapped = true;
            return style;
        }

        public static Style Boldface(int size = 10)
        {
            var style = new Style();
            style.Font.Name = "Franklin Gothic Book";
            style.Font.Size = size;
            style.VerticalAlignment = TextAlignmentType.Center;
            style.HorizontalAlignment = TextAlignmentType.Left;
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            return style;
        }

        public static Style SimpleStyle(int size = 10)
        {
            var style = BaseStyle(size);
            style.VerticalAlignment = TextAlignmentType.Left;
            style.HorizontalAlignment = TextAlignmentType.Left;
            return style;
        }

        public static Style DifferentFontStyle(int size = 10)
        {
            var style = BaseStyle(size);
            style.Font.IsBold = true;
            style.IsTextWrapped = true;
            return style;
        }

        public static Style BaseDoubleCellBorderType(int size = 10)
        {
            var style = BaseStyle(size);
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Double;
            return style;
        }

        public static Style DifferentFontBorderType(int size = 10)
        {
            var style = DifferentFontStyle(size);
            style.Borders[BorderType.BottomBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.TopBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.LeftBorder].LineStyle = CellBorderType.Double;
            style.Borders[BorderType.RightBorder].LineStyle = CellBorderType.Double;
            return style;
        }
    }
}

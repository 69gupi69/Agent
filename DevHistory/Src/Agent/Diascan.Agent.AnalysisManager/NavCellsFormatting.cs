using Aspose.Cells;
using Diascan.Agent.CalcDiagDataLossTask;
using DiCore.Lib.NDT.DataProviders.NAV;

namespace Diascan.Agent.AnalysisManager
{
    public class NavCellsFormatting
    {
        public static string GasEnvironment = "Данный режим движения возможен в случае пропуска ВИП по" +
                                              "\nгазу, газо-воздушной и богатой газом среде, обладающей" +
                                              "\nзначительной сжимаемостью (ГС) Кроме того, выполнение данных" +
                                              "\nкритериев может происходить  на особенностях  трубопровода" +
                                              "\nтипа косой стык, тройник, вантуз, камера пуска или приема, даже" +
                                              "\nесли ВИП не двигался в ГС";


        public static void PhaseLast600Sec<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"Δt≥ {param.Value} c" : $"Δt= {item} с";
        }
        public static void LinearSpeed<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"vj={param.Value}+{param.Threshold} м/с" : $"vj= {item} м/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void SquareDeviationAccelX<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[axj]<{param.Value} м/с²" : $"S[axj]= {item} м/с²";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void SquareDeviationAccelY<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ayj]<{param.Value} м/с²" : $"S[ayj]= {item} м/с";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void SquareDeviationAccelZ<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[azj]<{param.Value} м/с²" : $"S[azj]= {item} м/с";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void SquareDeviationAngularSpeedX<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωxj]<{param.Value} \u00B0/с" : $"S[ωxj]= {item} \u00B0/с";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void SquareDeviationAngularSpeedY<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωyj]<{param.Value} \u00B0/с" : $"S[ωyj]= {item} \u00B0/с";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void SquareDeviationAngularSpeedZ<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"S[ωzj]<{param.Value} \u00B0/с" : $"S[ωzj]= {item} \u00B0/с";
            cell.Characters(3, 2).Font.IsSubscript = true;
        }
        public static void GravAccel<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"g={param.Value}±{param.Threshold} м/с²" : $"g= {item} м/с²";
        }
        public static void EarthAngularSpeedRotation<T>(Cell cell, T item, bool isEqualOperation = true) // <-
        {
            cell.Value = item is NavCalcParamItem param
                ? (param.Threshold > 0 ? $"ωₒ= {param.Value}±{param.Threshold} \u00B0/с" : $"ωₒ< {param.Value}\u00B0/с")
                : (isEqualOperation ? $"ωₒ= {item} \u00B0/с" : $"ωₒ< {item} \u00B0/с");
        }
        public static void DiffLatitude<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"|ψ-ψₒ|<{param.Value}\u00B0" : $"|ψ-ψₒ|= {item}\u00B0";
        }
        public static void AccelSum<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"aс={param.Value}±{param.Threshold} м/с²" : $"aс= {item} м/с²";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void AccelMax<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"am < {param.Value} м/с²" : $"am= {item} м/с²";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void AngularSpeedSum<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ωс < {param.Value} \u00B0/с" : $"ωс= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void AngularSpeedMax<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ωm < {param.Value} \u00B0/с" : $"ωm= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void AverageRollAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"γс < {param.Value}\u00B0" : $"γс= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void AveragePitchAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"Θс < {param.Value}\u00B0" : $"Θс= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void MaxRollAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"γm < {param.Value}\u00B0" : $"γm= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void MaxPitchAngle<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"Θm < {param.Value}\u00B0" : $"Θm= {item} \u00B0/с";
            cell.Characters(1, 1).Font.IsSubscript = true;
        }
        public static void PitchAverageValue(Cell cell, NavCalcParamItem param)
        {
            cell.Value = $"ΔΘj > {param.Value}\u00B0 " + "M{ΔΘj} > " + param.Threshold + "\u00B0";
            cell.Characters(2, 1).Font.IsSubscript = true;
            cell.Characters(12 + param.Value.ToString().Length, 1).Font.IsSubscript = true;
        }
        public static void PitchAngleAtMovement<T>(Cell cell, T item)
        {
            cell.Value = item is NavCalcParamItem param ? $"ΔΘj > {param.Value}\u00B0" : $"ΔΘj = {item} \u00B0/с"; // угол тангажа > 4 градусов
            cell.Characters(2, 1).Font.IsSubscript = true;
        }
        public static void PitchAngleSection(Cell cell, int sectionLen, int value)
        {
            cell.Value = $"C{sectionLen} = {value}";
        }
        public static void AverageRollAngleDesc(Cell cell)
        {
            cell.Value = "где γс - среднее значение расхождения по углу крена в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void AveragePitchAngleDesc(Cell cell)
        {
            cell.Value = "где Θс - среднее значение расхождения по углу тангажа в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void MaxRollAngleDesc(Cell cell)
        {
            cell.Value = "где γс - максимальное значение расхождения по углу крена в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void MaxPitchAngleDesc(Cell cell)
        {
            cell.Value = "где Θс - максимальное значение расхождения по углу тангажа в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void PitchAngleAtMovementDesc(Cell cell)
        {
            cell.Value = "где ΔΘ - колличество расхождений по углу тангажа, > 4° градусов в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }

        public static void PitchAngleSectionPhaseDesc(Cell cell, int sectionLen)
        {
            cell.Value = $"наличие непрерывных участков данных протяженностью от {sectionLen} м";
        }

        public static void SetupNavTime(Cell cell)
        {
            cell.Value = $"где Δt - время выставки";
        }

        public static void AccelAngularSpeedDesc(Cell cell, enNavType navType)
        {
            if (navType == enNavType.Bep)
            {
                cell.Value = "где vj - линейная скорость ВИП" +
                             "\nj - элементы НД, используемые при обработке(относящиеся к участку выставки)" +
                             "\nωj – угловая скорость вращения ВИП" +
                             "\nΔωj - изменение угла гироскопа" +
                             "\naj – проекция линейного ускорения" +
                             "\nS{∙}-операция вычисления среднеквадратичного отклонения";
                cell.Characters(5, 1).Font.IsSubscript = true;
                cell.Characters(108, 1).Font.IsSubscript = true;
                cell.Characters(144, 1).Font.IsSubscript = true;
                cell.Characters(174, 1).Font.IsSubscript = true;
            }
            else
            {
                cell.Value = "где vj - линейная скорость ВИП" +
                             "\nj - элементы НД, используемые при обработке(относящиеся к участку выставки)" +
                             "\nωj = 200Δωj / 3600 – угловая скорость" +
                             "\nΔωj - изменение угла гироскопа" +
                             "\naj – проекция линейного ускорения" +
                             "\nS{∙}-операция вычисления среднеквадратичного отклонения";
                cell.Characters(5, 1).Font.IsSubscript = true;
                cell.Characters(108, 1).Font.IsSubscript = true;
                cell.Characters(147, 1).Font.IsSubscript = true;
                cell.Characters(177, 1).Font.IsSubscript = true;
            }
        }
        public static void GravAccelDesc(Cell cell)
        {
            cell.Value = @"где g - значение ускорения свободного падения на участке выставки в течение последней минуты перед началом движения";
        }
        public static void EarthAngularSpeedRotationDesc(Cell cell)
        {
            cell.Value = @"где ωₒ - значения угловой скорости вращения Земли на участке выставки в течение последней минуты перед началом движения";
        }
        public static void DiffLatitudeDesc(Cell cell)
        {
            cell.Value = @"где ψ -значения широты на участке выставки в течение последней минуты перед началом движения, ψₒ - истинная широта камеры пуска ВИП";
        }
        public static void AccelSumDesc(Cell cell)
        {
            cell.Value = "где aс - значение обобщенного ускорения в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void AccelMaxDesc(Cell cell)
        {
            cell.Value = "где am - значение обобщенного ускорения в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void AngularSpeedSumDesc(Cell cell)
        {
            cell.Value = "где ωс - значение обобщенной угловой скорости в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void AngularSpeedMaxDesc(Cell cell)
        {
            cell.Value = "где ωm -  значение обобщенной угловой скорости в пропуске";
            cell.Characters(5, 1).Font.IsSubscript = true;
        }
        public static void PitchAngleSectionDesc(Cell cell, int sectionLen)
        {
            cell.Value = $"С{sectionLen} - количество непрерывных участков данных протяженностью от {sectionLen} м";
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Diascan.Agent.Types.Annotations;
using Diascan.Agent.Types.ModelCalculationDiagData;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.Types
{
    public class HeaderSession : INotifyPropertyChanged
    {
        private readonly Action<object> loggerInfoAction;
        private bool stateError = false;
        private string state = string.Empty;
        private string status = string.Empty;
        private string restart = string.Empty;
        private bool completed = false;
        private const enCalculationStateTypes EnCompleted = enCalculationStateTypes.FindTypes |
                                            enCalculationStateTypes.Hashe |
                                            enCalculationStateTypes.OverSpeed |
                                            enCalculationStateTypes.HaltingSensors |
                                            enCalculationStateTypes.CdlTail |
                                            enCalculationStateTypes.SplitDataTypeRange |
                                            enCalculationStateTypes.Analysis;



        public int Id { get; set; }
        public string RunCode { get; set; }
        public string Path { get; set; }
        public string State {
            get => state;
            set
            {
                if (value == state) return;
                state = value;
                NotifyPropertyChanged();
            }
        }

        public string Status
        {
            get => status;
            set
            {
                if (value == status) return;
                status = value;
                NotifyPropertyChanged();
            }
        }
        public string Restart {
            get => restart;
            set
            {
                if (value == restart) return;
                restart = value;
                NotifyPropertyChanged();
            }
        }
        public DateTime DateTime { get; set; }
        //  no visible
        public Guid GlobalId { get; set; }
        public bool Сompleted {
            get => completed;
            set
            {
                if (value == completed) return;
                completed = value;
                NotifyPropertyChanged();
            }
        }

        public HeaderSession()
        {}

        public HeaderSession(Session session, Action<object> loggerInfoAction)
        {
            this.loggerInfoAction = loggerInfoAction;
            Id                    = session.Id;
            GlobalId              = session.GlobalID;
            Path                  = session.BasePath;
            DateTime              = session.StartDateDataCalculation;
            RunCode               = session.Calculations[0]?.DataOutput.WorkItemName ?? "";
            Change(session);
        }

        public void Change(Session session)
        {
            State   = GetState(session.Calculations);
            Status  = StatusOfCalculate(session.Calculations);
            Restart = RestartCalculation(session.Calculations);
        }

        private string RestartCalculation(List<Calculation> calculations)
        {
            
            if (calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(EnCompleted)))
                return calculations.Aggregate(false, (current, calculation) => current | calculation.IsNeedRestart) ? "Требуется перезапуск" : "Перезапуск не требуется";
            else
                return "";
        }

        private string StateErrorOrСompleted(List<Calculation> calculations)
        {
            var result = string.Empty;

            stateError = calculations.Aggregate(false, (current, calculation) => current | calculation.WorkState.HasFlag(enWorkState.Error));
            if (stateError)
                result = "Ошибка в расчете\n";

            Сompleted = calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(EnCompleted));

            if (Сompleted)
                result = "Расчет завершен\n";

            return result;
        }


        private string GetState(List<Calculation> calculations)
        {
            var result = StateErrorOrСompleted(calculations);
            if (string.IsNullOrEmpty(result))
            {
                result = ProgressPreliminaryCalculations(calculations) +
                         ProgressNavigation(calculations) +
                         ProgressPdiCalculations(calculations) +
                         ProgressCdlTail(calculations);
            }
            return result;
        }

        private string ProgressPreliminaryCalculations(List<Calculation> calculations)
        {
            var b = calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(enCalculationStateTypes.Hashe));
            return b ? "" :  $"Предварительные вычисления:  {Math.Round(calculations.Sum(calculation => calculation.ProgressHashes)/calculations.Count, 2)}%";
        }

        private string ProgressNavigation(List<Calculation> calculations)
        {
            var b = calculations.Aggregate(false, (current, calculation) => current | !calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.NavigationData));
            return b ? "" : $"{Environment.NewLine}Расчет навигаци: {Math.Round(calculations.Sum(calculation => calculation.ProgressNavData * 100) / calculations.Count, 1)}%";
        }

        private string ProgressPdiCalculations(List<Calculation> calculations)
        {
            double persentPdi = 0;
            var count = 0;

            foreach (var calculation in calculations)
            {
                foreach (var diagData in calculation.DiagDataList)
                {
                    if (diagData.DataType == DataType.Nav) continue;
                    var distStop = diagData.ProcessedDist < 0 ? 0 : diagData.ProcessedDist;
                    persentPdi += distStop / diagData.MaxDistance * 100;
                    count++;
                }
            }

            if (count == 0) return "\n";
            persentPdi = persentPdi / count;
            var str = double.IsNaN(persentPdi) ? "0" : Math.Round(persentPdi, 2).ToString();
            return $"{Environment.NewLine}Поиск ПДИ: {str}%\n";
        }

        private string ProgressCdlTail(List<Calculation> calculations)
        {
            if (calculations.All(calculation=> calculation.ProgressCdlTail == 0.0d))
                return "";

            var b = calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(enCalculationStateTypes.CdlTail));
            return b ? "" : $"{Environment.NewLine}Определение типа CD: {Math.Round(calculations.Sum(calculation => calculation.ProgressCdlTail) / calculations.Count, 2)}%";
        }

        private string StatusOfCalculate(List<Calculation> calculations)
        {
            if (calculations[0].DataOutput.Local)
                return "Расчет СМР";

            if (calculations[0].NavigationInfo.NavType == enNavType.None)
            {
                return calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(enCalculationStateTypes.FindTypes      |
                                                                                                                   enCalculationStateTypes.Hashe          |
                                                                                                                   enCalculationStateTypes.OverSpeed      |
                                                                                                                   enCalculationStateTypes.HaltingSensors |
                                                                                                                   enCalculationStateTypes.CdlTail        |
                                                                                                                   enCalculationStateTypes.Analysis       |
                                                                                                                   enCalculationStateTypes.Sended))
                       ? "Отправлено" : "Не отправлено";
            }
            else
                return calculations.Aggregate(false, (current, calculation) => current | calculation.State.HasFlag(enCalculationStateTypes.FindTypes      |
                                                                                                                   enCalculationStateTypes.Hashe          |
                                                                                                                   enCalculationStateTypes.OverSpeed      |
                                                                                                                   enCalculationStateTypes.HaltingSensors |
                                                                                                                   enCalculationStateTypes.CdlTail        |
                                                                                                                   enCalculationStateTypes.Analysis       |
                                                                                                                   enCalculationStateTypes.Sended)        &&
                                                                                         calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.CalcNavigation))
                       ? "Отправлено" : "Не отправлено";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
            catch (Exception ex)
            {
                loggerInfoAction?.Invoke($"HeaderCalculation :\n\n Id = {Id}\n RunCode = {RunCode}\n State = {State}\n Status = {Status}\n Restart = {Restart}\n Сompleted = {Сompleted}\n ERROR Message = {ex.Message}\n StackTrace =\n{ex.StackTrace}\n");
            }
        }
    }
}

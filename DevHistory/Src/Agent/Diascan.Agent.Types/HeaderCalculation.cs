using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Diascan.Agent.Types.Annotations;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders.NAV;
using Newtonsoft.Json;

namespace Diascan.Agent.Types
{
    public class HeaderCalculation : INotifyPropertyChanged
    {
        private readonly Action<object> loggerInfoAction;
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

        public HeaderCalculation()
        {}

        public HeaderCalculation(Calculation calculation, Action<object> loggerInfoAction)
        {
            this.loggerInfoAction = loggerInfoAction;
            Change(calculation);
        }

        public void Change(Calculation calculation)
        {
            Сompleted = calculation.State.HasFlag(EnCompleted) || calculation.WorkState.HasFlag(enWorkState.Error);
            Id = calculation.Id;
            GlobalId = calculation.GlobalId;
            RunCode = calculation.DataOutput.WorkItemName;
            Path = calculation.SourcePath;
            State = GetState(calculation);
            Status = StatusOfCalculate(calculation);
            Restart = RestartCalculation(calculation);
            DateTime = calculation.TimeAddCalculation;
        }

        private string RestartCalculation(Calculation calculation)
        {
            if (calculation.State.HasFlag(EnCompleted))
                return calculation.IsNeedRestart ? "Требуется перезапуск" : "Перезапуск не требуется";
            else
                return "";
        }


        private string GetState(Calculation calculation)
        {
            if (calculation.WorkState.HasFlag(enWorkState.Error))
                return "Ошибка в расчете";

            if (Сompleted)
                return "Расчет завершен";

            return ProgressPreliminaryCalculations(calculation) +
                   ProgressNavigation(calculation) +
                   ProgressPdiCalculations(calculation) +
                   ProgressCdlTail(calculation);
        }

        private string ProgressPreliminaryCalculations(Calculation calculation)
        {
            return $"Предварительные вычисления: {calculation.ProgressHashes}";
        }

        private string ProgressNavigation(Calculation calculation)
        {
            return calculation.NavigationInfo.State.HasFlag(NavigationStateTypes.NavigationData) ? $"{Environment.NewLine}Расчет навигаци: {Math.Round(calculation.ProgressNavData * 100, 1)}%" : "";
        }

        private string ProgressPdiCalculations(Calculation calculation)
        {
            double persentPdi = 0;
            var count = 0;
            foreach (var diagData in calculation.DiagDataList)
            {
                if (diagData.DataType == DataType.Nav) continue;
                var distStop = diagData.ProcessedDist < 0 ? 0 : diagData.ProcessedDist;
                persentPdi += distStop / diagData.MaxDistance * 100;
                count++;
            }

            if (count == 0) return "";
            persentPdi = persentPdi / count;
            var str = double.IsNaN(persentPdi) ? "0" : Math.Round(persentPdi, 2).ToString();
            return $"{Environment.NewLine}Поиск ПДИ: {str}%";
        }

        private string ProgressCdlTail(Calculation calculation)
        {
            return calculation.ProgressCdlTail == null
                ? ""
                : $"{Environment.NewLine}Определение типа CD: {calculation.ProgressCdlTail}";
        }

        private string StatusOfCalculate(Calculation calculation)
        {
            if (calculation.DataOutput.Local)
                return "Расчет СМР";

            if (calculation.NavigationInfo.NavType == enNavType.None)
            {
                return calculation.State.HasFlag(enCalculationStateTypes.FindTypes |
                                             enCalculationStateTypes.Hashe |
                                             enCalculationStateTypes.OverSpeed |
                                             enCalculationStateTypes.HaltingSensors |
                                             enCalculationStateTypes.CdlTail |
                                             enCalculationStateTypes.Analysis |
                                             enCalculationStateTypes.Sended) ? "Отправлено" : "Не отправлено";
            }
            else
                return (calculation.State.HasFlag(enCalculationStateTypes.FindTypes |
                                              enCalculationStateTypes.Hashe |
                                              enCalculationStateTypes.OverSpeed |
                                              enCalculationStateTypes.HaltingSensors |
                                              enCalculationStateTypes.CdlTail |
                                              enCalculationStateTypes.Analysis |
                                              enCalculationStateTypes.Sended) &&
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

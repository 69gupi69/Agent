using System;
using DiCore.Lib.NDT.Types;

namespace Diascan.Agent.Types
{
    
    public class ReferenceInputData
    {
        /// <summary>
        /// Назване прогона
        /// </summary>
        public string WorkItemName { get; set; }
        /// <summary>
        /// Имя учетной записи пользователя
        /// </summary>
        public string AccountUserName { get; set; }
        /// <summary>
        /// Имя компьютера
        /// </summary>
        public string ComputerName { get; set; }
        /// <summary>
        /// Ответственный за пропуск
        /// </summary>
        public string ResponsibleWorkItem { get; set; }
        /// <summary>
        /// Общая площадь ПДИ
        /// </summary>
        public double OverallAreaLdi { get; set; }
        /// <summary>
        /// Общая площадь ПДИ превышения скорости
        /// </summary>
        public double OverallSpeedAreaLdi { get; set; }
        /// <summary>
        /// Камера приема
        /// </summary>
        public double ReceptionChamber { get; set; }
        /// <summary>
        /// Камеры пуска
        /// </summary>
        public double TriggerChamber { get; set; }
        /// <summary>
        /// Общая площадь
        /// </summary>
        public double OverallArea { get; set; }
        /// <summary>
        /// Диаметр
        /// </summary>
        public double Diameter { get; set; }
        /// <summary>
        /// Длина участка по ТЗ (м) (фактическая длина)
        /// </summary>
        public double PlotLengthTechSpec { get; set; }
        /// <summary>
        /// Дата пропуска
        /// </summary>
        public string DateWorkItem { get; set; }
        /// <summary>
        /// Дефектоскоп №
        /// </summary>
        public string FlawDetector { get; set; }
        /// <summary>
        /// Протяженность по ВИП
        /// </summary>
        public Range<double> DistRange { get; set; }
        /// <summary>
        /// Контракторы
        /// </summary>
        public KeyValue<Guid, string> Contractor { get; set; }
        /// <summary>
        /// Трубопроводы
        /// </summary>
        public KeyValue<Guid, string> PipeLine { get; set; }
        /// <summary>
        /// Участок
        /// </summary>
        public KeyValue<Guid, string> Route { get; set; }
        /// <summary>
        /// Широта участка выставки
        /// </summary>
        public double RealLatitude { get; set; }
        /// <summary>
        /// Признак расчета СМР (без отправки на сервер)
        /// </summary>
        public bool Local { get; set; } = false;
        /// <summary>
        /// Наличие приемной задвижки
        /// </summary>
        public bool GateValve { get; set; } = false;
    }
    
    public class KeyValue<T1, T2>
    {
        public T1 Key { get; set; }
        public T2 Value { get; set; }
    
        public KeyValue() { }
    
        public KeyValue(T1 key, T2 value)
        {
            Key = key;
            Value = value;
        }
    }
}


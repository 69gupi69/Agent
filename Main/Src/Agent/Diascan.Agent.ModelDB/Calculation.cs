using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Windows;

namespace Diascan.Agent.ModelDB
{
    public class ReferenceInputData
    {
        public string WorkItemName { get; set; } // Назване прогона
        public string AccountUserName { get; set; } // Имя учетной записи пользователя
        public string ComputerName { get; set; } // Имя компьютера
        public string ResponsibleWorkItem { get; set; } // Отведственный за пропуск
        public double OverallAreaLdi { get; set; } // Общая площадь ПДИ
        public double OverallSpeedAreaLdi { get; set; } // Общая площадь ПДИ превышения скорости
        public double ReceptionChamber { get; set; } // Камера приема
        public double TriggerChamber { get; set; } // Камеры пуска
        public double OverallArea { get; set; } // Общая площадь
        public double Diameter { get; set; } // Диаметр
        public string DateWorkItem { get; set; } // Дата пропуска
        public string FlawDetector { get; set; } // Дефектоскоп №
        public Range<double> DistRange { get; set; } // Протяженность по ВИП
        public KeyValue<Guid, string> Contractor { get; set; } // Контракторы
        public KeyValue<Guid, string> PipeLine { get; set; } // Трубопроводы
        public KeyValue<Guid, string> Route { get; set; } // Участок
        public double RealLatitude { get; set; }    // Широта участка выставки
    }

    public class Helper
    {
        public double CdTailDistProgress { get; set; } = double.MinValue;
        public string ProgressHashes { get; set; } = "0%"; // Програесс хешей
        public string ProgressCdlTail { get; set; } // Програесс "хвостов"
        public double ProgressNavData { get; set; } = 0;  // Програесс навигации
        public Dictionary<int, int> AdjacentSensors { get; set; } = new Dictionary<int, int>();  //  таблица соответствия датчиков с двойными углами
        public List<SensorMediaIdentifier> Carriers { get; set; } //  найденые кериеры из таблицы SensorMediaIdentifiers
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

    public class Calculation
    {
        public Guid GlobalId { get; set; }
        [JsonIgnore]
        public int Id { get; set; }
        [JsonIgnore]                        
        public string SourcePath { get; set; }
        [JsonIgnore]                        
        public string OmniFilePath { get; set; }
        public ReferenceInputData DataOutput { get; set; }
        [JsonIgnore]
        public CalculationStateTypes State { get; set; }
        public NavigationInfo NavigationInfo { get; set; }
        public List<DiagData> DiagDataList { get; set; }
        public DateTime TimeAddCalculation { get; set; }
        public List<Rect> Frames { get; set; }
        public bool FramesTypeCds { get; set; }
        public bool CdChange { get; set; }
        /////////////////////////////////////////////
        public List<RestartCriterion> RestartReport { get; set; }
        /////////////////////////////////////////////
        [JsonIgnore]
        public Helper Helper { get; set; }

    public Calculation()
        {
            TimeAddCalculation = new DateTime();
            DiagDataList = new List<DiagData>();
            RestartReport = new List<RestartCriterion>();
            NavigationInfo = new NavigationInfo();
        }

        public Calculation(string path): this()
        {
            DataOutput = new ReferenceInputData();
            Frames = new List<Rect>();
            Helper = new Helper();
            SourcePath = path;
        }
    }
}

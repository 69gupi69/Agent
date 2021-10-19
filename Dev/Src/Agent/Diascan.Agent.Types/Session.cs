using System;
using System.Collections.Generic;
using Diascan.Agent.Types.ModelCalculationDiagData;
using Newtonsoft.Json;

namespace Diascan.Agent.Types
{
    public class Session
    {

        /// <summary>
        /// Код глобальный прогона
        /// </summary>
        public Guid GlobalID { get; set; }

        [JsonIgnore]
        public int Id { get; set; }

        private DateTime startLocalDate;
        /// <summary>
        /// Дата старта расчета данных 
        /// </summary>
        public DateTime StartDateDataCalculation
        {
            get { return startLocalDate; }

            set
            {
                if (startLocalDate == value) return;
                startLocalDate = value;
            }
        }

        private string basePath;
        /// <summary>
        /// Корневой каталог
        /// </summary>
        public string BasePath
        {
            get { return basePath; }

            set
            {
                if (basePath == value) return;
                basePath = value;
            }
        }

        private string baseFile;
        /// <summary>
        /// Имя базового файла
        /// </summary>
        public string BaseFile
        {
            get { return baseFile; }

            set
            {
                if (baseFile == value) return;

                baseFile = value;
            }
        }

        private List<Calculation> calculations;
        /// <summary>
        /// Расчеты инспекций
        /// </summary>
        public List<Calculation> Calculations
        {
            get { return calculations; }

            set
            {
                if (calculations == value) return;
                calculations = value;
            }
        }

        public Session(Guid id)
        {
            GlobalID = id;
            Calculations = new List<Calculation>();
            StartDateDataCalculation = DateTime.Now;
        }

        public Session()
        {
            Calculations = new List<Calculation>();
            StartDateDataCalculation = DateTime.Now;
        }
    }
}

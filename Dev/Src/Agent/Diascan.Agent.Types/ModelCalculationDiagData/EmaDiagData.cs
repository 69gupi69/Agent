using System.Collections.Generic;
using Diascan.NDT.Enums;
using DiCore.Lib.NDT.DataProviders.EMA;
using DiCore.Lib.NDT.Types;
using Newtonsoft.Json;

namespace Diascan.Agent.Types.ModelCalculationDiagData
{
    public class EmaDiagData : IDiagData
    {
        #region Property Interface
        public bool State { get; set; }
        public DataType DataType { get; set; }
        public int SensorCount { get; set; }
        public int NumberSensorsBlock { get; set; }
        public double ProcessedDist { get; set; }
        public double MaxDistance { get; set; }
        public double DistanceLength { get; set; }
        public double AreaLdi { get; set; }
        [JsonIgnore]
        public double AreaContiguous { get; set; }
        public double StartDist { get; set; }
        public double StopDist { get; set; }
        public Range<double> PassportSpeedDiapason { get; set; } // Паспортный диапазон скорости ВИП
        [JsonIgnore]
        public List<int> SensorsСontiguous { get; set; }
        public List<OverSpeedInfo> SpeedInfos { get; set; }
        public List<FileHashed> Files { get; set; }
        public Dictionary<int, List<SensorRange>> HaltingSensors { get; set; }
        public Dictionary<Range<double>, List<int>> ResultSensorDistances { get; set; }
        #endregion

        /// <summary>
        /// Правила анализа датчиков 
        /// </summary>
        public Dictionary<int, SensorAnalysisRule []> SensorAnalysisRules { get; set; }

        /// <summary>
        /// Прибор
        /// </summary>
        public string Defectoscope { get; set; }

        /// <summary>
        /// Диаметр в дюймах
        /// </summary>
        public int Diameter { get; }

        public EmaDiagData() { }

        public EmaDiagData(DataType dataType, FileHashed[] files, string defectoscope, int diameter)
        {
            Diameter               = diameter;
            Defectoscope           = defectoscope;
            DataType               = dataType;
            State                  = false;
            ProcessedDist          = float.MinValue;
            Files                  = new List<FileHashed>();
            SpeedInfos             = new List<OverSpeedInfo>();
            HaltingSensors         = new Dictionary<int, List<SensorRange>>();
            ResultSensorDistances  = new Dictionary<Range<double>, List<int>>();
            Files.AddRange(files);
            СreateSensorAnalysisRules();

            //var strRes = new List<string>();
            //for (var i = 0; i < str1.Length; i++)
            //{
            //    var strBuff = string.Empty;
            //    if (str1[i] == '[')
            //    {
            //        for (var j = i + 1; j < str1.Length; j++)
            //        {
            //            if (str1[j] == ']')
            //            {
            //                strRes.Add(strBuff);
            //                i = j;
            //                break;
            //            }
            //            strBuff += str1[j];
            //        }
            //    }
            //}
        }

        private void СreateSensorAnalysisRules()
        {
            switch (Diameter)
            {
                case 48:
                    SensorAnalysisRules = new Dictionary<int, SensorAnalysisRule[]>();
                    SensorAnalysisRules.Add(1  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 3  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 5  }});
                    SensorAnalysisRules.Add(2  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 4  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 6  }});
                    SensorAnalysisRules.Add(3  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 5  }});
                    SensorAnalysisRules.Add(4  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 6  }});
                    SensorAnalysisRules.Add(5  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 3  }});
                    SensorAnalysisRules.Add(6  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 4  }});
                    SensorAnalysisRules.Add(7  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 11 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 5  } , new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 3  }});
                    SensorAnalysisRules.Add(8  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 12 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 6  } , new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 4  }});
                    SensorAnalysisRules.Add(9  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 11 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 13 } , new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 5  }});
                    SensorAnalysisRules.Add(10 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 12 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 14 } , new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 6  }});
                    SensorAnalysisRules.Add(11 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 13 }});
                    SensorAnalysisRules.Add(12 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 14 }});
                    SensorAnalysisRules.Add(13 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 11 }});
                    SensorAnalysisRules.Add(14 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 12 }});
                    SensorAnalysisRules.Add(15 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 13 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 11 }});
                    SensorAnalysisRules.Add(16 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 14 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 12 }});
                    SensorAnalysisRules.Add(17 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 19 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 21 }});
                    SensorAnalysisRules.Add(18 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 20 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 22 }});
                    SensorAnalysisRules.Add(19 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 21 }});
                    SensorAnalysisRules.Add(20 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 22 }});
                    SensorAnalysisRules.Add(21 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 19 }});
                    SensorAnalysisRules.Add(22 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 20 }});
                    SensorAnalysisRules.Add(23 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 27 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 21 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 19 }});
                    SensorAnalysisRules.Add(24 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 28 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 22 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 20 }});
                    SensorAnalysisRules.Add(25 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 27 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 29 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 21 }});
                    SensorAnalysisRules.Add(26 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 28 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 30 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 22 }});
                    SensorAnalysisRules.Add(27 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 29 }});
                    SensorAnalysisRules.Add(28 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 30 }});
                    SensorAnalysisRules.Add(29 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 27 }});
                    SensorAnalysisRules.Add(30 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 28 }});
                    SensorAnalysisRules.Add(31 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 29 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 27 }});
                    SensorAnalysisRules.Add(32 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 30 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 28 }});
                    SensorAnalysisRules.Add(33 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 35 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 37 }});
                    SensorAnalysisRules.Add(34 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 36 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 38 }});
                    SensorAnalysisRules.Add(35 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 37 }});
                    SensorAnalysisRules.Add(36 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 38 }});
                    SensorAnalysisRules.Add(37 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 35 }});
                    SensorAnalysisRules.Add(38 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 36 }});
                    SensorAnalysisRules.Add(39 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 43 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 37 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 35 }});
                    SensorAnalysisRules.Add(40 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 44 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 38 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 36 }});
                    SensorAnalysisRules.Add(41 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 43 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 45 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 37 }});
                    SensorAnalysisRules.Add(42 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 44 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 46 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 38 }});
                    SensorAnalysisRules.Add(43 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 45 }});
                    SensorAnalysisRules.Add(44 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 46 }});
                    SensorAnalysisRules.Add(45 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 43 }});
                    SensorAnalysisRules.Add(46 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 44 }});
                    SensorAnalysisRules.Add(47 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 45 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 43}});
                    SensorAnalysisRules.Add(48 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 46 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 44}});
                    SensorAnalysisRules.Add(49 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 51 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 53}});
                    SensorAnalysisRules.Add(50 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 52 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 54}});
                    SensorAnalysisRules.Add(51 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 53 }});
                    SensorAnalysisRules.Add(52 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 54 }});
                    SensorAnalysisRules.Add(53 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 51 }});
                    SensorAnalysisRules.Add(54 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 52 }});
                    SensorAnalysisRules.Add(55 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 59 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 53}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 51 }});
                    SensorAnalysisRules.Add(56 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 60 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 54}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 52 }});
                    SensorAnalysisRules.Add(57 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 59 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 61}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 53 }});
                    SensorAnalysisRules.Add(58 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 60 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 62}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 54 }});
                    SensorAnalysisRules.Add(59 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 61 }});                                                                                                                          
                    SensorAnalysisRules.Add(60 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 62 }});                                                                                                                          
                    SensorAnalysisRules.Add(61 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 59 }});                                                                                                                          
                    SensorAnalysisRules.Add(62 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 60 }});                                                                                                                          
                    SensorAnalysisRules.Add(63 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 61 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 59}});                                                  
                    SensorAnalysisRules.Add(64 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 62 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 60}});                                                  
                    SensorAnalysisRules.Add(65 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 67 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 69}});                                                  
                    SensorAnalysisRules.Add(66 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 68 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 70}});                                                  
                    SensorAnalysisRules.Add(67 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 69 }});                                                                                                                          
                    SensorAnalysisRules.Add(68 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 70 }});                                                                                                                          
                    SensorAnalysisRules.Add(69 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 67 }});                                                                                                                          
                    SensorAnalysisRules.Add(70 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 68 }});                                                                                                                          
                    SensorAnalysisRules.Add(71 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 69}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 67 }});
                    SensorAnalysisRules.Add(72 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 76 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 70}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 68 }});
                    SensorAnalysisRules.Add(73 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 77}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 69 }});
                    SensorAnalysisRules.Add(74 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 76 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 78}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 70 }});
                    SensorAnalysisRules.Add(75 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 77 }});
                    SensorAnalysisRules.Add(76 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 78 }});
                    SensorAnalysisRules.Add(77 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 75 }});
                    SensorAnalysisRules.Add(78 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 76 }});
                    SensorAnalysisRules.Add(79 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 77 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 75}});
                    SensorAnalysisRules.Add(80 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 78 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 76}});
                    SensorAnalysisRules.Add(81 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 83 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 85}});
                    SensorAnalysisRules.Add(82 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 84 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 86}});
                    SensorAnalysisRules.Add(83 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 85 }});
                    SensorAnalysisRules.Add(84 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 86 }});
                    SensorAnalysisRules.Add(85 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 83 }});
                    SensorAnalysisRules.Add(86 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 84 }});
                    SensorAnalysisRules.Add(87 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 91 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 85}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 83 }});
                    SensorAnalysisRules.Add(88 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 92 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 86}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 84 }});
                    SensorAnalysisRules.Add(89 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 91 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 93}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 85 }});
                    SensorAnalysisRules.Add(90 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 92 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 94}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 86 }});
                    SensorAnalysisRules.Add(91 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 93 }});
                    SensorAnalysisRules.Add(92 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 94 }});
                    SensorAnalysisRules.Add(93 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 91 }});
                    SensorAnalysisRules.Add(94 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 92 }});
                    SensorAnalysisRules.Add(95 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 93 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 91 }});
                    SensorAnalysisRules.Add(96 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 94 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 92 }});
                    SensorAnalysisRules.Add(97 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 99 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 101}});
                    SensorAnalysisRules.Add(98 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 100}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 102}});
                    SensorAnalysisRules.Add(99 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 101}});
                    SensorAnalysisRules.Add(100, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 102}});
                    SensorAnalysisRules.Add(101, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 99 }});
                    SensorAnalysisRules.Add(102, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 100}});
                    SensorAnalysisRules.Add(103, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 107}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 101}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 99 }});
                    SensorAnalysisRules.Add(104, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 108}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 102}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 100}});
                    SensorAnalysisRules.Add(105, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 107}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 109}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 101}});
                    SensorAnalysisRules.Add(106, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 108}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 110}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 102}});
                    SensorAnalysisRules.Add(107, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 109}});                                                                                                                           
                    SensorAnalysisRules.Add(108, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 110}});                                                                                                                           
                    SensorAnalysisRules.Add(109, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 107}});                                                                                                                           
                    SensorAnalysisRules.Add(110, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 108}});                                                                                                                           
                    SensorAnalysisRules.Add(111, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 109}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 107}});                                                  
                    SensorAnalysisRules.Add(112, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 110}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 108}});                                                  
                    SensorAnalysisRules.Add(113, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 115}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 117}});                                                  
                    SensorAnalysisRules.Add(114, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 116}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 118}});                                                  
                    SensorAnalysisRules.Add(115, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 117}});                                                                                                                           
                    SensorAnalysisRules.Add(116, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 118}});                                                                                                                           
                    SensorAnalysisRules.Add(117, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 115}});                                                                                                                           
                    SensorAnalysisRules.Add(118, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 116}});                                                                                                                           
                    SensorAnalysisRules.Add(119, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 123}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 117}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 115}});
                    SensorAnalysisRules.Add(120, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2 ,EmittingSensor = 124}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 118}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 116}});
                    SensorAnalysisRules.Add(121, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 123}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 125}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 117}});
                    SensorAnalysisRules.Add(122, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 124}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 126}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 118}});
                    SensorAnalysisRules.Add(123, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 125}});
                    SensorAnalysisRules.Add(124, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1 ,EmittingSensor = 126}});
                    SensorAnalysisRules.Add(125, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 123}});
                    SensorAnalysisRules.Add(126, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 124}});
                    SensorAnalysisRules.Add(127, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 125}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 123} });
                    SensorAnalysisRules.Add(128, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1 ,EmittingSensor = 126}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 124} });
                    break;
                case 42:
                    SensorAnalysisRules = new Dictionary<int, SensorAnalysisRule[]>();
                    SensorAnalysisRules.Add(1  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 3  },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,     EmittingSensor =5 } });
                    SensorAnalysisRules.Add(2  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 4  },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =6 }});
                    SensorAnalysisRules.Add(3  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 5  }});                                              
                    SensorAnalysisRules.Add(4  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 6  }});                                              
                    SensorAnalysisRules.Add(5  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 9  },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =3  }});
                    SensorAnalysisRules.Add(6  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 10 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =4  }});
                    SensorAnalysisRules.Add(7  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 9  },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =11 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 5 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 3 }});
                    SensorAnalysisRules.Add(8  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 10 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =12 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 5 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 3 }});
                    SensorAnalysisRules.Add(9  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 11 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =5  }});
                    SensorAnalysisRules.Add(10 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 12 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =6  }});
                    SensorAnalysisRules.Add(11 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 9  }});                                                                     
                    SensorAnalysisRules.Add(12 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 10 }});                                                                     
                    SensorAnalysisRules.Add(13 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 11 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =9  }});
                    SensorAnalysisRules.Add(14 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 12 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =10 }});
                    SensorAnalysisRules.Add(15 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 17 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =19 }});
                    SensorAnalysisRules.Add(16 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 18 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =20 }});
                    SensorAnalysisRules.Add(17 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 19 }});                                                                     
                    SensorAnalysisRules.Add(18 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 20 }});                                                                     
                    SensorAnalysisRules.Add(19 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 23 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =17 }});
                    SensorAnalysisRules.Add(20 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 24 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =18 }});
                    SensorAnalysisRules.Add(21 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 23 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =25 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 19 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 17 }});
                    SensorAnalysisRules.Add(22 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 24 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =26 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 20 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 18 }});
                    SensorAnalysisRules.Add(23 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 25 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =19 }});
                    SensorAnalysisRules.Add(24 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 26 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =20 }});
                    SensorAnalysisRules.Add(25 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 23 }});                                                                     
                    SensorAnalysisRules.Add(26 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 24 }});                                                                     
                    SensorAnalysisRules.Add(27 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 25 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =23 }});
                    SensorAnalysisRules.Add(28 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 26 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =24 }});
                    SensorAnalysisRules.Add(29 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 31 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =33 }});
                    SensorAnalysisRules.Add(30 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 32 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =34 }});
                    SensorAnalysisRules.Add(31 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 33 }});                                                                     
                    SensorAnalysisRules.Add(32 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 34 }});                                                                     
                    SensorAnalysisRules.Add(33 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 37 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =31 }});
                    SensorAnalysisRules.Add(34 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 38 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =32 }});
                    SensorAnalysisRules.Add(35 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 37 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =39 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 33 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 31 }});
                    SensorAnalysisRules.Add(36 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 38 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =40 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 34 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 32 }});
                    SensorAnalysisRules.Add(37 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 39 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =33 }});
                    SensorAnalysisRules.Add(38 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 40 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =34 }});
                    SensorAnalysisRules.Add(39 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 37 }});                                                                     
                    SensorAnalysisRules.Add(40 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 38 }});                                                                     
                    SensorAnalysisRules.Add(41 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 39 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =37 }});
                    SensorAnalysisRules.Add(42 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 40 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =38 }});
                    SensorAnalysisRules.Add(43 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 45 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =47 }});
                    SensorAnalysisRules.Add(44 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 46 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =48 }});
                    SensorAnalysisRules.Add(45 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 47 }});                                                                     
                    SensorAnalysisRules.Add(46 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 48 }});                                                                     
                    SensorAnalysisRules.Add(47 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 51 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =45 }});
                    SensorAnalysisRules.Add(48 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 52 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =46 }});
                    SensorAnalysisRules.Add(49 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 51 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =53 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 47 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 45 }});
                    SensorAnalysisRules.Add(50 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 52 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =54 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 48 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 46 }});
                    SensorAnalysisRules.Add(51 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 53 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =47 }});
                    SensorAnalysisRules.Add(52 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 54 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =48 }});
                    SensorAnalysisRules.Add(53 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 51 }});                                                                     
                    SensorAnalysisRules.Add(54 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 52 }});                                                                     
                    SensorAnalysisRules.Add(55 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 53 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =51 }});
                    SensorAnalysisRules.Add(56 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 54 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =52 }});
                    SensorAnalysisRules.Add(57 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 59 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =61 }});
                    SensorAnalysisRules.Add(58 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 60 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =62 }});
                    SensorAnalysisRules.Add(59 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 61 }});                                                                     
                    SensorAnalysisRules.Add(60 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 62 }});                                                                     
                    SensorAnalysisRules.Add(61 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 65 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =59 }});
                    SensorAnalysisRules.Add(62 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 66 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =60 }});
                    SensorAnalysisRules.Add(63 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 65 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =67 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 61 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 59 }});
                    SensorAnalysisRules.Add(64 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 66 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =68 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 62 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 60 }});
                    SensorAnalysisRules.Add(65 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 67 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =61 }});
                    SensorAnalysisRules.Add(66 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 68 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =62 }});
                    SensorAnalysisRules.Add(67 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 65 }});                                                                      
                    SensorAnalysisRules.Add(68 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 66 }});                                                                      
                    SensorAnalysisRules.Add(69 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 67 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =65 }});
                    SensorAnalysisRules.Add(70 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 68 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =66 }});
                    SensorAnalysisRules.Add(71 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 73 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =75 }});
                    SensorAnalysisRules.Add(72 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 74 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =76 }});
                    SensorAnalysisRules.Add(73 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 75 }});                                                                     
                    SensorAnalysisRules.Add(74 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 76 }});                                                                     
                    SensorAnalysisRules.Add(75 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 79 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =73 }});
                    SensorAnalysisRules.Add(76 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 80 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =74 }});
                    SensorAnalysisRules.Add(77 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 79 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =81 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 73 }});
                    SensorAnalysisRules.Add(78 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 80 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =82 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 76 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 74 }});
                    SensorAnalysisRules.Add(79 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 81 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =75 }});
                    SensorAnalysisRules.Add(80 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 82 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =76 }});
                    SensorAnalysisRules.Add(81 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 79 }});                                                                     
                    SensorAnalysisRules.Add(82 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 80 }});                                                                     
                    SensorAnalysisRules.Add(83 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 81 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =79 }});
                    SensorAnalysisRules.Add(84 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 82 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =80 }});
                    SensorAnalysisRules.Add(85 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 87 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =89 }});
                    SensorAnalysisRules.Add(86 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 88 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =90 }});
                    SensorAnalysisRules.Add(87 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 89 }});                                                                     
                    SensorAnalysisRules.Add(88 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 90 }});                                                                     
                    SensorAnalysisRules.Add(89 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 93 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =87 }});
                    SensorAnalysisRules.Add(90 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 94 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =88 }});
                    SensorAnalysisRules.Add(91 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 93 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =95 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 89 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 87 }});
                    SensorAnalysisRules.Add(92 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 94 },new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =96 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 90 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 88 }});
                    SensorAnalysisRules.Add(93 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 95 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =89 }});
                    SensorAnalysisRules.Add(94 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 96 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =90 }});
                    SensorAnalysisRules.Add(95 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 93 }});                                              
                    SensorAnalysisRules.Add(96 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 94 }});                                              
                    SensorAnalysisRules.Add(97 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 95 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =93 }});
                    SensorAnalysisRules.Add(98 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 96 },new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =94 }});
                    SensorAnalysisRules.Add(99 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 101},new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =103 }});
                    SensorAnalysisRules.Add(100, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 102},new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =104 }});
                    SensorAnalysisRules.Add(101, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 103}});                                              
                    SensorAnalysisRules.Add(102, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 104}});                                              
                    SensorAnalysisRules.Add(103, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 107},new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =101 }});
                    SensorAnalysisRules.Add(104, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2, EmittingSensor = 108},new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,    EmittingSensor =102 }});
                    SensorAnalysisRules.Add(105, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 107},new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =109 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 103 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 101 }});
                    SensorAnalysisRules.Add(106, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 108},new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,    EmittingSensor =110 },  new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 104 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 102 }});
                    SensorAnalysisRules.Add(107, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 109},new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =103 }});
                    SensorAnalysisRules.Add(108, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1, EmittingSensor = 110},new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =104 }});
                    SensorAnalysisRules.Add(109, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 107}});                                              
                    SensorAnalysisRules.Add(110, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 108}});                                              
                    SensorAnalysisRules.Add(111, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 109},new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =107 }});
                    SensorAnalysisRules.Add(112, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1, EmittingSensor = 110},new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,    EmittingSensor =108 }});
                    break;
                case 40:
                    SensorAnalysisRules = new Dictionary<int, SensorAnalysisRule[]>();
                    SensorAnalysisRules.Add(1,   new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 3  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 5  } });
                    SensorAnalysisRules.Add(2  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 4  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 6  } });
                    SensorAnalysisRules.Add(3  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 5  }});                                                                     
                    SensorAnalysisRules.Add(4  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 6  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 8  } });
                    SensorAnalysisRules.Add(5  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 9  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 3  } });
                    SensorAnalysisRules.Add(6  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 8  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 10 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 4} });
                    SensorAnalysisRules.Add(7  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 9  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 11 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 5}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 3} });
                    SensorAnalysisRules.Add(8  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 10 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 6  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2, EmittingSensor = 4} });
                    SensorAnalysisRules.Add(9  , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 11 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 5  } });
                    SensorAnalysisRules.Add(10 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 8  }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 6  } });
                    SensorAnalysisRules.Add(11 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 9  }});
                    SensorAnalysisRules.Add(12 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 10 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 8  } });
                    SensorAnalysisRules.Add(13 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 11 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 9  } });
                    SensorAnalysisRules.Add(14 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 16 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 18 } });
                    SensorAnalysisRules.Add(15 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 17 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 19 } });
                    SensorAnalysisRules.Add(16 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 18 } });                                                                    
                    SensorAnalysisRules.Add(17 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 19 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 21 } });
                    SensorAnalysisRules.Add(18 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 22 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 16 } });
                    SensorAnalysisRules.Add(19 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 21 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 23 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 17} });
                    SensorAnalysisRules.Add(20 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 22 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 24 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 18}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 16} });
                    SensorAnalysisRules.Add(21 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 23 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 19 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 17} });
                    SensorAnalysisRules.Add(22 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 24 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 18 } });
                    SensorAnalysisRules.Add(23 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 21 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 19 } });
                    SensorAnalysisRules.Add(24 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 22 } });                                                                    
                    SensorAnalysisRules.Add(25 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 23 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 21 } });
                    SensorAnalysisRules.Add(26 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 24 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 22 } });
                    SensorAnalysisRules.Add(27 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 29 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 31 } });
                    SensorAnalysisRules.Add(28 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 30 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 32 } });
                    SensorAnalysisRules.Add(29 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 31 }});                                                                     
                    SensorAnalysisRules.Add(30 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 32 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 34 } });
                    SensorAnalysisRules.Add(31 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 35 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 29 } });
                    SensorAnalysisRules.Add(32 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 34 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 36 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 30} });
                    SensorAnalysisRules.Add(33 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 35 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 37 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 31}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 29} });
                    SensorAnalysisRules.Add(34 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 36 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 32 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 30} });
                    SensorAnalysisRules.Add(35 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 37 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 31 } });
                    SensorAnalysisRules.Add(36 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 34 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 32 } });
                    SensorAnalysisRules.Add(37 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 35 } });                                                                    
                    SensorAnalysisRules.Add(38 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 36 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 34 } });
                    SensorAnalysisRules.Add(39 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 37 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 35 } });
                    SensorAnalysisRules.Add(40 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 42 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 44 } });
                    SensorAnalysisRules.Add(41 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 43 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 45 } });
                    SensorAnalysisRules.Add(42 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 44 }});                                                                     
                    SensorAnalysisRules.Add(43 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 45 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 47 } });
                    SensorAnalysisRules.Add(44 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 48 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 42 } });
                    SensorAnalysisRules.Add(45 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 47 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 49 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 43} });
                    SensorAnalysisRules.Add(46 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 48 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 50 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 44}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 42} });
                    SensorAnalysisRules.Add(47 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 49 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 45 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 43} });
                    SensorAnalysisRules.Add(48 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 50 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 44 } });
                    SensorAnalysisRules.Add(49 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 47 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 45 } });
                    SensorAnalysisRules.Add(50 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 48 } });                                                                    
                    SensorAnalysisRules.Add(51 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 49 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 47 } });
                    SensorAnalysisRules.Add(52 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 50 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 48 } });
                    SensorAnalysisRules.Add(53 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 55 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 57 } });
                    SensorAnalysisRules.Add(54 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 56 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 58 } });
                    SensorAnalysisRules.Add(55 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 57 } });                                                                    
                    SensorAnalysisRules.Add(56 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 58 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 60 } });
                    SensorAnalysisRules.Add(57 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 61 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 55 } });
                    SensorAnalysisRules.Add(58 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 60 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 62 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 56} });
                    SensorAnalysisRules.Add(59 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 61 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 63 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 57}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 55} });
                    SensorAnalysisRules.Add(60 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 62 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 58 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 56} });
                    SensorAnalysisRules.Add(61 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 63 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 57 } });
                    SensorAnalysisRules.Add(62 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 60 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 58 } });
                    SensorAnalysisRules.Add(63 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 61 } });                                                                    
                    SensorAnalysisRules.Add(64 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 62 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 60 } });
                    SensorAnalysisRules.Add(65 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 63 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 61 } });
                    SensorAnalysisRules.Add(66 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 68 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 70 } });
                    SensorAnalysisRules.Add(67 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 69 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 74 } });
                    SensorAnalysisRules.Add(68 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 70 } });                                                                    
                    SensorAnalysisRules.Add(69 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 71 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 73 } });
                    SensorAnalysisRules.Add(70 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 74 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 68 } });
                    SensorAnalysisRules.Add(71 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 73 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 69} });
                    SensorAnalysisRules.Add(72 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 73 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 70}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 68} });
                    SensorAnalysisRules.Add(73 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 71 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 69} });
                    SensorAnalysisRules.Add(74 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 76 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 70 } });
                    SensorAnalysisRules.Add(75 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 73 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 71 } });
                    SensorAnalysisRules.Add(76 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 74 } });                                                                    
                    SensorAnalysisRules.Add(77 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 75 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 73 } });
                    SensorAnalysisRules.Add(78 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 76 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 74 } });
                    SensorAnalysisRules.Add(79 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 81 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 83 } });
                    SensorAnalysisRules.Add(80 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 82 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 84 } });
                    SensorAnalysisRules.Add(81 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 83 } });                                                                    
                    SensorAnalysisRules.Add(82 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 84 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 86 } });
                    SensorAnalysisRules.Add(83 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 87 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 81 } });
                    SensorAnalysisRules.Add(84 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 86 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 88 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 82} });
                    SensorAnalysisRules.Add(85 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 87 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 89 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 83}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 81} });
                    SensorAnalysisRules.Add(86 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 88 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 84 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 82} });
                    SensorAnalysisRules.Add(87 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 89 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 83 } });
                    SensorAnalysisRules.Add(88 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 86 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 84 } });
                    SensorAnalysisRules.Add(89 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 87 } });                                                                    
                    SensorAnalysisRules.Add(90 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 88 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 86 } });
                    SensorAnalysisRules.Add(91 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 89 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 87 } });
                    SensorAnalysisRules.Add(92 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 94 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 96 } });
                    SensorAnalysisRules.Add(93 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 95 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 97 } });
                    SensorAnalysisRules.Add(94 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 96 } });                                                                    
                    SensorAnalysisRules.Add(95 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 97 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 99 } });
                    SensorAnalysisRules.Add(96 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 100}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 94 } });
                    SensorAnalysisRules.Add(97 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 99 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 101}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 95} });
                    SensorAnalysisRules.Add(98 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 100}, new SensorAnalysisRule() { BScan = EmaRuleEnum.R2,EmittingSensor = 102}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 96}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 94} });
                    SensorAnalysisRules.Add(99 , new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 101}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 97 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 95} } );
                    SensorAnalysisRules.Add(100, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.R1,EmittingSensor = 102}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 96 } });
                    SensorAnalysisRules.Add(101, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 99 }, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 97 } });
                    SensorAnalysisRules.Add(102, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 100} });                                                                    
                    SensorAnalysisRules.Add(103, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 101}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 99 } });
                    SensorAnalysisRules.Add(104, new[] { new SensorAnalysisRule() { BScan = EmaRuleEnum.L1,EmittingSensor = 102}, new SensorAnalysisRule() { BScan = EmaRuleEnum.L2,EmittingSensor = 100} });
                    break;
            }
        }
    }
}

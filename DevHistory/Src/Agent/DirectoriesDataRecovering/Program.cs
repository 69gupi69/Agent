using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace DirectoriesDataRecovering
{

    public class DataParser
    {
        private string agentConnectionString;

        private string dcprimaryConnectionString; 

        public DataParser()
        {
            Init();
        }

        private void Init()
        {
            agentConnectionString = ConfigurationManager.ConnectionStrings["AgentConnectionString"].ConnectionString;
            dcprimaryConnectionString = ConfigurationManager.ConnectionStrings["DCPrimaryConnectionString"].ConnectionString;
        }

        public class Calculation 
        {
            public Guid Id { get; set; }
            public string Item { get; set; }
            public Guid ContractorId { get; set; }
            public Guid PipeLineId { get; set; }
            public Guid RouteId { get; set; }
            public DateTime DateWorkItem { get; set; }
        }

        public class AccessoriesInformation
        {
            public Guid ContractorId { get; set; }
            public string ContractorName { get; set; }
            public Guid PipelineId { get; set; }
            public string PipelineName { get; set; }
            public Guid RouteId { get; set; }
            public string RouteName { get; set; }
        }     

        public List<AccessoriesInformation> GetAccessoriesInformation()
        {
            var result = new List<AccessoriesInformation>();
            using (var connection = new NpgsqlConnection(dcprimaryConnectionString))
            {
                var command = new NpgsqlCommand($@"SELECT contractor.""Id"" AS ""ContractorId"", 
                                                          contractor.""ShortName"" AS ""ContractorName"", 
                                                          pipeline.""Id"" AS ""PipelineId"", 
                                                          pipeline.""Name"" AS ""PipelineName"", 
                                                          route.""Id"" AS ""RouteId"", 
                                                          route.""Name"" AS ""RouteName"" 
                                                   FROM pipeline.""Route"" as route
                                                     JOIN pipeline.""Pipeline"" as pipeline on pipeline.""Id"" = route.""PipelineId""
                                                     JOIN pipeline.""ContractorRouteRef"" as crr on crr.""RouteId"" = route.""Id""
                                                     JOIN staff.staff_contractor_contractor as contractor on contractor.""Id"" = crr.""ContractorId""", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        result.Add(new AccessoriesInformation()
                        {
                            ContractorId = new Guid(reader["ContractorId"].ToString()),
                            ContractorName = reader["ContractorName"].ToString(),
                            PipelineId = new Guid(reader["PipelineId"].ToString()),
                            PipelineName = reader["PipelineName"].ToString(),
                            RouteId = new Guid(reader["RouteId"].ToString()),
                            RouteName = reader["RouteName"].ToString()
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error: " + e);
                    }
                }
                connection.Close();
            }
            return result;
        }

        public List<Calculation> GetCalculationsData()
        {
            var result = new List<Calculation>();
            using (var connection = new NpgsqlConnection(agentConnectionString))
            {
                var command = new NpgsqlCommand($"SELECT \"Id\", \"Item\", \"ContractorId\", \"PipeLineId\", \"RouteId\", \"DateWorkItem\" FROM data.\"Calculations\"", connection);
                connection.Open();
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    try
                    {
                        result.Add(new Calculation()
                        {
                            Id = new Guid(reader["Id"].ToString()),
                            Item = reader["Item"].ToString(),
                            ContractorId = new Guid(reader["ContractorId"].ToString()),
                            PipeLineId = new Guid(reader["PipeLineId"].ToString()),
                            RouteId = new Guid(reader["RouteId"].ToString()),
                            DateWorkItem = Convert.ToDateTime(reader["DateWorkItem"].ToString())
                        });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("error: " + e);
                    }
                }
                connection.Close();
            }
            return result;
        }

        public void UpdateCalculations()
        {
            try
            {
                var overwrittenCalculations = new List<Guid?>();
                var dcpDirs = GetAccessoriesInformation();
                var calculations = GetCalculationsData();
                Console.WriteLine("Замена СontractorId, PipeLineId, RouteId в строках таблицы data.\"Calculations\" с \"Id\": ");

                foreach (var calculation in calculations)
                {
                    var data = JObject.Parse(calculation.Item);
                    var agentRoute = data["DataOutput"]?["Route"];
                    var agentPipeLine = data["DataOutput"]?["PipeLine"];
                    var agentContractor = data["DataOutput"]?["Contractor"];
                    var calculationContractor = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(agentContractor?.ToString() ?? string.Empty,
                                        new { Key = (Guid?)null, Value = (string)null });
                    var calculationPipeLine = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(agentPipeLine?.ToString() ?? string.Empty,
                                        new { Key = (Guid?)null, Value = (string)null });
                    var calculationRoute = Newtonsoft.Json.JsonConvert.DeserializeAnonymousType(agentRoute?.ToString() ?? string.Empty,
                                        new { Key = (Guid?)null, Value = (string)null });
                    var dcpDir = dcpDirs.Find(x => (x.ContractorName == calculationContractor.Value && x.PipelineName == calculationPipeLine.Value && x.RouteName == calculationRoute.Value));
                    if (dcpDir != null)
                    {
                        var wrongСontractor = dcpDir.ContractorId != calculationContractor.Key || new Guid(data["DataOutput"]["Contractor"]["Key"].ToString()) != dcpDir.ContractorId;
                        var wrongPipeline = dcpDir.PipelineId != calculationPipeLine.Key || new Guid(data["DataOutput"]["PipeLine"]["Key"].ToString()) != dcpDir.PipelineId;
                        var wrongRoute = dcpDir.RouteId != calculationRoute.Key || new Guid(data["DataOutput"]["Route"]["Key"].ToString()) != dcpDir.RouteId;
                        if (wrongСontractor)
                        {
                            calculation.ContractorId = dcpDir.ContractorId;
                            data["DataOutput"]["Contractor"]["Key"] = dcpDir.ContractorId;
                        }
                        if (wrongPipeline)
                        {
                            calculation.PipeLineId = dcpDir.PipelineId;
                            data["DataOutput"]["PipeLine"]["Key"] = dcpDir.PipelineId;
                        }
                        if (wrongRoute)
                        {
                            calculation.RouteId = dcpDir.RouteId;
                            data["DataOutput"]["Route"]["Key"] = dcpDir.RouteId;
                        }
                        if (wrongСontractor || wrongPipeline || wrongRoute)
                        {
                            calculation.Item = data.ToString();
                            var result = PutCalculation(calculation);
                            if (result != null)
                            {
                                Console.WriteLine(result.ToString());
                                overwrittenCalculations.Add(result);
                            };
                        }
                    }
                    else
                    {
                        Console.WriteLine($"Для записи '{calculation.Id}' не найдено сопоставление ОСТ: {calculationContractor.Value}, Трубопровод: {calculationPipeLine.Value}, Участок: {calculationRoute.Value}");
                    }
                }

                if (overwrittenCalculations.Count > 0)
                {
                    var path = Directory.GetCurrentDirectory();
                    var subpath = path + "\\Logs";
                    var dirInfo = new DirectoryInfo(subpath);
                    if (!dirInfo.Exists)
                    {
                        dirInfo.Create();
                    }
                    var filePath = $"{subpath}\\OverwrittenCalcs-{DateTime.Now:yyyy-MM-dd HH-mm}.txt";

                    if (!File.Exists(filePath))
                    {
                        using (var sw = File.CreateText(filePath))
                        {
                            foreach (var id in overwrittenCalculations)
                            {
                                sw.WriteLine($"'{id.ToString()}'");
                            }
                            Console.WriteLine($"Текст записан в файл {filePath}");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e);
            }
        }

        public Guid? PutCalculation(Calculation calc)
        {
            try
            {
                using (var connection = new NpgsqlConnection(agentConnectionString))
                {
                    var cmd = new NpgsqlCommand($@"UPDATE data.""Calculations"" 
                                                   SET ""Id""=@Id, 
                                                       ""Item""=@Item, 
                                                       ""ContractorId""=@ContractorId, 
                                                       ""PipeLineId""=@PipeLineId, 
                                                       ""RouteId""=@RouteId, 
                                                       ""DateWorkItem""=@DateWorkItem
                                                   WHERE data.""Calculations"".""Id"" = @Id ;", connection);

                    connection.Open();
                    cmd.Parameters.AddWithValue("@Id", calc.Id);
                    cmd.Parameters.AddWithValue("@Item", NpgsqlTypes.NpgsqlDbType.Jsonb, calc.Item);
                    cmd.Parameters.AddWithValue("@ContractorId", calc.ContractorId);
                    cmd.Parameters.AddWithValue("@PipeLineId", calc.PipeLineId);
                    cmd.Parameters.AddWithValue("@RouteId", calc.RouteId);
                    cmd.Parameters.AddWithValue("@DateWorkItem", calc.DateWorkItem);

                    cmd.ExecuteNonQuery();
                    connection.Close();
                    return calc.Id;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("error: " + e);
            }
            return null;
        }

    }
    public class Program
    {


        public static void Main(string[] args)
        {
            var deserialize = new DataParser();
            deserialize.UpdateCalculations();
            Console.WriteLine("Программа завершила работу.");
            Console.ReadKey();
        }
    }
}

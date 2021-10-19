using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using NpgsqlTypes;
using Newtonsoft.Json;
using Diascan.Agent.Types.ModelCalculationDiagData;


namespace DataBaseMigration
{
    public class Migration
    {
        public string         ConnectionString { get; set; }
        public List<CalcHead> CalcHeads        { get; set; }
        public JsonSerializerSettings Settings { get; set; }

        public Migration(string connectionString)
        {
            ConnectionString = connectionString;
            CalcHeads = new List<CalcHead>();
            Settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead };
        }

        public void DoWorck()
        {
            if (!GetDeserialize()) return;
            if (!DeleteCalculations()) return;
            if (!InsertJson()) return;
        }

        private bool GetDeserialize()
        {
            var i = 0;
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT * FROM data.\"Calculations\"", connection);
                    connection.Open();
                    Console.WriteLine($"Соединение с БД: ОТКРЫТО");
                    using (var reader = сommand.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var currentDateTime = DateTime.Parse(reader[5].ToString());


                            var json = reader[1].ToString();
                            json = json.Replace("ModelDB", "Types");
                            json = json.Replace("EndDist", "StopDist");
                            var calculation = JsonConvert.DeserializeObject<Calculation>(json, Settings);
                            json = JsonConvert.SerializeObject(calculation, Settings);
                            CalcHeads.Add(new CalcHead()
                            {
                                Id           = (Guid)reader[0],
                                Item         = json,
                                ContractorId = (Guid)reader[2],
                                PipeLineId   = (Guid)reader[3],
                                RouteId      = (Guid)reader[4],
                                DateWorkItem = currentDateTime
                            });
                            Console.WriteLine(++i);
                        }
                    }

                    Console.WriteLine($"Количество прочитанных и преобразованных данных = {i}");
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Console.WriteLine($"Соединение с БД: ЗАКРЫТО");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR Get Deserialize!!! ");
                Console.WriteLine(e);
                return false;
            }
            return true;
        }

        private bool DeleteCalculations()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    
                    var sqlDELETE = $"DELETE FROM data.\"Calculations\";";
                    var command = new NpgsqlCommand(sqlDELETE, connection);
                    command.ExecuteNonQuery();
                    Console.WriteLine($"Очистка таблицы Calculations");
                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR Delete Calculations!!! ");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }


        private bool InsertJson()
        {
            try
            {
                using (var connection = new NpgsqlConnection(ConnectionString))
                {
                    connection.Open();
                    Console.WriteLine($"Соединение с БД: ОТКРЫТО");
                    var i = 0;
                    using (var writer = connection.BeginBinaryImport("COPY data.\"Calculations\" (\"Id\", \"Item\",\"ContractorId\", \"PipeLineId\",\"RouteId\", \"DateWorkItem\") FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (var CalcHead in CalcHeads)
                        {
                            writer.StartRow();
                            writer.Write(CalcHead.Id,           NpgsqlDbType.Uuid);
                            writer.Write(CalcHead.Item,         NpgsqlDbType.Jsonb);
                            writer.Write(CalcHead.ContractorId, NpgsqlDbType.Uuid); // Контракторы
                            writer.Write(CalcHead.PipeLineId,   NpgsqlDbType.Uuid); // Трубопроводы
                            writer.Write(CalcHead.RouteId,      NpgsqlDbType.Uuid); // Участок
                            writer.Write(CalcHead.DateWorkItem, NpgsqlDbType.Date); // Дата пропуска
                            Console.WriteLine($" Uuid: {CalcHead.Id} Контракторы:{CalcHead.ContractorId} Трубопроводы: {CalcHead.PipeLineId} Участок: {CalcHead.RouteId} Дата пропуска: {CalcHead.DateWorkItem}");
                            i++;
                        }
                        writer.Complete();
                    }
                    Console.WriteLine($"Количество внесенных данных в таблицу = {i}");
                    if (connection.State == ConnectionState.Open)
                    {
                        connection.Close();
                        Console.WriteLine($"Соединение с БД: ЗАКРЫТО");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"ERROR Insert Json!!! ");
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }

    public class CalcHead
    {
        public Guid     Id           { get; set; }
        public string   Item         { get; set; }
        public Guid     ContractorId { get; set; } 
        public Guid     PipeLineId   { get; set; }
        public Guid     RouteId      { get; set; }
        public DateTime DateWorkItem { get; set; }
    }
}

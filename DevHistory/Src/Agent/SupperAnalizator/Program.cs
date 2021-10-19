using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Diascan.Agent.LiteDbAccess;
using Diascan.Agent.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using SupperAnalizator;

namespace SupperAnalizator
{
    public class Deserialize
    {
        private string connectionString =
            "server=10.209.81.239;Port=5432;database=DBAgent;User Id = agent; Password=Pa3b487fo;Convert Infinity DateTime=true;";
        private AccessManager accessManager;

        public Deserialize()
        {
            Init();
        }

        private void Init()
        {
            accessManager = new AccessManager();
        }

        
        public void GetDeserialize()
        {
            accessManager.DiascanAgentAccess.OpenConnection();
            accessManager.DiascanAgentAccess.DeleteDb();
            var i = 0;
            var settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto, MetadataPropertyHandling = MetadataPropertyHandling.ReadAhead };
            using (var connection = new NpgsqlConnection(connectionString))
            {
                var сommand = new NpgsqlCommand($"SELECT * FROM data.\"Calculations\"", connection);
                connection.Open();
                using (var reader = сommand.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var currentDateTime = DateTime.Parse(reader[5].ToString());

                        if (currentDateTime < new DateTime(2019, 3, 1))
                            continue;

                        try
                        {
                            var text = reader[1].ToString().Replace("ModelDB", "Types");
                            var calculation = JsonConvert.DeserializeObject<Calculation>(text, settings);
                            accessManager.DiascanAgentAccess.CalculationAccess.Insert(calculation);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                        Console.WriteLine(++i);
                    }
                }
                Console.WriteLine("Aus");
                accessManager.DiascanAgentAccess.CloseConnection();
                connection.Close();
            }
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            var deserialize = new Deserialize();
            deserialize.GetDeserialize();

            Console.ReadKey();
        }
    }
}

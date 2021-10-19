using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Npgsql;

namespace Deserialize
{
    public class Deserialize
    {
        private string connectionString =
            "server=10.209.81.239;Port=5432;database=DBAgent;User Id = agent; Password=Pa3b487fo;Convert Infinity DateTime=true;";

        private Dictionary<Guid, (JObject, DateTime)> data;
        private string allObject;
        private List<string> list;

        public void GetDeserialize()
        {
            list = new List<string>();
            data = new Dictionary<Guid, (JObject, DateTime)>();
            try
            {
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    var сommand = new NpgsqlCommand($"SELECT * FROM data.\"Calculations\"", connection);
                    connection.Open();
                    using (var reader = сommand.ExecuteReader())
                    {
                        var i = 0;
                        while (reader.Read())
                        {
                            data.Add(new Guid(reader[0].ToString()), (JObject.Parse(reader[1].ToString()), DateTime.Parse(reader[5].ToString())));
                            i++;
                        }
                    }

                    connection.Close();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Ошибка: " + e.Message);
            }


            foreach (var d in data)
            {
                foreach (var objCh in d.Value.Item1)
                {
                    R(objCh.Value);
                }
                list.Add(allObject);
                allObject = "";
            }
            
        }

        public void R(JToken obj)
        {
            switch (obj.Type)
            {
                case JTokenType.Property:
                    foreach (var objCh in obj)
                    {
                        R(objCh);
                    }
                    break;
                case JTokenType.Object:
                    foreach (var objCh in obj)
                    {
                        R(objCh);
                    }
                    break;
                case JTokenType.Array:
                    if(obj.Count() != 0)
                        R(obj[0]);
                    break;
                default:
                    allObject += $"{obj.Path}({obj.Type})\n";
                    return;
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

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseMigration
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var migration = new Migration(ConfigurationManager.ConnectionStrings["AgentConnectionString"].ConnectionString);
            migration.DoWorck();
            Console.ReadKey();
        }
    }
}

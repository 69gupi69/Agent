using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diascan.Agent.ModelDB
{
    public class TableRowData
    {
        public int Id { get; set; }
        public string RunCode { get; set; }
        public string Path { get; set; }
        public string State { get; set; }
        public string Status { get; set; }
        public bool Restart { get; set; }
        public DateTime DateTime { get; set; }
    }
}

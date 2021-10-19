using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diascan.Agent.DirectoryDataModel
{
    public class Contractor
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public Contractor() { }

        public Contractor(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

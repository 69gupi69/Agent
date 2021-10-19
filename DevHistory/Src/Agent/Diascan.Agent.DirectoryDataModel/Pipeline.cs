using System;

namespace Diascan.Agent.DirectoryDataModel
{
    public class Pipeline
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Pipeline() { }

        public Pipeline(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Dynamic;

namespace RoverMob.Protocol
{
    public class MessageMemento
    {
        public List<string> Topics { get; set; }
        public string Hash { get; set; }
        public string MessageType { get; set; }
        public List<PredecessorMemento> Predecessors { get; set; }
        public Guid ObjectId { get; set; }
        public ExpandoObject Body { get; set; }
    }
}

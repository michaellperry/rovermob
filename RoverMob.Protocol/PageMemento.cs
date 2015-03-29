using System.Collections.Generic;

namespace RoverMob.Protocol
{
    public class PageMemento
    {
        public string Bookmark { get; set; }
        public List<MessageMemento> Messages { get; set; }
    }
}

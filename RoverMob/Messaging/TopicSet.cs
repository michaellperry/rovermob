using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class TopicSet
    {
        private List<string> _topics = new List<string>();

        public TopicSet Add(string topic)
        {
            if (!string.IsNullOrEmpty(topic))
                _topics.Add(topic);
            return this;
        }

        public ImmutableList<string> ToImmutableList()
        {
            return _topics.ToImmutableList();
        }
    }
}

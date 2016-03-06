using RoverMob.Protocol;

namespace RoverMob.Distributor.Filters
{
    public class MessagesInTopic : IFilter
    {
        private readonly string _topic;

        public MessagesInTopic(string topic)
        {
            _topic = topic;
        }

        public bool Accepts(string topic, MessageMemento message)
        {
            return topic == _topic;
        }
    }
}

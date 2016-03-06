using RoverMob.Protocol;

namespace RoverMob.Distributor.Filters
{
    public class MessagesOfType : IFilter
    {
        private readonly string _messageType;

        public MessagesOfType(string messageType)
        {
            _messageType = messageType;
        }

        public bool Accepts(string topic, MessageMemento message)
        {
            return message.MessageType == _messageType;
        }
    }
}

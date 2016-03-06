using RoverMob.Protocol;
using System.Linq;

namespace RoverMob.Distributor.Filters
{
    public class MessagesOfType : IFilter
    {
        private readonly string[] _messageTypes;

        public MessagesOfType(params string[] messageTypes)
        {
            _messageTypes = messageTypes;
        }

        public bool Accepts(string topic, MessageMemento message)
        {
            return _messageTypes.Contains(message.MessageType);
        }
    }
}

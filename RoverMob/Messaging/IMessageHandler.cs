using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoverMob.Messaging
{
    public interface IMessageHandler
    {
        Guid GetObjectId();
        void HandleMessage(Message message);
        void HandleAllMessages(IEnumerable<Message> messages);
        IEnumerable<IMessageHandler> Children { get; }
    }
}

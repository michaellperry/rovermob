using System;
using System.Collections.Generic;

namespace RoverMob.Messaging
{
    class MessageDispatcher<T>
    {
        private Dictionary<string, Action<T, Message>> _handlerByMessageType =
            new Dictionary<string,Action<T,Message>>();

        public MessageDispatcher<T> On(string messageType, Action<T, Message> handler)
        {
            _handlerByMessageType.Add(messageType, handler);
            return this;
        }

        public void Dispatch(T target, Message message)
        {
            Action<T, Message> handler;
            if (_handlerByMessageType.TryGetValue(message.Type, out handler))
                handler(target, message);
        }
    }
}

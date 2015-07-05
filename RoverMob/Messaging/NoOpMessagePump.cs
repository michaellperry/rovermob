using System;
using System.Collections.Immutable;
using System.Collections.Generic;

namespace RoverMob.Messaging
{
    public class NoOpMessagePump : IMessagePump
    {
        public event MessageReceivedHandler MessageReceived;

        public void Subscribe(Func<IEnumerable<string>> subscription)
        {
        }

        public void Enqueue(Message message)
        {
        }

        public void SendAndReceiveMessages()
        {
        }

        public void SendAllMessages(ImmutableList<Message> messages)
        {
        }

        public bool Busy
        {
            get { return false; }
        }

        public Exception Exception
        {
            get { return null; }
        }
    }
}

using System;
using System.Collections.Immutable;

namespace RoverMob.Messaging
{
    public delegate void MessageReceivedHandler(Message message);

    public interface IMessagePump
    {
        event MessageReceivedHandler MessageReceived;

        void Subscribe(string topic);
        void Enqueue(Message message);
        void SendAndReceiveMessages();
        void SendAllMessages(ImmutableList<Message> messages);
        bool Busy { get; }
        Exception Exception { get; }
    }
}

using System;
using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;

namespace RoverMob.Messaging
{
    public delegate void MessageReceivedHandler(Message message);

    public interface IMessagePump
    {
        event MessageReceivedHandler MessageReceived;

        void Subscribe(Func<IEnumerable<string>> subscription);
        void Enqueue(Message message);
        void SendAndReceiveMessages();
        void SendAllMessages(ImmutableList<Message> messages);
        bool Busy { get; }
        Exception Exception { get; }
    }

    public static class MessagePumpExtensions
    {
        public static void Subscribe(
            this IMessagePump messagePump,
            Func<string> subscription)
        {
            messagePump.Subscribe(delegate()
            {
                return Enumerable.Repeat(subscription(), 1)
                    .Where(t => !string.IsNullOrEmpty(t));
            });
        }
    }
}

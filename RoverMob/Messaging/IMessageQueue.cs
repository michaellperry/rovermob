using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public interface IMessageQueue
    {
        Task<ImmutableList<Message>> LoadAsync();
        void Enqueue(Message message);
        void Confirm(Message message);
        Exception Exception { get; }
    }
}

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpMessageQueue : IMessageQueue
    {
        public NoOpMessageQueue()
        {
        }

        public Task<ImmutableList<Message>> LoadAsync()
        {
            return Task.FromResult(ImmutableList<Message>.Empty);
        }

        public void Enqueue(Message message)
        {
        }

        public void Confirm(Message message)
        {
        }

        public Exception Exception
        {
            get
            {
                return null;
            }
        }
    }
}

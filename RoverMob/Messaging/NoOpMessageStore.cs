using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpMessageStore : IMessageStore
    {
        public Task<ImmutableList<Message>> LoadAsync(Guid objectId)
        {
            return Task.FromResult(ImmutableList<Message>.Empty);
        }

        public void Save(Message message)
        {
        }
    }
}

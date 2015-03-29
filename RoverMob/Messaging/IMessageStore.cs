using System.Threading.Tasks;
using System.Collections.Immutable;
using System;

namespace RoverMob.Messaging
{
    public interface IMessageStore
    {
        Task<ImmutableList<Message>> LoadAsync(Guid objectId);
        void Save(Message message);
    }
}

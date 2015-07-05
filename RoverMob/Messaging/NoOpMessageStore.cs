using System;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpMessageStore : IMessageStore
    {
        private ImmutableDictionary<Guid, ImmutableList<Message>> _messagesByObjectId =
            ImmutableDictionary<Guid, ImmutableList<Message>>.Empty;

        public Task<ImmutableList<Message>> LoadAsync(Guid objectId)
        {
            var messages = _messagesByObjectId.GetValueOrDefault(
                objectId, ImmutableList<Message>.Empty);
            return Task.FromResult(messages);
        }

        public void Save(Message message)
        {
            var objectId = message.ObjectId;
            var messages = _messagesByObjectId.GetValueOrDefault(
                objectId, ImmutableList<Message>.Empty);
            _messagesByObjectId = _messagesByObjectId.SetItem(
                objectId, messages.Add(message));
        }

        public Task<Guid?> GetUserIdentifierAsync(string role)
        {
            return Task.FromResult((Guid?)Guid.NewGuid());
        }

        public void SaveUserIdentifier(string role, Guid userIdentifier)
        {
        }
    }
}

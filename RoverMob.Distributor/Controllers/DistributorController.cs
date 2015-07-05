using RoverMob.Distributor.Storage;
using RoverMob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RoverMob.Distributor.Notification;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Controllers
{
    [Authorize]
    public abstract class DistributorController : ApiController
    {
        private readonly AzureStorageProvider _storage;
        private readonly AzurePushNotificationProvider _pushNotification;

        protected DistributorController(
            string storageConnectionString,
            string notificationConnectionString)
        {
            _storage = new AzureStorageProvider(storageConnectionString);
            _pushNotification = new AzurePushNotificationProvider(notificationConnectionString);
        }

        public async Task Post(string topic, [FromBody]MessageMemento message)
        {
            string userId = this.User.Identity.Name;
            bool authorized = await AuthorizeUserForPost(topic, userId);
            if (!authorized)
                throw new UnauthorizedAccessException();

            _storage.WriteMessage(topic, message);
            await _pushNotification.SendNotificationAsync(topic, message);
        }

        public async Task<PageMemento> Get(string topic, string bookmark)
        {
            string userId = this.User.Identity.Name;
            bool authorized = await AuthorizeUserForGet(topic, userId);
            if (!authorized)
                throw new UnauthorizedAccessException();

            return _storage.ReadMessages(topic, bookmark);
        }

        protected abstract Task<bool> AuthorizeUserForPost(
            string topic, string userId);
        protected abstract Task<bool> AuthorizeUserForGet(
            string topic, string userId);

        protected Guid GetUserIdentifier(string role, string userId)
        {
            return _storage.GetUserIdentifier(role, userId);
        }

        protected IEnumerable<dynamic> GetMessagesInTopic(
            string topic,
            string createdByMessageType,
            string removedByMessageType,
            string removedByRole)
        {
            var messages = GetMessagesInTopic(topic).ToList();
            var predecessors = messages
                .Where(m => m.MessageType == removedByMessageType)
                .SelectMany(m => m.Predecessors)
                .Where(p => p.Role == removedByRole)
                .Select(p => p.Hash)
                .ToDictionary(h => h);
            return messages
                .Where(m =>
                    m.MessageType == createdByMessageType &&
                    !predecessors.ContainsKey(m.Hash))
                .Select(m => m.Body);
        }

        private IEnumerable<MessageMemento> GetMessagesInTopic(string topic)
        {
            string bookmark = string.Empty;
            while (true)
            {
                PageMemento page = _storage.ReadMessages(topic, bookmark);
                if (!page.Messages.Any())
                    break;

                foreach (var message in page.Messages)
                    yield return message;

                bookmark = page.Bookmark;
            }
        }
    }
}
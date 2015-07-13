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
using System.Diagnostics.Tracing;

namespace RoverMob.Distributor.Controllers
{
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

        public async Task<HttpResponseMessage> Post(string topic, [FromBody]MessageMemento message)
        {
            string userId = this.User != null
                ? this.User.Identity != null
                    ? this.User.Identity.Name
                    : null
                : null;
            bool authorized = await AuthorizeUserForPost(topic, userId);
            if (authorized)
            {
                _storage.WriteMessage(topic, message);
                try
                {
                    await _pushNotification.SendNotificationAsync(topic, message);
                }
                catch (Exception ex)
                {
                    // If push notifications fail, don't worry about it.
                }
            }

            // If the user is not authorized, it's likely a duplicate message.
            return Request.CreateResponse();
        }

        public async Task<PageMemento> Get(string topic, string bookmark)
        {
            string userId = this.User != null
                ? this.User.Identity != null
                    ? this.User.Identity.Name
                    : null
                : null;
            bool authorized = await AuthorizeUserForGet(topic, userId);
            if (authorized)
                return _storage.ReadMessages(topic, bookmark);
            else
                // If the user is not authorized, he will learn
                // about it via another topic.
                return new PageMemento
                {
                    Bookmark = bookmark,
                    Messages = new List<MessageMemento>()
                };
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
                .Distinct()
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
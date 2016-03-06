using RoverMob.Distributor.Dispatchers;
using RoverMob.Distributor.Filters;
using RoverMob.Distributor.Storage;
using RoverMob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace RoverMob.Distributor.Controllers
{
    public abstract class DistributorController : ApiController
    {
        struct Handler
        {
            public IFilter Filter;
            public IDispatcher Dispatcher;
        }

        private readonly AzureStorageProvider _storage;

        private List<Handler> _handlers = new List<Handler>();

        protected DistributorController(
            string storageConnectionString)
        {
            _storage = new AzureStorageProvider(storageConnectionString);
        }

        [Obsolete("Use the constructor with one parameter, and add dispatchers individually")]
        protected DistributorController(
            string storageConnectionString,
            string notificationConnectionString,
            string serviceBusConnectionString,
            string serviceBusPath) :
            this(storageConnectionString)
        {
            AddDispatcher(new AzureNotificationHubDispatcher(notificationConnectionString));
            AddDispatcher(new AzureServiceBusQueueDispatcher(serviceBusConnectionString, serviceBusPath));
        }

        public void AddDispatcher(IDispatcher dispatcher)
        {
            AddDispatcher(AllMessages.Instance, dispatcher);
        }

        public void AddDispatcher(IFilter filter, IDispatcher dispatcher)
        {
            _handlers.Add(new Handler { Filter = filter, Dispatcher = dispatcher });
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
                await Task.WhenAll(_handlers
                    .Where(h => h.Filter.Accepts(topic, message))
                    .Select(h => h.Dispatcher.DispatchAsync(topic, message)));
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
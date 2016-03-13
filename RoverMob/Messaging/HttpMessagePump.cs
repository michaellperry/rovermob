using RoverMob.Protocol;
using RoverMob.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Assisticant.Fields;
using Assisticant.Collections;
using RoverMob.Implementation;
using System.Runtime.InteropServices;

namespace RoverMob.Messaging
{
    public class HttpMessagePump : Process, IMessagePump
    {
        public event MessageReceivedHandler MessageReceived;

        private readonly Uri _uri;
        private readonly IMessageQueue _messageQueue;
        private readonly IBookmarkStore _bookmarkStore;
        private readonly IAccessTokenProvider _accessTokenProvider;
        private readonly IPushNotificationSubscription _pushNotificationSubscription;

        private ImmutableQueue<Message> _queue = ImmutableQueue<Message>.Empty;

        private ObservableList<Func<IEnumerable<string>>> _subscriptions = new ObservableList<Func<IEnumerable<string>>>();
        private Computed<List<string>> _topics;
        private ComputedSubscription _updateTopics;
        private Dictionary<string, string> _bookmarkByTopic = new Dictionary<string, string>();
        
        public HttpMessagePump(
            Uri uri,
            IMessageQueue messageQueue,
            IBookmarkStore bookmarkStore,
            IAccessTokenProvider accessTokenProvider = null,
            IPushNotificationSubscription pushNotificationSubscription = null)
        {
            _uri = uri;
            _messageQueue = messageQueue;
            _bookmarkStore = bookmarkStore;
            _accessTokenProvider = accessTokenProvider;
            _pushNotificationSubscription = pushNotificationSubscription;

            _topics = new Computed<List<string>>(() => _subscriptions.SelectMany(s => s()).ToList());
            _updateTopics = _topics.Subscribe(OnTopicsChanged);
        }

        public string ApiKey { get; set; }

        public void Subscribe(Func<IEnumerable<string>> subscription)
        {
            _subscriptions.Add(subscription);
        }

        public void SendAllMessages(ImmutableList<Message> messages)
        {
            lock (this)
            {
                foreach (var message in messages)
                    _queue = _queue.Enqueue(message);
            }
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        public void Enqueue(Message message)
        {
            lock (this)
            {
                _queue = _queue.Enqueue(message);
            }
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        public void SendAndReceiveMessages()
        {
            Perform(() => SendAndReceiveMessagesInternalAsync());
        }

        private void OnTopicsChanged(List<string> t)
        {
            if (_pushNotificationSubscription != null)
                _pushNotificationSubscription.Subscribe(t);
            SendAndReceiveMessages();
        }

        private async Task SendAndReceiveMessagesInternalAsync()
        {
            int attempts = 2;
            while (true)
            {
                attempts--;
                try
                {
                    string accessToken;
                    if (_accessTokenProvider != null)
                        accessToken = await _accessTokenProvider.GetAccessTokenAsync();
                    else
                        accessToken = null;
                    using (var client = await HttpImplementation.CreateProxyAsync(
                        accessToken, ApiKey))
                    {
                        await SendMessagesInternalAsync(client);

                        foreach (var topic in _topics.Value)
                            await ReceiveMessagesInternalAsync(client, topic);
                    }
                    break;
                }
                catch (COMException x) when (
                    x.HResult == unchecked((int)0x80190191) &&
                    _accessTokenProvider != null &&
                    attempts > 0)
                {
                    _accessTokenProvider.RefreshAccessToken();
                }
                break;
            }
        }

        private async Task SendMessagesInternalAsync(HttpProxy client)
        {
            while (true)
            {
                var queue = _queue;
                if (queue.IsEmpty)
                    return;
                var message = queue.Peek();

                await SendMessageAsync(client, message);

                lock (this)
                {
                    _queue = _queue.Dequeue();
                }
                _messageQueue.Confirm(message);
            }
        }

        private async Task SendMessageAsync(HttpProxy client, Message message)
        {
            var json = JsonConvert.SerializeObject(message.GetMemento());

            foreach (var topic in message.Topics)
            {
                var resourceUri = new Uri(_uri, topic);
                await client.PostJsonAsync(resourceUri, json);
            }
        }

        private async Task ReceiveMessagesInternalAsync(HttpProxy client, string topic)
        {
            string bookmark;
            if (!_bookmarkByTopic.TryGetValue(topic, out bookmark))
                bookmark = await _bookmarkStore.LoadBookmarkAsync(topic);

            while (true)
            {
                Uri resourceUri = new Uri(_uri, String.Format("{0}?bookmark={1}", topic, bookmark));
                var jsonString = await client.GetJsonAsync(resourceUri);
                var page = JsonConvert.DeserializeObject<PageMemento>(jsonString);
                if (page.Messages.Count == 0)
                    return;

                if (MessageReceived != null)
                    foreach (var message in page.Messages)
                        MessageReceived(Message.FromMemento(message));

                bookmark = page.Bookmark;
                _bookmarkByTopic[topic] = bookmark;
                await _bookmarkStore.SaveBookmarkAsync(topic, bookmark);
            }
        }
    }
}

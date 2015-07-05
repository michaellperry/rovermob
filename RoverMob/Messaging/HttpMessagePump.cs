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
using Windows.Web.Http;
using Windows.Web.Http.Headers;
using Windows.Web.Http.Filters;
using Assisticant.Fields;
using Assisticant.Collections;

namespace RoverMob.Messaging
{
    public class HttpMessagePump : Process, IMessagePump
    {
        public event MessageReceivedHandler MessageReceived;

        private readonly Uri _uri;
        private readonly IMessageQueue _messageQueue;
        private readonly IBookmarkStore _bookmarkStore;
        private readonly IAccessTokenProvider _accessTokenProvider;

        private ImmutableQueue<Message> _queue = ImmutableQueue<Message>.Empty;

        private ObservableList<Func<IEnumerable<string>>> _subscriptions = new ObservableList<Func<IEnumerable<string>>>();
        private Computed<List<string>> _topics;
        private ComputedSubscription _updateTopics;
        private Dictionary<string, string> _bookmarkByTopic = new Dictionary<string, string>();
        
        public HttpMessagePump(
            Uri uri,
            IMessageQueue messageQueue,
            IBookmarkStore bookmarkStore,
            IAccessTokenProvider accessTokenProvider = null)
        {
            _uri = uri;
            _messageQueue = messageQueue;
            _bookmarkStore = bookmarkStore;
            _accessTokenProvider = accessTokenProvider;

            _topics = new Computed<List<string>>(() => _subscriptions.SelectMany(s => s()).ToList());
            _updateTopics = _topics.Subscribe(_ => SendAndReceiveMessages());
        }

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

        private async Task SendAndReceiveMessagesInternalAsync()
        {
            try
            {
                var httpBaseFilder = new HttpBaseProtocolFilter
                {
                    AllowUI = false
                };
                using (HttpClient client = new HttpClient(httpBaseFilder))
                {
                    if (_accessTokenProvider != null)
                    {
                        string accessToken = await _accessTokenProvider
                            .GetAccessTokenAsync();
                        client.DefaultRequestHeaders.Authorization =
                            new HttpCredentialsHeaderValue(
                                "Bearer", accessToken);
                    }
                    await SendMessagesInternalAsync(client);

                    foreach (var topic in _topics.Value)
                        await ReceiveMessagesInternalAsync(client, topic);
                }
            }
            catch (Exception x)
            {
                if (_accessTokenProvider != null)
                    _accessTokenProvider.RefreshAccessToken();
                throw;
            }
        }

        private async Task SendMessagesInternalAsync(HttpClient client)
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

        private async Task SendMessageAsync(HttpClient client, Message message)
        {
            var json = JsonConvert.SerializeObject(message.GetMemento());
            using (var content = new HttpStringContent(
		        json,
		        Windows.Storage.Streams.UnicodeEncoding.Utf8,
		        "application/json"))
            {
                foreach (var topic in message.Topics)
                {
                    var resourceUri = new Uri(_uri, topic);
                    var response = await client.PostAsync(resourceUri, content);
                    response.EnsureSuccessStatusCode();
                }
            }
        }

        private async Task ReceiveMessagesInternalAsync(HttpClient client, string topic)
        {
            string bookmark;
            if (!_bookmarkByTopic.TryGetValue(topic, out bookmark))
                bookmark = await _bookmarkStore.LoadBookmarkAsync(topic);

            while (true)
            {
                Uri resourceUri = new Uri(_uri, String.Format("{0}?bookmark={1}", topic, bookmark));
                var response = await client.GetAsync(resourceUri);
                response.EnsureSuccessStatusCode();

                var jsonString = await response.Content.ReadAsStringAsync();
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

using Assisticant.Collections;
using Assisticant.Fields;
using RoverMob.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace RoverMob
{
    public class Application<TRoot>
        where TRoot : IMessageHandler
    {
        private readonly IMessageStore _messageStore;
        private readonly IMessageQueue _messageQueue;
        private readonly IMessagePump _messagePump;
        private readonly IPushNotificationSubscription _pushNotificationSubscription;
        private readonly IUserProxy _userProxy;

        private Observable<TRoot> _root = new Observable<TRoot>();
        private ComputedDictionary<Guid, IMessageHandler> _messageHandlers;

        private Observable<Exception> _exception = new Observable<Exception>();
        
        public Application()
            : this(
                new NoOpMessageStore(),
                new NoOpMessageQueue(),
                new NoOpMessagePump(),
                new NoOpPushNotificationSubscription(),
                new NoOpUserProxy())
        {
        }

        public Application(
            IMessageStore messageStore,
            IMessageQueue messageQueue,
            IMessagePump messagePump,
            IPushNotificationSubscription pushNotificationSubscription,
            IUserProxy userProxy)
        {
            _messageStore = messageStore;
            _messageQueue = messageQueue;
            _messagePump = messagePump;
            _pushNotificationSubscription = pushNotificationSubscription;
            _userProxy = userProxy;

            _messagePump.MessageReceived += MessageReceived;
            _pushNotificationSubscription.MessageReceived += NotificationReceived;

            _messageHandlers = new ComputedDictionary<Guid, IMessageHandler>(() =>
                Decendants(_root.Value)
                    .Distinct()
                    .ToDictionary(m => m.GetObjectId()));
        }

        public async void GetUserIdentifier(string role, Action<Guid> callback)
        {
            try
            {
                Guid? localUserIdentifier = await _messageStore
                    .GetUserIdentifierAsync(role);
                if (localUserIdentifier != null)
                    callback(localUserIdentifier.Value);
                else
                {
                    Guid userIdentifier = await _userProxy
                        .GetUserIdentifier(role);
                    _messageStore.SaveUserIdentifier(role, userIdentifier);
                    callback(userIdentifier);
                }
            }
            catch (Exception x)
            {
                _exception.Value = x;
            }
        }

        public async void Load(TRoot root)
        {
            try
            {
                _root.Value = root;

                LoadNewObjects(ImmutableList<IMessageHandler>.Empty);

                var queueMessages = await _messageQueue.LoadAsync();
                _messagePump.SendAllMessages(queueMessages);
            }
            catch (Exception ex)
            {
                _exception.Value = ex;
            }
        }

        public TRoot Root
        {
            get { return _root; }
        }

        public Exception Exception
        {
            get
            {
                return
                    _exception.Value ??
                    _messageQueue.Exception ??
                    _messagePump.Exception;
            }
        }

        public void SendAndReceiveMessages()
        {
            _messagePump.SendAndReceiveMessages();
        }

        public void EmitMessage(Message message)
        {
            _messageStore.Save(message);
            _messageQueue.Enqueue(message);
            _messagePump.Enqueue(message);
            HandleMessage(message);
        }

        private void MessageReceived(Message message)
        {
            _messageStore.Save(message);
            HandleMessage(message);
        }

        private void NotificationReceived(Message message)
        {
            _messageStore.Save(message);
            HandleMessage(message);
            _messagePump.SendAndReceiveMessages();
        }

        private void HandleMessage(Message message)
        {
            IMessageHandler messageHandler;
            if (_messageHandlers.TryGetValue(message.ObjectId, out messageHandler))
            {
                var handlers = _messageHandlers.Values.ToImmutableList();
                messageHandler.HandleMessage(message);
                LoadNewObjects(handlers);
            }
        }

        private async void LoadNewObjects(ImmutableList<IMessageHandler> loadedHandlers)
        {
            try
            {
                var newHandlers = _messageHandlers.Values.Except(loadedHandlers).ToImmutableList();
                while (newHandlers.Any())
                {
                    foreach (var handler in newHandlers)
                    {
                        var messages = await _messageStore.LoadAsync(handler.GetObjectId());
                        if (messages.Any())
                            handler.HandleAllMessages(messages);
                    }
                    loadedHandlers = loadedHandlers.AddRange(newHandlers);
                    newHandlers = _messageHandlers.Values.Except(loadedHandlers).ToImmutableList();
                }
            }
            catch (Exception ex)
            {
                _exception.Value = ex;
            }
        }

        private static IEnumerable<IMessageHandler> Decendants(IMessageHandler parent)
        {
            if (parent != null)
                return new List<IMessageHandler>() { parent }
                    .Concat(parent.Children.SelectMany(c => Decendants(c)));
            else
                return new List<IMessageHandler>();
        }
    }
}

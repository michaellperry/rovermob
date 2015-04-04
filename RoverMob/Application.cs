using Assisticant.Collections;
using Assisticant.Fields;
using RoverMob.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RoverMob
{
    public class Application<TRoot>
        where TRoot : IMessageHandler
    {
        private readonly IMessageStore _messageStore;
        private readonly IMessageQueue _messageQueue;
        private readonly IMessagePump _messagePump;
        private readonly IPushNotificationSubscription _pushNotificationSubscription;

        private Observable<TRoot> _root = new Observable<TRoot>();
        private ComputedDictionary<Guid, IMessageHandler> _messageHandlers;

        private Observable<Exception> _exception = new Observable<Exception>();

        public Application()
            : this(
                new NoOpMessageStore(),
                new NoOpMessageQueue(),
                new NoOpMessagePump(),
                new NoOpPushNotificationSubscription())
        {
        }

        public Application(
            IMessageStore messageStore,
            IMessageQueue messageQueue,
            IMessagePump messagePump,
            IPushNotificationSubscription pushNotificationSubscription)
        {
            _messageStore = messageStore;
            _messageQueue = messageQueue;
            _messagePump = messagePump;
            _pushNotificationSubscription = pushNotificationSubscription;

            _messagePump.MessageReceived += MessageReceived;
            _pushNotificationSubscription.MessageReceived += NotificationReceived;

            _messageHandlers = new ComputedDictionary<Guid, IMessageHandler>(() =>
                Decendants(_root.Value).ToDictionary(m => m.GetObjectId()));
        }

        public async void Load(TRoot root)
        {
            try
            {
                _root.Value = root;

                _messagePump.Subscribe(root.GetObjectId().ToCanonicalString());
                await _pushNotificationSubscription.Subscribe(
                    root.GetObjectId().ToCanonicalString());

                List<IMessageHandler> loadedHandlers = new List<IMessageHandler>();
                List<IMessageHandler> newHandlers = _messageHandlers.Values.ToList();
                while (newHandlers.Any())
                {
                    foreach (var handler in newHandlers)
                    {
                        var messages = await _messageStore.LoadAsync(handler.GetObjectId());
                        handler.HandleAllMessages(messages);
                    }
                    loadedHandlers.AddRange(newHandlers);
                    newHandlers = _messageHandlers.Values.Except(loadedHandlers).ToList();
                }

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
                messageHandler.HandleMessage(message);
        }

        private static IEnumerable<IMessageHandler> Decendants(IMessageHandler parent)
        {
            return new List<IMessageHandler>() { parent }
                .Concat(parent.Children.SelectMany(c => Decendants(c)));
        }
    }
}

using Assisticant.Collections;
using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoverMob.Messaging
{
    /// <summary>
    /// A collection of items created by successor messages.
    /// </summary>
    /// <typeparam name="T">The type of item in the collection</typeparam>
    public class SuccessorCollection<T>
    {
        private readonly string _createdByMessageType;
        private readonly Func<Message, T> _createItem;
        private readonly string _removedByMessageType;
        private readonly string _removedByRole;
        
        private HashSet<MessageHash> _removed = new HashSet<MessageHash>();
        private Observable<ImmutableList<Candidate<T>>> _items =
            new Observable<ImmutableList<Candidate<T>>>(
                ImmutableList<Candidate<T>>.Empty);

        /// <summary>
        /// Create a collection and provide the rules for adding and removing items.
        /// </summary>
        /// <param name="createdByMessageType">The type of message that creates an item</param>
        /// <param name="createItem">A function that creates an item from a message</param>
        /// <param name="removedByMessageType">The type of message that removes an item</param>
        /// <param name="removedByRole">The role of the removing message that contains the hash of the creating message</param>
        public SuccessorCollection(
            string createdByMessageType,
            Func<Message, T> createItem,
            string removedByMessageType,
            string removedByRole)
        {
            _createdByMessageType = createdByMessageType;
            _createItem = createItem;
            _removedByMessageType = removedByMessageType;
            _removedByRole = removedByRole;
        }
        
        /// <summary>
        /// Read the items in the collection.
        /// </summary>
        public IEnumerable<T> Items
        {
            get { return _items.Value.Select(c => c.Value); }
        }

        /// <summary>
        /// Handle a single message that might insert or remove an item.
        /// </summary>
        /// <param name="message">The message to handle</param>
        public void HandleMessage(Message message)
        {
            if (message.Type == _createdByMessageType)
            {
                var messageHash = message.Hash;

                lock (this)
                {
                    var items = _items.Value;
                    if (!_removed.Contains(messageHash))
                    {
                        T item = _createItem(message);
                        items = items.Add(new Candidate<T>(
                            messageHash, item));
                    }

                    _items.Value = items;
                }
            }
            else if (message.Type == _removedByMessageType)
            {
                var predecessors = message.GetPredecessors(_removedByRole);

                lock (this)
                {
                    var items = _items.Value;

                    var newRemoved = predecessors.Except(_removed);
                    items = items.RemoveAll(c =>
                        newRemoved.Contains(c.MessageHash));

                    foreach (var predecessor in newRemoved)
                        _removed.Add(predecessor);

                    _items.Value = items;
                }
            }
        }

        /// <summary>
        /// Handle a set of messages thay may add or remove items in the collection.
        /// </summary>
        /// <param name="messages">The messages to handle</param>
        public void HandleAllMessages(IEnumerable<Message> messages)
        {
            var removed = messages
                .Where(m => m.Type == _removedByMessageType)
                .SelectMany(m => m.GetPredecessors(_removedByRole))
                .Distinct()
                .ToLookup(h => h);
            var newItems = messages
                .Where(m =>
                    m.Type == _createdByMessageType &&
                    !removed.Contains(m.Hash))
                .Distinct()
                .Select(m => new Candidate<T>(m.Hash, _createItem(m)));

            lock (this)
            {
                var candidates = _items.Value;
                candidates = candidates.RemoveAll(c =>
                    removed.Contains(c.MessageHash));
                foreach (var pair in removed)
                    _removed.Add(pair.Key);
                candidates = candidates.AddRange(newItems);
                _items.Value = candidates;
            }
        }
    }
}

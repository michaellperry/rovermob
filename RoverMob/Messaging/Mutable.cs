using Assisticant.Collections;
using Assisticant.Fields;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoverMob.Messaging
{
    public class Mutable<T>
    {
        private readonly string _topic;

        private HashSet<MessageHash> _predecessors = new HashSet<MessageHash>();
        private Observable<ImmutableList<Candidate<T>>> _candidates =
            new Observable<ImmutableList<Candidate<T>>>(ImmutableList<Candidate<T>>.Empty);
        
        public Mutable(string topic)
        {
            _topic = topic;
        }
        
        public IEnumerable<Candidate<T>> Candidates
        {
            get { return _candidates.Value; }
        }

        public Message CreateMessage(
            string messageType,
            Guid objectId,
            T value)
        {
            return Message.CreateMessage(
                _topic,
                messageType,
                Predecessors.Set
                    .In("prior", Candidates.Select(t => t.MessageHash)),
                objectId,
                new
                {
                    Value = value
                });
        }

        public void HandleMessage(Message message)
        {
            var messageHash = message.Hash;
            T value = (T)message.Body.Value;
            var predecessors = message.GetPredecessors("prior");

            lock (this)
            {
                var candidates = _candidates.Value;
                if (!_predecessors.Contains(messageHash))
                {
                    candidates = candidates.Add(new Candidate<T>(messageHash, value));
                }

                var newPredecessors = predecessors.Except(_predecessors);
                candidates = candidates.RemoveAll(c => newPredecessors.Contains(c.MessageHash));

                foreach (var predecessor in newPredecessors)
                    _predecessors.Add(predecessor);
                _candidates.Value = candidates;
            }
        }

        public void HandleAllMessages(IEnumerable<Message> messages)
        {
            var predecessors = messages
                .SelectMany(m => m.GetPredecessors("prior"))
                .Distinct()
                .ToLookup(h => h);
            var newCandidates = messages
                .Where(m => !predecessors.Contains(m.Hash))
                .Distinct()
                .Select(m => new Candidate<T>(m.Hash, (T)m.Body.Value));

            lock (this)
            {
                var candidates = _candidates.Value;
                candidates = candidates.RemoveAll(c => predecessors.Contains(c.MessageHash));
                foreach (var pair in predecessors)
                    _predecessors.Add(pair.Key);
                candidates = candidates.AddRange(newCandidates);
                _candidates.Value = candidates;
            }
        }
    }
}

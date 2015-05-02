using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace RoverMob.Messaging
{
    public class Predecessors
    {
        private List<Predecessor> _predecessors = new List<Predecessor>();

        public static Predecessors Set
        {
            get { return new Predecessors(); }
        }

        private Predecessors()
        {
        }

        public Predecessors In(string role, IEnumerable<MessageHash> hashes)
        {
            _predecessors.AddRange(
                hashes.Select(h => new Predecessor(role, h)));
            return this;
        }

        public Predecessors In(string role, MessageHash hash)
        {
            _predecessors.Add(new Predecessor(role, hash));
            return this;
        }

        internal ImmutableList<Predecessor> ToImmutableList()
        {
            return _predecessors.ToImmutableList();
        }
    }
}

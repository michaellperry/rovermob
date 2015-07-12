using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpBookmarkStore : IBookmarkStore
    {
        private Dictionary<string, string> _bookmarks = new Dictionary<string, string>();

        public Task<string> LoadBookmarkAsync(string topic)
        {
            string bookmark;
            if (_bookmarks.TryGetValue(topic, out bookmark))
                return Task.FromResult(bookmark);
            else
                return Task.FromResult(string.Empty);
        }

        public Task SaveBookmarkAsync(string topic, string bookmark)
        {
            _bookmarks[topic] = bookmark;
            return Task.FromResult(0);
        }
    }
}

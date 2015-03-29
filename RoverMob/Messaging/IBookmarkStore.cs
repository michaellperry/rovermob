using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public interface IBookmarkStore
    {
        Task<string> LoadBookmarkAsync(string topic);
        Task SaveBookmarkAsync(string topic, string bookmark);
    }
}

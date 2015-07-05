using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpUserProxy : IUserProxy
    {
        public Task<Guid> GetUserIdentifier(string role)
        {
            return Task.FromResult(Guid.Empty);
        }
    }
}

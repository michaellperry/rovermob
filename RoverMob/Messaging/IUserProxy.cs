using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public interface IUserProxy
    {
        Task<Guid> GetUserIdentifier(string role);
    }
}

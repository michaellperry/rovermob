using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RoverMob.Messaging
{
    public class CommunicationException : Exception
    {
        public CommunicationException(string message)
            : base(message)
        {
        }
    }
}

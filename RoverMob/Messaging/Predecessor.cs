using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    class Predecessor
    {
        private readonly string _role;
        private readonly MessageHash _hash;

        public Predecessor(string role, MessageHash hash)
        {
            _role = role;
            _hash = hash;
        }

        public string Role
        {
            get { return _role; }
        }

        public MessageHash Hash
        {
            get { return _hash; }
        }
    }
}

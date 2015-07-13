using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoverMob.Messaging
{
    public class NoOpPushNotificationSubscription : IPushNotificationSubscription
    {
        public event MessageReceivedHandler MessageReceived;

        public void Subscribe(List<string> topics)
        {
        }
    }
}

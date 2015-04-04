#if WINDOWS_PHONE_APP || WINDOWS_APP
using Microsoft.WindowsAzure.Messaging;
using Newtonsoft.Json;
using RoverMob.Protocol;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

namespace RoverMob.Messaging
{
    public class PushNotificationSubscription : IPushNotificationSubscription
    {
        public event MessageReceivedHandler MessageReceived;

        public async Task Subscribe(string topic)
        {
#if WINDOWS_PHONE_APP || WINDOWS_APP
            try
            {
                var channel = await PushNotificationChannelManager
                    .CreatePushNotificationChannelForApplicationAsync();
                channel.PushNotificationReceived += channel_PushNotificationReceived;

                var hub = new NotificationHub("occdist", "Endpoint=sb://occdist-ns.servicebus.windows.net/;SharedAccessKeyName=DefaultListenSharedAccessSignature;SharedAccessKey=+unDBkvenHMZKDwK8ZFZywiemEbFTC5Q64Op1J0TqZw=");
                await hub.RegisterNativeAsync(channel.Uri,
                    new string[] { topic });
            }
            catch (Exception ex)
            {
                // Ignore. Continue without push notifications.
            }
#else
            throw new NotImplementedException();
#endif
        }

#if WINDOWS_PHONE_APP || WINDOWS_APP
        void channel_PushNotificationReceived(PushNotificationChannel sender, PushNotificationReceivedEventArgs args)
        {
            if (args.NotificationType == PushNotificationType.Raw)
            {
                var message = JsonConvert.DeserializeObject<MessageMemento>(
                    args.RawNotification.Content);

                if (MessageReceived != null)
                    MessageReceived(Message.FromMemento(message));
            }
        }
#endif
    }
}

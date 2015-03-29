using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RoverMob.Protocol;
using Microsoft.ServiceBus.Notifications;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Notification
{
    public class AzurePushNotificationProvider
    {
        private readonly string _connectionString;

        public AzurePushNotificationProvider(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task SendNotificationAsync(string topic, MessageMemento message)
        {
            NotificationHubClient hub = NotificationHubClient
                .CreateClientFromConnectionString(
                    _connectionString, "occdist");

            var json = JsonConvert.SerializeObject(message);
            var notification = new WindowsNotification(json,
                new Dictionary<string, string> {
				{"X-WNS-Type", "wns/raw"}
			});
            await hub.SendNotificationAsync(notification, topic);
        }
    }
}

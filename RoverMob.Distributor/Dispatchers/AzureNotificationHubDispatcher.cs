using RoverMob.Distributor.Notification;
using RoverMob.Protocol;
using System;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Dispatchers
{
    public class AzureNotificationHubDispatcher : IDispatcher
    {
        private readonly AzurePushNotificationProvider _pushNotification;

        public AzureNotificationHubDispatcher(string notificationConnectionString)
        {
            _pushNotification = new AzurePushNotificationProvider(notificationConnectionString);
        }

        public async Task DispatchAsync(string topic, MessageMemento message)
        {
            try
            {
                await _pushNotification.SendNotificationAsync(topic, message);
            }
            catch (Exception)
            {
                // If push notifications fail, don't worry about it.
            }
        }
    }
}

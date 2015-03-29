using RoverMob.Distributor.Storage;
using RoverMob.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using RoverMob.Distributor.Notification;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Controllers
{
    public abstract class DistributorController : ApiController
    {
        private AzureStorageProvider _storage;
        private AzurePushNotificationProvider _pushNotification;

        protected DistributorController(
            string storageConnectionString,
            string notificationConnectionString)
        {
            _storage = new AzureStorageProvider(storageConnectionString);
            _pushNotification = new AzurePushNotificationProvider(notificationConnectionString);
        }

        public async Task Post(string topic, [FromBody]MessageMemento message)
        {
            _storage.WriteMessage(topic, message);
            await _pushNotification.SendNotificationAsync(topic, message);
        }

        public PageMemento Get(string topic, string bookmark)
        {
            return _storage.ReadMessages(topic, bookmark);
        }
    }
}
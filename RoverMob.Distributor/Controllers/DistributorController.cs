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

        [Authorize]
        public async Task Post(string topic, [FromBody]MessageMemento message)
        {
            string userId = this.User.Identity.Name;
            bool authorized = await AuthorizeUserForPost(topic, userId);
            if (!authorized)
                throw new UnauthorizedAccessException();

            _storage.WriteMessage(topic, message);
            await _pushNotification.SendNotificationAsync(topic, message);
        }

        [Authorize]
        public async Task<PageMemento> Get(string topic, string bookmark)
        {
            string userId = this.User.Identity.Name;
            bool authorized = await AuthorizeUserForGet(topic, userId);
            if (!authorized)
                throw new UnauthorizedAccessException();

            return _storage.ReadMessages(topic, bookmark);
        }

        protected abstract Task<bool> AuthorizeUserForPost(
            string topic, string userId);
        protected abstract Task<bool> AuthorizeUserForGet(
            string topic, string userId);
    }
}
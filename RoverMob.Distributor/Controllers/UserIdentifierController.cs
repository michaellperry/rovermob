using RoverMob.Distributor.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace RoverMob.Distributor.Controllers
{
    public abstract class UserIdentifierController : ApiController
    {
        private readonly AzureStorageProvider _storage;
        private readonly string _role;
        
        public UserIdentifierController(
            string storageConnectionString,
            string role)
        {
            _storage = new AzureStorageProvider(storageConnectionString);
            _role = role;
        }

        public HttpResponseMessage Get()
        {
            string userId = this.User != null
                ? this.User.Identity != null
                    ? this.User.Identity.Name
                    : null
                : null;

            if (userId == null)
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            return GetUserIdentifierResponse(userId);
        }

        public HttpResponseMessage Get(string userId)
        {
            if (string.IsNullOrEmpty(userId) ||
                User.Identity == null ||
                !AuthorizeUserForGet(userId, User.Identity.Name))
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            }

            return GetUserIdentifierResponse(userId);
        }

        private HttpResponseMessage GetUserIdentifierResponse(string userId)
        {
            Guid identifier = _storage.GetUserIdentifier(_role, userId);
            var resp = new HttpResponseMessage(HttpStatusCode.OK);
            resp.Content = new StringContent(
                identifier.ToString(),
                Encoding.UTF8,
                "text/plain");
            return resp;
        }

        protected abstract bool AuthorizeUserForGet(
            string requestedUserId,
            string userId);
    }
}

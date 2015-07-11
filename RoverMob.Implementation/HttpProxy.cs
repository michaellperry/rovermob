using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace RoverMob.Implementation
{
    public class HttpProxy : IDisposable
    {
        public Task<string> GetJsonAsync(Uri resourceUri)
        {
            throw new NotImplementedException();
        }

        public Task PostJsonAsync(Uri resourceUri, string json)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}

using RoverMob.Protocol;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Dispatchers
{
    public interface IDispatcher
    {
        Task DispatchAsync(string topic, MessageMemento message);
    }
}

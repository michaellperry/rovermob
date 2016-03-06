using Microsoft.ServiceBus.Messaging;
using RoverMob.Protocol;
using System.Threading.Tasks;

namespace RoverMob.Distributor.Dispatchers
{
    public class AzureServiceBusQueueDispatcher : IDispatcher
    {
        private readonly QueueClient _queueClient;

        public AzureServiceBusQueueDispatcher(
            string serviceBusConnectionString,
            string serviceBusPath)
        {
            _queueClient = QueueClient.CreateFromConnectionString(
                serviceBusConnectionString,
                serviceBusPath);
        }

        public async Task DispatchAsync(string topic, MessageMemento message)
        {
            await _queueClient.SendAsync(new BrokeredMessage(message));
        }
    }
}

using RoverMob.Protocol;
using System;

namespace RoverMob.Distributor.Filters
{
    public class AllMessages : IFilter
    {
        public static IFilter Instance { get; } = new AllMessages();

        public bool Accepts(string topic, MessageMemento message)
        {
            return true;
        }
    }
}

using RoverMob.Protocol;
using System;

namespace RoverMob.Distributor.Filters
{
    public interface IFilter
    {
        bool Accepts(string topic, MessageMemento message);
    }
}

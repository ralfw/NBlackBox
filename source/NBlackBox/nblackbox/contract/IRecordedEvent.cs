using System;

namespace nblackbox.contract
{
    public interface IRecordedEvent : IEvent
    {
        DateTime Timestamp { get; }
        string Sequencenumber { get; }
    }
}
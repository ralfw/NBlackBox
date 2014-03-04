using System;

namespace nblackbox.contract
{
    public interface IRecordedEvent : IEvent
    {
        DateTime Timestamp { get; }
        long Index { get; }
    }
}
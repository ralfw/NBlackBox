using System;

namespace nblackbox.contract
{
    public interface IRecordedEvent : IEvent
    {
        Guid Id { get; }
        DateTime Timestamp { get; }
        string Sequencenumber { get; }
    }
}
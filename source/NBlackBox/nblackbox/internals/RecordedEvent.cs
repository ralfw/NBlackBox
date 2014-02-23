using System;
using nblackbox.contract;

namespace nblackbox.internals
{
    internal class RecordedEvent : IRecordedEvent
    {
        public RecordedEvent(DateTime timestamp, long index, string name, string context, string data)
        {
            Timestamp = timestamp;
            Index = index;
            Name = name;
            Context = context;
            Data = data;
        }

        public DateTime Timestamp { get; private set; }
        public long Index { get; private set; }
        public string Name { get; private set; }
        public string Context { get; private set; }
        public string Data { get; private set; }
    }
}
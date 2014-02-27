using System;
using System.Collections.Generic;

namespace nblackbox.contract
{
    public interface IBlackBox : IDisposable
    {
        void Record(string name, string context, string data);
        void Record(IEvent @event);
        void Record(IEnumerable<IEvent> eventBatch);

        IBlackBoxPlayer Player { get; }

        event Action<IRecordedEvent> OnRecorded;
    }
}

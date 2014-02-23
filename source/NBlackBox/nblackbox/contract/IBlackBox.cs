using System;

namespace nblackbox.contract
{
    public interface IBlackBox : IDisposable
    {
        void Record(string name, string context, string data);
        void Record(IEvent @event);

        IBlackBoxPlayer Player { get; }

        event Action<IRecordedEvent> OnRecorded;
    }
}

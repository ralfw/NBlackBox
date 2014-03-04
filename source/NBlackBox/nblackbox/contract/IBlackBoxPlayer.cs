using System.Collections.Generic;

namespace nblackbox.contract
{
    public interface IBlackBoxPlayer
    {
        IBlackBoxPlayer WithContext(params string[] contexts);
        IBlackBoxPlayer ForEvent(params string[] eventnames);
        IBlackBoxPlayer FromIndex(long index);

        IEnumerable<IRecordedEvent> Play();
    }
}
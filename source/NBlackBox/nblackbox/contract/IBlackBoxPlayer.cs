using System;
using System.Collections.Generic;

namespace nblackbox.contract
{
    public interface IBlackBoxPlayer
    {
        IBlackBoxPlayer WithContext(params string[] contexts);
        IBlackBoxPlayer ForEvent(params string[] eventnames);
        IBlackBoxPlayer AfterId(Guid id);

        IEnumerable<IRecordedEvent> Play();
    }
}
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nblackbox.contract
{
    public interface IBlackBox
    {
        void Record(string name, string context, string data);
        void Record(IEvent @event);

        IBlackBoxPlayer Player { get; }

        event Action<IRecordedEvent> OnRecorded;
    }
}

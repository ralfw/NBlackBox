using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using nblackbox.contract;

namespace nblackbox
{
    public class FileBlackBox : IBlackBox
    {
        private readonly string _folderpath;
        private readonly FileStore _filestore;

        public FileBlackBox(string folderpath)
        {
            if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
            _folderpath = folderpath;
            _filestore = new FileStore();
        }


        public void Record(IEvent @event) { Record(@event.Name, @event.Context, @event.Data); }
        public void Record(string name, string context, string data)
        {
            var timestamp = DateTime.Now;
            var index = Directory.GetFiles(_folderpath).Length;

            var e = new RecordedEvent(timestamp, index, name, context, data);
            var filename = timestamp.ToString("s").Replace(":", "-") + "-" + index.ToString("000000000000");
            _filestore.Write(Path.Combine(_folderpath, filename), e);

            OnRecorded(e);
        }


        public IBlackBoxPlayer Player { get { return new BlackBoxPlayer(_folderpath, _filestore); } }


        public event Action<IRecordedEvent> OnRecorded = _ => { };
    }
}

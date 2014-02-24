using System;
using System.IO;
using System.Threading;
using nblackbox.contract;
using nblackbox.internals;

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
            RecordedEvent @event;
            lock (this)
            {
                var timestamp = DateTime.Now;
                var index = Directory.GetFiles(_folderpath).Length;
                @event = new RecordedEvent(timestamp, index, name, context, data);
                var filename = timestamp.ToString("s").Replace(":", "-") + "-" + index.ToString("000000000000");
                _filestore.Write(Path.Combine(_folderpath, filename), @event);
            }
            OnRecorded(@event);
        }


        public IBlackBoxPlayer Player { get { return new BlackBoxPlayer(_folderpath, _filestore); } }


        public event Action<IRecordedEvent> OnRecorded = _ => { };


        public void Dispose() {}
    }
}

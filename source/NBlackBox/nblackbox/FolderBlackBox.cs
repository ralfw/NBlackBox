using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nblackbox.contract;
using nblackbox.internals;
using nblackbox.internals.folder;

namespace nblackbox
{
    public class FolderBlackBox : IBlackBox
    {
        private readonly string _folderpath;
        private readonly FileStore _filestore;

        public FolderBlackBox(string folderpath)
        {
            if (!Directory.Exists(folderpath)) Directory.CreateDirectory(folderpath);
            _folderpath = folderpath;
            _filestore = new FileStore();
        }


        public void Record(IEvent @event) { Record(new[]{@event}); }
        public void Record(string name, string context, string data) { Record(new Event(name,context,data));}
        public void Record(IEnumerable<IEvent> eventBatch)
        {
            var events = new List<RecordedEvent>();
            lock (this)
            {
                eventBatch.ToList().ForEach(e => events.Add(Store(e.Name, e.Context, e.Data)));
            }
            events.ForEach(OnRecorded);
        }

        private RecordedEvent Store(string name, string context, string data)
        {
            var id = Guid.NewGuid();
            var timestamp = DateTime.Now.ToUniversalTime();

            var @event = new RecordedEvent(id, timestamp, name, context, data);

            var sequencenumber = Directory.GetFiles(_folderpath).Length;
            var filename = sequencenumber.ToString("000000000000") + ".txt";
            _filestore.Write(Path.Combine(_folderpath, filename), @event);

            return @event;
        }


        public IBlackBoxPlayer Player { get { return new Player(_folderpath, _filestore); } }


        public event Action<IRecordedEvent> OnRecorded = _ => { };


        public void Dispose() {}
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using nblackbox.contract;

namespace nblackbox.internals.folder
{
    internal class Player : IBlackBoxPlayer
    {
        private readonly string _folderpath;
        private readonly FileStore _filestore;
        private readonly List<Func<IRecordedEvent, bool>> _predicates; 


        public Player(string folderpath, FileStore filestore)
        {
            _folderpath = folderpath;
            _filestore = filestore;
            _predicates = new List<Func<IRecordedEvent, bool>>();
        }


        public IBlackBoxPlayer WithContext(params string[] contexts)
        {
            _predicates.Add(r => contexts.Contains(r.Context));
            return this;
        }

        public IBlackBoxPlayer ForEvent(params string[] eventnames)
        {
            _predicates.Add(r => eventnames.Contains(r.Name));
            return this;
        }

        public IBlackBoxPlayer AfterId(Guid id)
        {
            throw new NotImplementedException();
            return this;
        }


        public IEnumerable<IRecordedEvent> Play()
        {
            var filenames = Directory.GetFiles(_folderpath);
            var events = filenames.Select(_filestore.Read);
            return events.Where(e => _predicates.All(p => p(e)));
        }
    }
}
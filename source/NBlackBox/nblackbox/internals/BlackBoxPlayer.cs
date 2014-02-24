using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using nblackbox.contract;

namespace nblackbox.internals
{
    internal class BlackBoxPlayer : IBlackBoxPlayer
    {
        private readonly string _folderpath;
        private readonly FileStore _filestore;
        private readonly List<Func<IRecordedEvent, bool>> _predicates; 


        public BlackBoxPlayer(string folderpath, FileStore filestore)
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

        public IBlackBoxPlayer FromIndex(long index)
        {
            _predicates.Add(r => r.Index >= index);
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
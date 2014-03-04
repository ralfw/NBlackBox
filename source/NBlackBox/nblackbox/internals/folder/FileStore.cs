using System;
using System.IO;

namespace nblackbox.internals.folder
{
    internal class FileStore
    {
        public void Write(string filename, RecordedEvent @event)
        {
            using (var sw = new StreamWriter(filename))
            {
                sw.WriteLine("1.0");
                sw.WriteLine(@event.Id);
                sw.WriteLine(@event.Timestamp.ToString("yyyy-MM-ddThh:mm:ss.fffffff"));
                sw.WriteLine(@event.Sequencenumber);
                sw.WriteLine(@event.Name);
                sw.WriteLine(@event.Context);
                sw.Write(@event.Data);
            }
        }


        public RecordedEvent Read(string filename)
        {
            using (var sr = new StreamReader(filename))
            {
                var ignore_versionnumber_for_now = sr.ReadLine();
                var id = Guid.Parse("" + sr.ReadLine());
                var timestamp = DateTime.Parse(sr.ReadLine());
                var sequencenumber = sr.ReadLine();
                var name = sr.ReadLine();
                var context = sr.ReadLine();
                var data = sr.ReadToEnd();
                return new RecordedEvent(id, timestamp, sequencenumber, name, context, data);
            }
        }
    }
}
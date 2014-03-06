using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using nblackbox.contract;
using nblackbox.internals;
using nblackbox.internals.mongodb;

namespace nblackbox
{
    public class MongoDbBlackBox : IBlackBox
    {
        private readonly MongoCollection<BsonDocument> _eventCol;
        private long _eventCounter;


        public MongoDbBlackBox(string connectionstring) : this(connectionstring, "NBlackBox"){}
        public MongoDbBlackBox(string connectionstring, string eventstorename)
        {
            var mdbClient = new MongoClient(connectionstring);
            var mdbServer = mdbClient.GetServer();
            var mdb = mdbServer.GetDatabase(eventstorename);

            _eventCol = mdb.GetCollection("Events");
            _eventCol.EnsureIndex(new IndexKeysBuilder().Ascending("sequencenumber"));
            _eventCol.EnsureIndex(new IndexKeysBuilder().Ascending("name"));
            _eventCol.EnsureIndex(new IndexKeysBuilder().Ascending("context"));
        }


        public void Record(string name, string context, string data) { Record(new Event(name, context, data)); }
        public void Record(IEvent @event) { Record(new[]{@event}); }
        public void Record(IEnumerable<IEvent> eventBatch)
        {
            var recordedEvents = new List<RecordedEvent>();
            foreach (var e in eventBatch)
            {
                var timestamp = DateTime.Now.ToUniversalTime();
                var sequencenumber = timestamp.Ticks.ToString("00000000000000000000") + _eventCounter++.ToSequenceNumber();
                var re = new RecordedEvent(Guid.NewGuid(),
                                           timestamp,
                                           sequencenumber,
                                           e.Name,
                                           e.Context,
                                           e.Data);

                var edoc = new BsonDocument {
                    {"_id", re.Id.ToString()},
                    {"timestamp", re.Timestamp},
                    {"sequencenumber", re.Sequencenumber},
                    {"name", re.Name},
                    {"context", re.Context},
                    {"data", re.Data}
                };
                _eventCol.Insert(edoc);

                recordedEvents.Add(re);
            }
            recordedEvents.ForEach(OnRecorded);
        }


        public IBlackBoxPlayer Player { get { return new Player(_eventCol);} }

        public event Action<IRecordedEvent> OnRecorded;

        public void Dispose() {}
    }
}

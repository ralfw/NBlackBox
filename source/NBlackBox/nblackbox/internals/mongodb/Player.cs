using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using nblackbox.contract;

namespace nblackbox.internals.mongodb
{
    class Player : IBlackBoxPlayer
    {
        private readonly MongoCollection<BsonDocument> _eventCol;

        private readonly List<string> _eventnames = new List<string>();
        private readonly List<string> _contexts = new List<string>();
        private string _fromSequenceNumber = "";

        public Player(MongoCollection<BsonDocument> eventCol)
        {
            _eventCol = eventCol;
        }


        public IBlackBoxPlayer WithContext(params string[] contexts)
        {
            _contexts.AddRange(contexts);
            return this;
        }

        public IBlackBoxPlayer ForEvent(params string[] eventnames)
        {
            _eventnames.AddRange(eventnames);
            return this;
        }

        public IBlackBoxPlayer FromSequenceNumber(string sequencenumber)
        {
            _fromSequenceNumber = sequencenumber;
            return this;
        }


        public IEnumerable<IRecordedEvent> Play()
        {
            var queryparts = new List<IMongoQuery> {Query.GTE("sequencenumber", _fromSequenceNumber)};
            if (_eventnames.Count > 0) queryparts.Add(Query.In("name", _eventnames.Select(n => new BsonString(n))));
            if (_contexts.Count > 0) queryparts.Add(Query.In("context", _contexts.Select(c => new BsonString(c))));

            var q = Query.And(queryparts);
            var events = _eventCol.Find(q)
                                  .SetSortOrder("sequencenumber");

            return events.Select(e => new RecordedEvent(Guid.Parse(e["_id"].AsString),
                                                        e["timestamp"].ToUniversalTime(),
                                                        e["sequencenumber"].AsString,
                                                        e["name"].AsString,
                                                        e["context"].AsString,
                                                        e["data"].AsString))
                         .ToArray();
        }
    }
}

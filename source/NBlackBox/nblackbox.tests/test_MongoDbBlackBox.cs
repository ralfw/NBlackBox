using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Driver;
using NUnit.Framework;
using nblackbox.contract;

namespace nblackbox.tests
{
    [TestFixture]
    public class test_MongoDbBlackBox
    {
        [Test, Explicit]
        public void Acceptance_for_singles_against_local_mongodb_instance()
        {
            const string MDBCONNECTION = "mongodb://localhost:27017";
            const string BBNAME = "testbb";

            // mongodb://<user>:<password>@troup.mongohq.com:10091/NBlackBoxTest

            var mdbClient = new MongoClient(MDBCONNECTION);
            var mdbServer = mdbClient.GetServer();
            mdbServer.DropDatabase(BBNAME);

            using (var sut = new MongoDbBlackBox(MDBCONNECTION, BBNAME))
            {
                var recorded = new List<IRecordedEvent>();
                sut.OnRecorded += recorded.Add;

                sut.Record("a", "1", "d1");
                sut.Record("a", "2", "d2");
                sut.Record("b", "2", "d3");
                sut.Record("a", "3", "d4");

                Assert.AreEqual(4, recorded.Count);

                recorded = sut.Player.Play().ToList();
                Assert.AreEqual(4, recorded.Count);
                Assert.AreEqual(new[] { "00", "01", "02", "03" },
                                recorded.Select(r => r.Sequencenumber.Substring(r.Sequencenumber.Length-2, 2)).ToArray());
                Assert.AreEqual(new[] { "d1", "d2", "d3", "d4" }, recorded.Select(r => r.Data).ToArray());

                var fromSequenceNumber = recorded[2].Sequencenumber;

                recorded = sut.Player.ForEvent("a").Play().ToList();
                Assert.AreEqual(3, recorded.Count);

                recorded = sut.Player.WithContext("2").Play().ToList();
                Assert.AreEqual(2, recorded.Count);

                recorded = sut.Player.WithContext("1", "3").Play().ToList();
                Assert.AreEqual(2, recorded.Count);

                recorded = sut.Player.ForEvent("a").WithContext("2").Play().ToList();
                Assert.AreEqual(1, recorded.Count);

                recorded = sut.Player.FromSequenceNumber(fromSequenceNumber).Play().ToList();
                Assert.AreEqual(2, recorded.Count);
            }
        }


        [Test, Explicit]
        public void Use_remote_database()
        {
            const string MDBCONNECTION = "...";
            const string BBNAME = "...";

            var mdbClient = new MongoClient(MDBCONNECTION);
            var mdbServer = mdbClient.GetServer();
            var mdb = mdbServer.GetDatabase(BBNAME);
            mdb.DropCollection("Events");

            using (var sut = new MongoDbBlackBox(MDBCONNECTION, BBNAME))
            {
                sut.Record("a", "1", "d1");
                sut.Record("a", "2", "d2");
                sut.Record("b", "2", "d3");
                sut.Record("a", "3", "d4");

                var events = sut.Player.Play().ToArray();

                Assert.AreEqual(4, events.Length);
                Assert.AreEqual("d1", events[0].Data);
                Assert.AreEqual("d4", events[3].Data);
            }
        }
    }
}
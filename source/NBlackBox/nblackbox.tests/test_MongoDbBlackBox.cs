using System.Collections.Generic;
using System.IO;
using MongoDB.Driver;
using NUnit.Framework;
using nblackbox.contract;

namespace nblackbox.tests
{
    [TestFixture]
    public class test_MongoDbBlackBox
    {
        [Test, Explicit]
        public void Acceptance_for_singles()
        {
            const string MDBCONNECTION = "mongodb://localhost:27017";
            const string BBNAME = "testbb";

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
            }
        } 
    }
}
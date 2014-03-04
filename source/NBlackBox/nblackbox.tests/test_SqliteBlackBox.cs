using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using nblackbox.contract;

namespace nblackbox.tests
{
    using System.Diagnostics;

    using nblackbox.internals;

    [TestFixture]
    public class test_SqliteBlackBox
    {
        [Test]
        public void Acceptance()
        {
            const string BBFILENAME = "testbb.db3";

            if (File.Exists(BBFILENAME)) File.Delete(BBFILENAME);
            using (var sut = new SQliteBlackBox(BBFILENAME))
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
                Assert.AreEqual(new Int64[] {0, 1, 2, 3}, recorded.Select(r => r.Index).ToArray());
                Assert.AreEqual(new[] {"d1", "d2", "d3", "d4"}, recorded.Select(r => r.Data).ToArray());

                recorded = sut.Player.ForEvent("a").Play().ToList();
                Assert.AreEqual(3, recorded.Count);

                recorded = sut.Player.WithContext("2").Play().ToList();
                Assert.AreEqual(2, recorded.Count);

                recorded = sut.Player.WithContext("1", "3").Play().ToList();
                Assert.AreEqual(2, recorded.Count);

                recorded = sut.Player.ForEvent("a").WithContext("2").Play().ToList();
                Assert.AreEqual(1, recorded.Count);

                recorded = sut.Player.FromIndex(2).Play().ToList();
                Assert.AreEqual(2, recorded.Count);
            }
        }


        [Test]
        public void Simple_performance_check()
        {
            const string BBFILENAME = "perftest.db3";
            if (File.Exists(BBFILENAME)) File.Delete(BBFILENAME);
            using (var bb = new SQliteBlackBox(BBFILENAME))
            {
                var sw = Stopwatch.StartNew();
                bb.Record(Create_many_events());
                sw.Stop();
                Console.WriteLine("Created in {0}", sw.Elapsed);

                sw.Restart();
                var cards = bb.Player.Play().ToList();
                sw.Stop();
                Console.WriteLine("Loaded {0} events in {1}", cards.Count, sw.Elapsed);
                Assert.IsTrue(true);
            }
        }

        private static IEnumerable<IEvent> Create_many_events()
        {
            for (var card = 0; card < 1000; card++)
            {
                var newGuid = Guid.NewGuid().ToString();
                var randomText = newGuid;

                yield return new Event("EntityAdded", newGuid, randomText);

                for (var i = 0; i < 10; i++)
                    yield return new Event("EntityChanged", newGuid, i.ToString());
            }
        }
    }
}

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
        public void StoreTest()
        {
            var bb = new SQliteBlackBox("TestStore.db3");

            var sw = Stopwatch.StartNew();
            bb.Record(CreateRecords());
            sw.Stop();
            Console.WriteLine("Created in {0}", sw.Elapsed);

            sw.Restart();
            var cards = bb.Player.Play().ToList();
            sw.Stop();
            Console.WriteLine("Loaded in {0}", sw.Elapsed);
            Assert.IsTrue(true);
        }

        private static IEnumerable<IEvent> CreateRecords()
        {
            for (int card = 0; card < 1000; card++)
            {
                var newGuid = Guid.NewGuid().ToString();
                var randomText = newGuid;
                yield return new BareEvent() { Name = "CardAdded", Context = newGuid, Data = randomText };
                for (int i = 0; i < 10; i++)
                {
                    yield return new BareEvent() { Name = "CardMoved", Context = newGuid, Data = i.ToString() };
                }
            }
        }

        private class BareEvent : IEvent
        {
            public string Name { get; set; }
            public string Context { get; set; }
            public string Data { get; set; }
        }
    }
}

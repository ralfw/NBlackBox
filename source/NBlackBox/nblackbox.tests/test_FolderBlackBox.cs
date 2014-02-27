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
    [TestFixture]
    public class test_FolderBlackBox
    {
        [Test]
        public void Acceptance()
        {
            const string BBFOLDERPATH = "testbb";

            if (Directory.Exists(BBFOLDERPATH)) Directory.Delete(BBFOLDERPATH, true);
            using (var sut = new FolderBlackBox(BBFOLDERPATH))
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
                Assert.AreEqual(new[] {0, 1, 2, 3}, recorded.Select(r => r.Index).ToArray());
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
    }
}

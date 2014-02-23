using System;
using System.IO;
using NUnit.Framework;
using equalidator;

namespace nblackbox.tests
{
    [TestFixture]
    public class test_FileStore
    {
        [Test]
        public void Write_read()
        {
            const string EVENTFILENAME = "event.txt";
            File.Delete(EVENTFILENAME);
            var sut = new FileStore();

            var e = new RecordedEvent(new DateTime(2000, 5, 12, 10, 11, 12), 42, "e", "c", "d1\nd2");
            sut.Write(EVENTFILENAME, e);
            var r = sut.Read(EVENTFILENAME);

            Equalidator.AreEqual(r, e);
        } 
    }
}
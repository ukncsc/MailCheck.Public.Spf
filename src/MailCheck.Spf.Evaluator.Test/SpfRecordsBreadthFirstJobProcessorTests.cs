using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;
using NUnit.Framework;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Evaluator.Test
{
    [TestFixture]
    public class SpfRecordsDepthFirstJobProcessorTests
    {
        [Test]
        public async Task VisitsAllNodes()
        {
            SpfRecordsDepthFirstJobProcessor processor = new SpfRecordsDepthFirstJobProcessor();

            SpfRecords spfRecords = CreateSpfRecords();

            List<string> spfs = new List<string>();

            Task GetSpfRecord(SpfRecords spfRecds)
            {
                foreach (SpfRecord spfRecord in spfRecds.Records)
                {
                    spfs.Add(spfRecord.Record);
                }
                return Task.CompletedTask;
            }

            await processor.Process(spfRecords, GetSpfRecord);

            Assert.That(spfs.Count, Is.EqualTo(3));
            Assert.That(spfs[0], Is.EqualTo("one"));
            Assert.That(spfs[1], Is.EqualTo("two"));
            Assert.That(spfs[2], Is.EqualTo("three"));
        }

        [Test]
        public async Task RunsAllJobs()
        {
            SpfRecordsDepthFirstJobProcessor processor = new SpfRecordsDepthFirstJobProcessor();

            SpfRecords spfRecords = CreateSpfRecords();

            List<string> spfs = new List<string>();

            Task GetSpfRecord(SpfRecords spfRecds)
            {
                foreach (SpfRecord spfRecord in spfRecds.Records)
                {
                    spfs.Add(spfRecord.Record);
                }
                return Task.CompletedTask;
            }

            await processor.Process(spfRecords, GetSpfRecord, GetSpfRecord);

            Assert.That(spfs.Count, Is.EqualTo(6));
            Assert.That(spfs[0], Is.EqualTo("one"));
            Assert.That(spfs[1], Is.EqualTo("one"));
            Assert.That(spfs[2], Is.EqualTo("two"));
            Assert.That(spfs[3], Is.EqualTo("two"));
            Assert.That(spfs[4], Is.EqualTo("three"));
            Assert.That(spfs[5], Is.EqualTo("three"));
        }

        private static SpfRecords CreateSpfRecords()
        {
            SpfRecords spfRecords = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string> {"one"}, new Version(string.Empty, true), new List<Term>
                {
                    new Include(Qualifier.Pass, string.Empty, string.Empty, new SpfRecords(new List<SpfRecord>
                    {
                        new SpfRecord(new List<string> {"two"}, new Version(string.Empty, true), new List<Term>
                        {
                            new Include(Qualifier.Pass, string.Empty, string.Empty, new SpfRecords(new List<SpfRecord>
                            {
                                new SpfRecord(new List<string> {"three"}, new Version(string.Empty, true), new List<Term>(),
                                    new List<Message>(), false)
                            }, 0, new List<Message>()), true)
                        }, new List<Message>(), false)
                    }, 0, new List<Message>()), true)
                }, new List<Message>(), true)
            }, 0, new List<Message>());
            return spfRecords;
        }
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Expansion;
using NUnit.Framework;
using A = FakeItEasy.A;
using Version = MailCheck.Spf.Poller.Domain.Version;

namespace MailCheck.Spf.Poller.Test.Expansion
{
    [TestFixture]
    public class SpfRecordExpanderTests
    {
        private SpfRecordExpander _spfRecordExpander;
        private ISpfPollerConfig _spfPollerConfig;
        private ISpfTermExpanderStrategy _spfTermExpanderStrategy;

        [SetUp]
        public void SetUp()
        {
            _spfTermExpanderStrategy = A.Fake<ISpfTermExpanderStrategy>();

            List<ISpfTermExpanderStrategy> spfTermExpanderStrategies = new List<ISpfTermExpanderStrategy>
            {
                _spfTermExpanderStrategy
            };

            _spfPollerConfig = A.Fake<ISpfPollerConfig>();

            _spfRecordExpander = new SpfRecordExpander(spfTermExpanderStrategies, _spfPollerConfig );
        }

        [Test]
        public async Task ExpandsWhileMoreRecordsFound()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(20);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords spfRecords1 = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>{new Include(string.Empty, Qualifier.Pass, new DomainSpec(string.Empty))})
            }, 200);

            SpfRecords spfRecords2 = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>())
            }, 200);

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, A<Term>._)).ReturnsNextFromSequence(
                spfRecords1,
                spfRecords1,
                spfRecords2
                );

            int count = await _spfRecordExpander.ExpandSpfRecords(string.Empty, spfRecords1);

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task ExpandsUntilLimitMet()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(2);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords spfRecords1 = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>{new Include(string.Empty, Qualifier.Pass, new DomainSpec(string.Empty))})
            }, 200);

            SpfRecords spfRecords2 = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>())
            }, 200);

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, A<Term>._)).ReturnsNextFromSequence(
                spfRecords1,
                spfRecords1,
                spfRecords2
            );

            int count = await _spfRecordExpander.ExpandSpfRecords(string.Empty, spfRecords1);

            Assert.That(count, Is.EqualTo(2));
        }
    }
}

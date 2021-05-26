using System.Collections.Generic;
using System.Linq;
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

        private Include _includeDomain2;
        private Include _includeDomain3;
        private Include _includeDomain4;

        [SetUp]
        public void SetUp()
        {
            _spfTermExpanderStrategy = A.Fake<ISpfTermExpanderStrategy>();

            List<ISpfTermExpanderStrategy> spfTermExpanderStrategies = new List<ISpfTermExpanderStrategy>
            {
                _spfTermExpanderStrategy
            };

            _spfPollerConfig = A.Fake<ISpfPollerConfig>();

            _includeDomain2 = new Include("include:domain2.com", Qualifier.Pass, new DomainSpec("domain2.com"));
            _includeDomain3 = new Include("include:domain3.com", Qualifier.Pass, new DomainSpec("domain3.com"));
            _includeDomain4 = new Include("include:domain4.com", Qualifier.Pass, new DomainSpec("domain4.com"));

            _spfRecordExpander = new SpfRecordExpander(spfTermExpanderStrategies, _spfPollerConfig);
        }

        [Test]
        public async Task ExpandSpfRecordsTraversesTree()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(20);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords domain1SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2));
            SpfRecords domain2SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain3));
            SpfRecords domain3SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain4));
            SpfRecords domain4SpfRecords = CreateSpfRecords(CreateSpfRecord());

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain2)).Returns(domain2SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain3)).Returns(domain3SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain4)).Returns(domain4SpfRecords);

            int count = await _spfRecordExpander.ExpandSpfRecords("domain1.com", domain1SpfRecords);

            Assert.That(count, Is.EqualTo(3));
        }

        [Test]
        public async Task ExpandSpfRecordsTraversesTreeUntilLimitMet()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(2);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords domain1SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2));
            SpfRecords domain2SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain3));
            SpfRecords domain3SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain4));
            SpfRecords domain4SpfRecords = CreateSpfRecords(CreateSpfRecord());

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain2)).Returns(domain2SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain3)).Returns(domain3SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain4)).Returns(domain4SpfRecords);

            int count = await _spfRecordExpander.ExpandSpfRecords("domain1.com", domain1SpfRecords);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task ExpandSpfRecordsTraversesTreeUntilRecursionDetected()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(20);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords domain1SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2));
            SpfRecords domain2SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain3));
            SpfRecords domain3SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2));
            SpfRecords domain4SpfRecords = CreateSpfRecords(CreateSpfRecord());

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain2)).Returns(domain2SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain3)).Returns(domain3SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain4)).Returns(domain4SpfRecords);

            int count = await _spfRecordExpander.ExpandSpfRecords("domain1.com", domain1SpfRecords);

            Assert.AreEqual(1, domain3SpfRecords.AllErrors.Count);

            Assert.AreEqual("35b3a09e-a314-412c-91b8-5c016b7f8d7c", domain3SpfRecords.AllErrors[0].Id.ToString());
            Assert.AreEqual(ErrorType.Error, domain3SpfRecords.AllErrors[0].ErrorType);
            Assert.AreEqual("Your SPF record includes a circular reference to itself, and so Mail Check is unable to expand your SPF record. This could also cause issues for any email service that checks your SPF record", domain3SpfRecords.AllErrors[0].Message);
            Assert.AreEqual("The term `include:domain2.com` is causing a DNS query to a parent SPF record.", domain3SpfRecords.AllErrors[0].Markdown);

            Assert.That(count, Is.EqualTo(2));
        }

        [Test]
        public async Task ExpandSpfRecordsIgnoresRepeatsOnParallelIncludes()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(20);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords domain1SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2, _includeDomain3));
            SpfRecords domain2SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain3));
            SpfRecords domain3SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain4));
            SpfRecords domain4SpfRecords = CreateSpfRecords(CreateSpfRecord());

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain2)).Returns(domain2SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain3)).Returns(domain3SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain4)).Returns(domain4SpfRecords);

            int count = await _spfRecordExpander.ExpandSpfRecords("domain1.com", domain1SpfRecords);

            Assert.AreEqual(0, domain1SpfRecords.AllErrors.Count);
            Assert.That(count, Is.EqualTo(5));
        }

        [Test]
        public async Task ExpandSpfRecordsIgnoresRepeatsOnParallelSpfRecords()
        {
            A.CallTo(() => _spfPollerConfig.MaxDnsQueryCount).Returns(20);
            A.CallTo(() => _spfTermExpanderStrategy.TermType).Returns(typeof(Include));

            SpfRecords domain1SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain2), CreateSpfRecord(_includeDomain3));
            SpfRecords domain2SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain3));
            SpfRecords domain3SpfRecords = CreateSpfRecords(CreateSpfRecord(_includeDomain4));
            SpfRecords domain4SpfRecords = CreateSpfRecords(CreateSpfRecord());

            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain2)).Returns(domain2SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain3)).Returns(domain3SpfRecords);
            A.CallTo(() => _spfTermExpanderStrategy.Process(A<string>._, _includeDomain4)).Returns(domain4SpfRecords);

            int count = await _spfRecordExpander.ExpandSpfRecords("domain1.com", domain1SpfRecords);

            Assert.AreEqual(0, domain1SpfRecords.AllErrors.Count);
            Assert.That(count, Is.EqualTo(5));
        }

        private static SpfRecord CreateSpfRecord(params Include[] includes)
        {
            return new SpfRecord(new List<string>(), new Version(string.Empty), includes.Select(x => (Term)x).ToList());
        }

        private static SpfRecords CreateSpfRecords(params SpfRecord[] spfRecords)
        {
            return new SpfRecords(spfRecords.ToList(), 0);
        }
    }
}

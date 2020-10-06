using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Expansion;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Poller.Test.Expansion
{
    [TestFixture]
    public class SpfIncludeTermExpanderTests
    {
        private SpfIncludeTermExpander _spfIncludeTermExpander;
        private IDnsClient _dnsClient;
        private ISpfRecordsParser _spfRecordsParser;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();

            _spfRecordsParser = A.Fake<ISpfRecordsParser>();

            _spfIncludeTermExpander = new SpfIncludeTermExpander(_dnsClient, _spfRecordsParser);
        }

        [Test]
        public async Task IncludeTermExpanded()
        {
            List<string> spfRecord = new List<string>{"v=spf1"};

            A.CallTo(() => _dnsClient.GetSpfRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<List<string>>>
                    (new List<List<string>>{ spfRecord }, 200)));

            Include include = new Include("", Qualifier.Pass, new DomainSpec(""));

            SpfRecords spfRecords = await _spfIncludeTermExpander.Process("", include);

            Assert.That(spfRecords, Is.Not.Null);
            Assert.That(include.Records, Is.SameAs(spfRecords));

            Assert.That(include.AllErrors, Is.Empty);
        }

        [Test]
        public async Task IncludeTermErroredIfDnsFails()
        {
            A.CallTo(() => _dnsClient.GetSpfRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<List<string>>>
                    ("Error")));

            Include include = new Include("", Qualifier.Pass, new DomainSpec("domain"));

            SpfRecords spfRecords = await _spfIncludeTermExpander.Process("", include);

            Assert.That(spfRecords, Is.Null);

            Assert.That(include.AllErrors.Count, Is.EqualTo(1));
            Assert.AreEqual("Failed SPF record query for domain with error Error", include.AllErrors[0].Message);
        }
    }
}

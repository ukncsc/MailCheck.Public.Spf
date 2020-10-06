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
    public class SpfRedirectTermExpanderTests
    {
        private IDnsClient _dnsClient;
        private ISpfRecordsParser _spfRecordExpander;
        private SpfRedirectTermExpander _spfRedirectTermExpander;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();

            _spfRecordExpander = A.Fake<ISpfRecordsParser>();

            _spfRedirectTermExpander = new SpfRedirectTermExpander(_dnsClient, _spfRecordExpander);
        }

        [Test]
        public async Task RedirectTermExpanded()
        {
            List<string> spfRecord = new List<string> { "v=spf1" };

            A.CallTo(() => _dnsClient.GetSpfRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<List<string>>>
                    (new List<List<string>> { spfRecord }, 200)));

            Redirect redirect = new Redirect("", new DomainSpec(""));

            SpfRecords spfRecords = await _spfRedirectTermExpander.Process("", redirect);

            Assert.That(spfRecords, Is.Not.Null);
            Assert.That(redirect.Records, Is.SameAs(spfRecords));

            Assert.That(redirect.AllErrors, Is.Empty);
        }

        [Test]
        public async Task RedirectTermErroredIfDnsFails()
        {
            A.CallTo(() => _dnsClient.GetSpfRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<List<string>>>
                    ("Error")));

            Redirect redirect = new Redirect("", new DomainSpec("domain"));

            SpfRecords spfRecords = await _spfRedirectTermExpander.Process("", redirect);

            Assert.That(spfRecords, Is.Null);

            Assert.That(redirect.AllErrors.Count, Is.EqualTo(1));
            Assert.AreEqual("Failed SPF record query for domain with error Error", redirect.AllErrors[0].Message);
        }
    }
}

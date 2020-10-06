using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Expansion;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Poller.Test.Expansion
{
    [TestFixture]
    public class SpfATermExpanderTests
    {
        private SpfATermExpander _spfATermExpander;
        private IDnsClient _dnsClient;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _spfATermExpander = new SpfATermExpander(_dnsClient);
        }

        [Test]
        public async Task ARecordExpanded()
        {
            string ip1 = "192.168.1.1";
            string ip2 = "192.168.1.2";

            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>(new List<string> {ip1, ip2}, 200)));

            Domain.A term = new Domain.A("", Qualifier.Pass, new DomainSpec(""),
                new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfATermExpander.Process("", term);

            Assert.That(spfRecords, Is.Null);
            Assert.That(term.Ip4s.Count, Is.EqualTo(2));
            Assert.That(term.Ip4s[0], Is.EqualTo(ip1));
            Assert.That(term.Ip4s[1], Is.EqualTo(ip2));

            Assert.That(term.AllErrors, Is.Empty);
        }

        [Test]
        public async Task ATermWithDomainSpecifiedInExpanderErroredIfDnsFails()
        {
            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>("Error")));

            Domain.A term = new Domain.A("", Qualifier.Pass, new DomainSpec(""),
                new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfATermExpander.Process("domain", term);

            Assert.That(spfRecords, Is.Null);
            Assert.That(term.Ip4s, Is.Null);

            Assert.That(term.AllErrors.Count, Is.EqualTo(1));
            Assert.AreEqual("Failed A record query for domain with error Error", term.AllErrors[0].Message);
        }

        [Test]
        public async Task ATermWithDomainSpecifiedInDomainSpecErroredIfDnsFails()
        {
            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>("Error")));

            Domain.A term = new Domain.A("", Qualifier.Pass, new DomainSpec("domain"),
                new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfATermExpander.Process("", term);

            Assert.That(spfRecords, Is.Null);
            Assert.That(term.Ip4s, Is.Null);

            Assert.That(term.AllErrors.Count, Is.EqualTo(1));
            Assert.AreEqual("Failed A record query for domain with error Error", term.AllErrors[0].Message);
        }
    }
}

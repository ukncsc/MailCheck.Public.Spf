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
    public class SpfMxTermExpanderTests
    {
        private SpfMxTermExpander _spfMxTermExpander;
        private IDnsClient _dnsClient;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _spfMxTermExpander = new SpfMxTermExpander(_dnsClient);
        }

        [Test]
        public async Task MxTermExpanded()
        {
            string host1 = "host1";
            string host2 = "host2";

            string ip1 = "192.168.1.1";
            string ip2 = "192.168.1.2";

            A.CallTo(() => _dnsClient.GetMxRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>
                    (new List<string> { host1, host2 }, 200)));

            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .ReturnsNextFromSequence(
                    Task.FromResult(new DnsResult<List<string>>(new List<string> { ip1, ip2 }, 200)),
                    Task.FromResult(new DnsResult<List<string>>(new List<string> { ip1 }, 200)));

            Mx mx = new Mx("", Qualifier.Pass, new DomainSpec(""), new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfMxTermExpander.Process("", mx);

            Assert.That(spfRecords, Is.Null);

            Assert.That(mx.MxHosts.Count, Is.EqualTo(2));
            Assert.That(mx.MxHosts[0].Host, Is.EqualTo(host1));
            Assert.That(mx.MxHosts[1].Host, Is.EqualTo(host2));

            Assert.That(mx.MxHosts[0].Ip4S.Count, Is.EqualTo(2));
            Assert.That(mx.MxHosts[1].Ip4S.Count, Is.EqualTo(1));

            Assert.That(mx.MxHosts[0].Ip4S[0], Is.EqualTo(ip1));
            Assert.That(mx.MxHosts[0].Ip4S[1], Is.EqualTo(ip2));

            Assert.That(mx.MxHosts[1].Ip4S[0], Is.EqualTo(ip1));

            Assert.That(mx.AllErrors, Is.Empty);
        }

        [Test]
        public async Task MxTermErroredIfMxQueryFails()
        {
            A.CallTo(() => _dnsClient.GetMxRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>("Error")));

            Mx mx = new Mx("", Qualifier.Pass, new DomainSpec("domain"), new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfMxTermExpander.Process("", mx);

            Assert.That(spfRecords, Is.Null);

            Assert.That(mx.MxHosts, Is.Null);

            Assert.That(mx.AllErrors.Count, Is.EqualTo(1));
            Assert.AreEqual("Failed MX record query for domain with error Error", mx.AllErrors[0].Message);
        }

        [Test]
        public async Task MxTermErroredIfARecordLookUpFails()
        {
            string host1 = "host1";

            A.CallTo(() => _dnsClient.GetMxRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>
                    (new List<string> { host1 }, 200)));

            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .ReturnsNextFromSequence(
                    Task.FromResult(new DnsResult<List<string>>("Error")));

            Mx mx = new Mx("", Qualifier.Pass, new DomainSpec(""), new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfMxTermExpander.Process("", mx);

            Assert.That(spfRecords, Is.Null);

            Assert.That(mx.MxHosts.Count, Is.EqualTo(1));

            Assert.That(mx.MxHosts[0].Host, Is.EqualTo(host1));

            Assert.That(mx.MxHosts[0].Ip4S, Is.Empty);

            Assert.That(mx.AllErrors.Count, Is.EqualTo(1));

            Assert.AreEqual("Failed A record query for host1 with error Error", mx.AllErrors[0].Message);
        }

        [Test]
        public async Task MxTermErroredIfARecordHasTooManyResults()
        {
            string host1 = "host1";

            string ip1 = "192.168.1.1";
            string ip2 = "192.168.1.2";
            string ip3 = "192.168.1.3";
            string ip4 = "192.168.1.4";
            string ip5 = "192.168.1.5";
            string ip6 = "192.168.1.6";
            string ip7 = "192.168.1.7";
            string ip8 = "192.168.1.8";
            string ip9 = "192.168.1.9";
            string ip10 = "192.168.1.10";
            string ip11 = "192.168.1.11";

            A.CallTo(() => _dnsClient.GetMxRecords(A<string>._))
                .Returns(Task.FromResult(new DnsResult<List<string>>
                    (new List<string> { host1 }, 200)));

            A.CallTo(() => _dnsClient.GetARecords(A<string>._))
                .ReturnsNextFromSequence(
                    Task.FromResult(new DnsResult<List<string>>(new List<string> { ip1, ip2, ip3, ip4, ip5, ip6, ip7, ip8, ip9, ip10, ip11 }, 200)));

            Mx mx = new Mx("", Qualifier.Pass, new DomainSpec(""), new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfMxTermExpander.Process("", mx);

            Assert.That(spfRecords, Is.Null);

            Assert.That(mx.MxHosts.Count, Is.EqualTo(1));

            Assert.That(mx.MxHosts[0].Host, Is.EqualTo(host1));

            Assert.That(mx.MxHosts[0].Ip4S, Is.Empty);

            Assert.That(mx.AllErrors.Count, Is.EqualTo(1));

            Assert.AreEqual("Too many A records 11 returned for host1. Limit is 10.", mx.AllErrors[0].Message);
        }

        [Test]
        public async Task NoLookupForMacro()
        {
            string macro = "%{o}";

            Mx mx = new Mx("", Qualifier.Pass, new DomainSpec(macro), new DualCidrBlock(new Ip4CidrBlock(32), new Ip6CidrBlock(128)));

            SpfRecords spfRecords = await _spfMxTermExpander.Process("", mx);

            A.CallTo(() => _dnsClient.GetMxRecords(A<string>._)).MustNotHaveHappened();
        }
    }
}

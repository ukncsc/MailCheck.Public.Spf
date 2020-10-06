using System.Linq;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class Ip6AddrParserTests
    {
        private Ip6AddrParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new Ip6AddrParser();
        }

        [TestCase("192.168.1.1", "Invalid ipv6 address value: 192.168.1.1.", TestName = "Valid ip 4 address - invalid")]
        [TestCase("qwerqwer", "Invalid ip address value: qwerqwer.", TestName = "Invalid ip address - invalid")]
        [TestCase("", "Invalid ip address value: .", TestName = "Empty string ip address - invalid")]
        [TestCase(null, "Invalid ip address value: .", TestName = "Null ip address - invalid")]
        public void TestWithErrors(string ipString, string errorStringPattern)
        {
            Ip6Addr ip6Addr = _parser.Parse(ipString);

            Assert.That(ip6Addr.Value, Is.EqualTo(ipString));
            Assert.That(ip6Addr.ErrorCount, Is.EqualTo(1));
            Assert.That(ip6Addr.Errors[0].Message.Contains(errorStringPattern), Is.True);
        }

        [TestCase("fe80::b88a:c43a:2f51:89b6%8", TestName = "Valid ip 6 address - valid")]
        public void TestWithoutErrors(string ipString)
        {
            Ip6Addr ip6Addr = _parser.Parse(ipString);

            Assert.That(ip6Addr.Value, Is.EqualTo(ipString));
            Assert.That(ip6Addr.ErrorCount, Is.EqualTo(0));
        }
    }
}
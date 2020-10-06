using System.Linq;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class Ip4AddrParserTests
    {
        private Ip4AddrParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new Ip4AddrParser();
        }

        [TestCase("192.168.1.1", 0, "", TestName = "Valid ip 4 address - valid")]
        [TestCase("fe80::b88a:c43a:2f51:89b6%8", 1, "Invalid ipv4 address value: fe80::b88a:c43a:2f51:89b6%8.", TestName = "Valid ip 6 address - invalid")]
        [TestCase("qwerqwer", 1, "Invalid ip address value: qwerqwer.", TestName = "Invalid ip address - invalid")]
        [TestCase("", 1, "Invalid ip address value: .", TestName = "Empty string ip address - invalid")]
        [TestCase(null, 1, "Invalid ip address value: .", TestName = "Null ip address - invalid")]
        public void Test(string ipString, int errorCount, string errorStringPattern)
        {
            Ip4Addr ip4Addr = _parser.Parse(ipString);

            Assert.That(ip4Addr.Value, Is.EqualTo(ipString));
            Assert.That(ip4Addr.ErrorCount, Is.EqualTo(errorCount));
            Assert.That((ip4Addr.Errors.FirstOrDefault()?.Message ?? string.Empty).Contains(errorStringPattern), Is.True);
        }
    }
}
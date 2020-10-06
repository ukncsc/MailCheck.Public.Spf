using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class Ip6CidrBlockParserTests
    {
        private Ip6CidrBlockParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new Ip6CidrBlockParser();
        }

        [TestCase("129", 0, "Invalid ipv6 cidr block value: 129. Value must be in the range 0-128.", TestName = "129 invalid ip6 cidr")]
        [TestCase("-1", 0, "Invalid ipv6 cidr block value: -1. Value must be in the range 0-128.", TestName = "-1 invalid ip6 cidr")]
        public void TestWithErrors(string value, int cidr, string errorMessage)
        {
            Ip6CidrBlock ip6CidrBlock = _parser.Parse(value);
            Assert.That(ip6CidrBlock.Value, Is.Null);
            Assert.That(ip6CidrBlock.ErrorCount, Is.EqualTo(1));
            Assert.That(ip6CidrBlock.Errors[0].Message, Is.EqualTo(errorMessage));
        }

        [TestCase("0", 0, TestName = "0 valid ip6 cidr")]
        [TestCase("128", 128, TestName = "128 valid ip6 cidr")]
        [TestCase("", 128, TestName = "empty string valid ip46cidr")]
        [TestCase(null, 128, TestName = "null valid ip6 cidr")]
        public void TestWithoutErrors(string value, int cidr)
        {
            Ip6CidrBlock ip6CidrBlock = _parser.Parse(value);
            Assert.That(ip6CidrBlock.Value, Is.EqualTo(cidr));
            Assert.That(ip6CidrBlock.ErrorCount, Is.EqualTo(0));
        }
    }
}
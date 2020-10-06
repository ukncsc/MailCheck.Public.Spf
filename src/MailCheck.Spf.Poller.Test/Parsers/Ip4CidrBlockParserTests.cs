using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class Ip4CidrBlockParserTests
    {
        private Ip4CidrBlockParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new Ip4CidrBlockParser();
        }

        [TestCase("33", 0, "Invalid ipv4 cidr block value: 33. Value must be in the range 0-32.", TestName = "33 invalid ip4 cidr")]
        [TestCase("-1", 0, "Invalid ipv4 cidr block value: -1. Value must be in the range 0-32.", TestName = "-1 invalid ip4 cidr")]
        public void TestWithErrors(string value, int cidr, string errorMessage)
        {
            Ip4CidrBlock ip4CidrBlock = _parser.Parse(value);
            Assert.That(ip4CidrBlock.Value, Is.EqualTo(null));
            Assert.That(ip4CidrBlock.ErrorCount, Is.EqualTo(1));
            Assert.That(ip4CidrBlock.Errors[0].Message, Is.EqualTo(errorMessage));
        }

        [TestCase("0", 0, TestName = "0 valid ip4 cidr")]
        [TestCase("32", 32, TestName = "32 valid ip4 cidr")]
        [TestCase("", 32, TestName = "empty string valid ip4 cidr")]
        [TestCase(null, 32, TestName = "null valid ip4 cidr")]
        public void TestWithoutErrors(string value, int cidr)
        {
            Ip4CidrBlock ip4CidrBlock = _parser.Parse(value);
            Assert.That(ip4CidrBlock.Value, Is.EqualTo(cidr));
            Assert.That(ip4CidrBlock.ErrorCount, Is.EqualTo(0));
        }
    }
}
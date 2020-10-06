using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class SpfVersionParserTests
    {
        private SpfVersionParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new SpfVersionParser();
        }

        [TestCase("asdfasdf", "Invalid SPF version value: Value: asdfasdf.", TestName = "random string invalid")]
        [TestCase("", "Invalid SPF version value: Value: .", TestName = "empty string invalid")]
        [TestCase(null, "Invalid SPF version value: Value: .", TestName = "null string invalid")]
        public void TestWithErrors(string value, string errorMessage)
        {
            Version version = _parser.Parse(value);
            Assert.That(version.Value, Is.EqualTo(value));
            Assert.That(version.ErrorCount, Is.EqualTo(1));
            Assert.That(version.Errors[0].Message, Is.EqualTo(errorMessage));
        }

        [TestCase("v=spf1", TestName = "valid version")]
        [TestCase("v=SpF1", TestName = "case insensitive")]
        public void TestWithoutErrors(string value)
        {
            Version version = _parser.Parse(value);
            Assert.That(version.Value, Is.EqualTo(value));
            Assert.That(version.ErrorCount, Is.EqualTo(0));
        }
    }
}

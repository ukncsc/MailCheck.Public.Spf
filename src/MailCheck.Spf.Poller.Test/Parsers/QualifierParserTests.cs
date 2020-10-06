using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class QualifierParserTests
    {
        private QualifierParser _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new QualifierParser();
        }

        [TestCase("", Qualifier.Pass)]
        [TestCase("+", Qualifier.Pass)]
        [TestCase("-", Qualifier.Fail)]
        [TestCase("?", Qualifier.Neutral)]
        [TestCase("~", Qualifier.SoftFail)]
        [TestCase("*", Qualifier.Unknown)]
        public void Test(string value, Qualifier expectedQualifier)
        {
            Qualifier qualifier = _parser.Parse(value);
            Assert.That(qualifier, Is.EqualTo(expectedQualifier));
        }
    }
}
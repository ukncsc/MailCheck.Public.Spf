using FakeItEasy;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture]
    public class DomainSpecDualCidrBlockMechanismParserTests
    {
        private DomainSpecDualCidrBlockMechanismParser _parser;
        private IDomainSpecParser _domainSpecParser;
        private IDualCidrBlockParser _dualCidrBlockParser;

        [SetUp]
        public void SetUp()
        {
            _domainSpecParser = A.Fake<IDomainSpecParser>();
            _dualCidrBlockParser = A.Fake<IDualCidrBlockParser>();
            _parser = new DomainSpecDualCidrBlockMechanismParser(_domainSpecParser, _dualCidrBlockParser);
        }

        [TestCase("a.b.com/32//128", "a.b.com", "32", "128", TestName = "Domain spec, ip4 cidr, ip6 cidr.")]
        [TestCase("a.b.com//128", "a.b.com", "", "128", TestName = "Domain spec, ip6 cidr.")]
        [TestCase("a.b.com/32", "a.b.com", "32", "", TestName = "Domain spec, ip4 cidr.")]
        [TestCase("a.b.com", "a.b.com", "", "", TestName = "Domain spec.")]
        [TestCase("32//128", "", "", "", TestName = "ip4 cidr, ip6 cidr.")]
        [TestCase("/32//128", "", "", "", TestName = "ip4 cidr, ip6 cidr 2.")]
        [TestCase("", "", "", "", TestName = "empty string.")]
        [TestCase(null, "", "", "", TestName = "null string.")]
        public void Test(string value, string domainSpec, string ip4Cidr, string ip6Cidr)
        {
            Term term = _parser.Parse(string.Empty, Qualifier.Pass, value, (s, q, ds, dc) => new Mx(s, q, ds, dc));

            A.CallTo(() => _domainSpecParser.Parse(domainSpec, false)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dualCidrBlockParser.Parse(ip4Cidr, ip6Cidr)).MustHaveHappenedOnceExactly();

            Assert.That(term, Is.Not.Null);
        }
    }
}

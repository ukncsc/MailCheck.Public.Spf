using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class RedirectDoesntOccurMoreThanOnceTests
    {
        private RedirectDoesntOccurMoreThanOnce _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RedirectDoesntOccurMoreThanOnce();
        }

        [TestCase(0, TestName = "No redirect term no error")]
        [TestCase(1, TestName = "One redirect term no error")]
        public async Task Test(int occurances)
        {
            List<Term> terms = Enumerable.Range(0, occurances)
                .Select(_ => new Redirect(string.Empty, new DomainSpec(string.Empty)))
                .Cast<Term>()
                .ToList();

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task TwoRedirectTermsError()
        {
            List<Term> terms = Enumerable.Range(0, 2)
                .Select(_ => new Redirect(string.Empty, new DomainSpec(string.Empty)))
                .Cast<Term>()
                .ToList();

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(1));
        }
    }
}

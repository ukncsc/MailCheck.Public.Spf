using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class ExplanationDoesntOccurMoreThanOnceTests
    {
        private ExplanationDoesntOccurMoreThanOnce _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new ExplanationDoesntOccurMoreThanOnce();
        }

        [TestCase(0, TestName = "No exp term no error")]
        [TestCase(1, TestName = "One exp term no error")]
        public async Task Test(int occurances)
        {
            List<Term> terms = Enumerable.Range(0, occurances)
                .Select(_ => new Explanation(string.Empty, new DomainSpec(string.Empty)))
                .Cast<Term>()
                .ToList();

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task MoreThanOneExpGivesError()
        {
            List<Term> terms = Enumerable.Range(0, 2)
                .Select(_ => new Explanation(string.Empty, new DomainSpec(string.Empty)))
                .Cast<Term>()
                .ToList();

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].Message, Is.EqualTo(@"domain1: Record should not contain 'exp' more than once. This record has 2 'exp' terms."));
        }
    }
}

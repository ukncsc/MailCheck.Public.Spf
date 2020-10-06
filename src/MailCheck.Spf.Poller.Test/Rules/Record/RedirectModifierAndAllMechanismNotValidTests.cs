using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class RedirectModifierAndAllMechanismNotValidTests
    {
        private RedirectModifierAndAllMechanismNotValid _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new RedirectModifierAndAllMechanismNotValid();
        }

        [TestCase(false, false, TestName = "No redirect term no error 1")]
        [TestCase(false, true, TestName = "No redirect term no error 2")]
        [TestCase(true, false, TestName = "No redirect term no error 3")]
        [TestCase(true, true, true, TestName = "Redirect term with implicit all, no error.")]
        public async Task Test(bool isAllTerm, bool isRedirectTerm, bool isImplicitAll = false)
        {
            List<Term> terms = new List<Term>
            {
                isAllTerm ? new All(string.Empty, Qualifier.Fail, isImplicitAll) : null,
                isRedirectTerm ? new Redirect(string.Empty, new DomainSpec(string.Empty)) : null
            };

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ErrorIfRedirectAndAll()
        {
            List<Term> terms = new List<Term>
            {
                new All(string.Empty, Qualifier.Fail),
                new Redirect(string.Empty, new DomainSpec(string.Empty))
            };

            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(1));
            Assert.That(errors[0].Message, Is.EqualTo("domain1: Record should not contain both 'redirect' and 'all'."));
        }
    }
}

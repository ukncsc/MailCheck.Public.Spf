using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class DontUsePtrMechanismTest
    {
        private DontUsePtrMechanism _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new DontUsePtrMechanism();
        }

        [Test]
        public async Task NoPtrTermNoError()
        {
            Ptr ptr = null;
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term> { ptr }));

            List<Error> error = await _rule.Evaluate(spfRecord);

            Assert.That(error.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task PtrTermError()
        {
            Ptr ptr = new Ptr(string.Empty, Qualifier.Fail, new DomainSpec(string.Empty));
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term> { ptr }));

            List<Error> error = await _rule.Evaluate(spfRecord);

            Assert.That(error.Count, Is.EqualTo(1));
            Assert.That(error[0].Message, Is.EqualTo("domain1: Record should not contain “ptr”."));
        }
    }
}

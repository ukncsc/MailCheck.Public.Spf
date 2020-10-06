using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class AdviseAgainstMxMechanismTest
    {
        private AdviseAgainstMxMechanism _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new AdviseAgainstMxMechanism();
        }

        [Test]
        public async Task NoInfoMessageWhenMxNotPresent()
        {
            Mx mx = null;
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term> { mx }));

            List<Error> error = await _rule.Evaluate(spfRecord);

            Assert.That(error.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task InfoMessageWhenMxPresent()
        {
            Mx mx = new Mx(string.Empty, (Qualifier)999, null, null);
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term> { mx }));

            List<Error> error = await _rule.Evaluate(spfRecord);

            Assert.That(error.Count, Is.EqualTo(1));
            Assert.AreEqual("The use of the mx mechanism is not recommended", error[0].Message);
            Assert.AreEqual(ErrorType.Info, error[0].ErrorType);
        }
    }
}

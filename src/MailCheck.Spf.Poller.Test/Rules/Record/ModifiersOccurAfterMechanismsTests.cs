using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Record;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Record
{
    [TestFixture]
    public class ModifiersOccurAfterMechanismsTests
    {
        private ModifiersOccurAfterMechanisms _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new ModifiersOccurAfterMechanisms();
        }

        [TestCaseSource(nameof(TestCaseSource))]
        public async Task Test(List<Term> terms, bool isErrorExpected)
        {
            DomainSpfRecord spfRecord = new DomainSpfRecord("domain1", new SpfRecord(new List<string>(), new Version(string.Empty), terms));

            List<Error> errors = await _rule.Evaluate(spfRecord);

            Assert.That(errors.Count, Is.EqualTo(isErrorExpected ? 1 : 0));
        }

        private static IEnumerable<TestCaseData> TestCaseSource()
        {
            yield return new TestCaseData(new List<Term> { new All(string.Empty, Qualifier.Fail), new Redirect(string.Empty, new DomainSpec(string.Empty)) }, false).SetName("Mechanisms before modifiers no errors.");
            yield return new TestCaseData(new List<Term> { new Redirect(string.Empty, new DomainSpec(string.Empty)), new All(string.Empty, Qualifier.Fail) }, true).SetName("Modifiers before mechanisms errors.");
            yield return new TestCaseData(new List<Term> { new All(string.Empty, Qualifier.Fail) }, false).SetName("No modifiers no errors.");
            yield return new TestCaseData(new List<Term> { new Redirect(string.Empty, new DomainSpec(string.Empty)) }, false).SetName("No mechanisms no errors.");
            yield return new TestCaseData(new List<Term> { new Redirect(string.Empty, new DomainSpec(string.Empty)), new All(string.Empty, Qualifier.Fail, true) }, false).SetName("Implicit modifiers before mechanisms, no errors.");
        }
    }
}

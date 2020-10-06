using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Evaluator.Explainers;
using MailCheck.Spf.Evaluator.Rules;
using NUnit.Framework;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Evaluator.Test.Rules
{
    [TestFixture]
    public class ShouldHaveHardFailAllEnabledTests
    {
        [TestCase(Qualifier.Pass, true, @"Only ""-all"" (do not allow other ip addresses) or ""~all"" (allow but mark other ip addresses) protect recipients from spoofed mail. Consider changing from  (allow other ip addresses) to ""-all"" or ""~all"".", TestName = "All Pass causes warning on root")]
        [TestCase(Qualifier.Unknown, true, @"Only ""-all"" (do not allow other ip addresses) or ""~all"" (allow but mark other ip addresses) protect recipients from spoofed mail. Consider changing from  ( other ip addresses) to ""-all"" or ""~all"".", TestName = "All Unknown causes warning on root")]
        [TestCase(Qualifier.Neutral, true, @"Only ""-all"" (do not allow other ip addresses) or ""~all"" (allow but mark other ip addresses) protect recipients from spoofed mail. Consider changing from  (allow without evaluation other ip addresses) to ""-all"" or ""~all"".", TestName = "All Neutral causes warning on root")]
        public async Task TestWithErrors(Qualifier? qualifier, bool root, string expectedErrorMessage)
        {
            List<Term> terms = qualifier.HasValue
                ? new List<Term> { new All(qualifier.Value, string.Empty, true, false)}
                : new List<Term>();

            SpfRecord record = new SpfRecord(new List<string>(), new Version(string.Empty, true), terms, new List<Message>(), root);

            ShouldHaveHardFailAllEnabled rule = new ShouldHaveHardFailAllEnabled(new QualifierExplainer());
            
            List<Message> messages = await rule.Evaluate(record);

            Assert.That(messages.Any(), Is.True);

            Assert.That(messages[0].Text, Is.EqualTo(expectedErrorMessage));
        }
        
        [TestCase(Qualifier.Fail, true, TestName = "All Fail does not cause warning on root")]
        [TestCase(Qualifier.SoftFail, true, TestName = "All SoftFail does not causes warning on root")]
        [TestCase(null, true, TestName = "No all term does not cause warning on root")]
        [TestCase(Qualifier.Pass, false, TestName = "All Pass does not cause warning on non root")]
        [TestCase(Qualifier.Unknown, false, TestName = "All Unknown does not cause warning on non root")]
        [TestCase(Qualifier.Neutral, false, TestName = "All Neutral does not cause warning on non root")]
        [TestCase(Qualifier.Fail, false, TestName = "All Fail does not cause warning on non root")]
        [TestCase(Qualifier.SoftFail, false, TestName = "All SoftFail does not causes warning on non root")]
        [TestCase(null, false, TestName = "No all term does not cause warning on non root")]
        public async Task TestWithoutErrors(Qualifier? qualifier, bool root)
        {
            List<Term> terms = qualifier.HasValue
                ? new List<Term> { new All(qualifier.Value, string.Empty, true, false) }
                : new List<Term>();

            SpfRecord record = new SpfRecord(new List<string>(), new Version(string.Empty, true), terms, new List<Message>(), root);

            ShouldHaveHardFailAllEnabled rule = new ShouldHaveHardFailAllEnabled(new QualifierExplainer());

            List<Message> messages = await rule.Evaluate(record);

            Assert.That(messages.Any(), Is.False);
        }
    }
}

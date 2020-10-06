using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Records;
using NUnit.Framework;
using Version = MailCheck.Spf.Poller.Domain.Version;

namespace MailCheck.Spf.Poller.Test.Rules.Records
{
    [TestFixture]
    public class OnlyOneSpfRecordTests
    {
        private OnlyOneSpfRecord _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new OnlyOneSpfRecord();
        }

        [TestCase(0, true)]
        [TestCase(1, false)]
        [TestCase(2, true)]
        public async Task Test(int count, bool isErrorExpected)
        {
            List<SpfRecord> spfRecords = Enumerable.Range(0, count).Select(_ => new SpfRecord(new List<string>(), new Version(string.Empty), new List<Term>())).ToList();
            DomainSpfRecords domainSpfRecords = new DomainSpfRecords("domain1", new SpfRecords(spfRecords, 10));

            List<Error> errors = await _rule.Evaluate(domainSpfRecords);

            Assert.That(errors.Count, Is.EqualTo( isErrorExpected ? 1 : 0 ));
        }
    }
}

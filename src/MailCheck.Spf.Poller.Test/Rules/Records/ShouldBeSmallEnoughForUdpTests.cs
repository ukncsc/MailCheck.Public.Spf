using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.Records;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Rules.Records
{
    [TestFixture]
    public class ShouldBeSmallEnoughForUdpTests
    {
        private ShouldBeSmallEnoughForUdp _rule;

        [SetUp]
        public void SetUp()
        {
            _rule = new ShouldBeSmallEnoughForUdp();
        }

        [TestCase(0, false)]
        [TestCase(450, false)]
        [TestCase(451, true)]
        public async Task Test(int messageSize, bool isErrorExpected)
        {
            DomainSpfRecords domainSpfRecords = new DomainSpfRecords("domain1", new SpfRecords(new List<SpfRecord>(), messageSize));

            List<Error> errors = await _rule.Evaluate(domainSpfRecords);

            if (isErrorExpected)
            {
                Assert.That(errors.Count, Is.EqualTo(1));
                Assert.That(errors[0].ErrorType, Is.EqualTo(ErrorType.Info));
            }
            else
            {
                Assert.That(errors.Count, Is.EqualTo(0));
            }
        }
    }

}

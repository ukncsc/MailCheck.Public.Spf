using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules.PollResult;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Poller.Test.Rules.PollResult
{
    [TestFixture]
    public class ShouldNotHaveMoreThan10RecordsTests
    {
        private ShouldNotHaveMoreThan10QueryLookups _rule;
        private ISpfPollerConfig _config;

        [SetUp]
        public void SetUp()
        {
            _config = A.Fake<ISpfPollerConfig>();
            A.CallTo(() => _config.MaxDnsQueryCount).Returns(20);
            _rule = new ShouldNotHaveMoreThan10QueryLookups(_config);
        }

        [Test]
        public async Task TenSpfRecordsShouldGiveWarning()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 10, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Warning));
            Assert.That(result[0].Message, Is.EqualTo("The SPF specification limits the amount of DNS lookups for a record to 10. This record currently has 10 which could be taken over the limit by a third party change. You are likely to experience SPF failures if you exceed the limit of 10."));

        }

        [Test]
        public async Task FiveSpfRecordsShouldGiveInfo()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 5, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Info));
            Assert.That(result[0].Message, Is.EqualTo("5/10 DNS lookups used. You are likely to experience SPF failures if you exceed this limit of 10."));
        }

        [Test]
        public async Task MoreThanTenSpfRecordsShouldGiveError()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 11, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Error));
            Assert.That(result[0].Message, Is.EqualTo("The SPF specification limits the amount of DNS lookups for a record to 10. This record currently has 11 which will likely cause SPF failures."));
        }

        [Test]
        public async Task GreaterThanOrEqualToMaxDnsQueryCountShouldGiveErrorWithAtLeastMessage()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 21, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Error));
            Assert.That(result[0].Message, Is.EqualTo("The SPF specification limits the amount of DNS lookups for a record to 10. This record currently has at least 20 which will likely cause SPF failures."));
        }

    }
}

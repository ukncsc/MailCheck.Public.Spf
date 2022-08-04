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

        public Guid InfoId => Guid.Parse("6BA5A47E-D212-4131-AC74-38F298A57894");
        public Guid WarningId => Guid.Parse("8ab5ea1a-659d-4259-ba74-df25a6e7a617");
        public Guid ErrorId => Guid.Parse("f16e0f37-3b6e-46b8-bc55-0edefde112cf");

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
            Assert.That(result[0].Message, Is.EqualTo("DNS lookups at limit of 10. Added lookups (eg caused by 3rd parties) are likely to cause SPF failures. Reduce lookups if possible."));
            Assert.That(result[0].Id, Is.EqualTo(WarningId));
        }

        [Test]
        public async Task SevenSpfRecordsShouldGiveInfo()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 7, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Info));
            Assert.That(result[0].Message, Is.EqualTo("7/10 DNS lookups used. You are likely to experience SPF failures if you exceed this limit of 10."));
            Assert.That(result[0].Id, Is.EqualTo(InfoId));
        }

        [Test]
        public async Task MoreThanTenSpfRecordsShouldGiveError()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 11, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Error));
            Assert.That(result[0].Message, Is.EqualTo("The DNS lookup limit of 10 has been exceeded. This domain has 11 which will likely cause SPF failures."));
            Assert.That(result[0].Id, Is.EqualTo(ErrorId));
        }

        [Test]
        public async Task LessThanSevenSpfRecordsShouldGiveNoAdvisory()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 6, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task GreaterThanOrEqualToMaxDnsQueryCountShouldGiveErrorWithAtLeastMessage()
        {
            SpfPollResult spfPollResult = new SpfPollResult(new SpfRecords(new List<SpfRecord>(), 0), 21, TimeSpan.MaxValue);
            List<Error> result = await _rule.Evaluate(spfPollResult);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].ErrorType, Is.EqualTo(ErrorType.Error));
            Assert.That(result[0].Message, Is.EqualTo("The DNS lookup limit of 10 has been exceeded. This domain has at least 20 which will likely cause SPF failures."));
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MailCheck.Spf.Scheduler.Processor;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Spf.Scheduler.Test.Processor
{
    [TestFixture]
    public class SpfPollSchedulerProcessorTests
    {
        private SpfPollSchedulerProcessor _sut;
        private ISpfPeriodicSchedulerDao _dao;
        private IMessagePublisher _publisher;
        private ISpfPeriodicSchedulerConfig _config;
        private ILogger<SpfPollSchedulerProcessor> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ISpfPeriodicSchedulerDao>();
            _publisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<ISpfPeriodicSchedulerConfig>();
            _log = A.Fake<ILogger<SpfPollSchedulerProcessor>>();

            _sut = new SpfPollSchedulerProcessor(_dao, _publisher, _config, _log);
        }

        [Test]
        public async Task ItShouldPublishAndUpdateThenContinueWhenThereAreExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredSpfRecords())
                .Returns(CreateSchedulerStates("ncsc.gov.uk", "fco.gov.uk"));

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<SpfRecordExpired>._, A<string>._))
                .MustHaveHappenedTwiceExactly();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<SpfSchedulerState>>._))
                .MustHaveHappenedOnceExactly();

            Assert.AreEqual(ProcessResult.Continue, result);
        }

        [Test]
        public async Task ItShouldNotPublishOrUpdateThenStopWhenThereAreNoExpiredRecords()
        {
            A.CallTo(() => _dao.GetExpiredSpfRecords())
                .Returns(CreateSchedulerStates());

            ProcessResult result = await _sut.Process();

            A.CallTo(() => _publisher.Publish(A<SpfRecordExpired>._, A<string>._))
                .MustNotHaveHappened();

            A.CallTo(() => _dao.UpdateLastChecked(A<List<SpfSchedulerState>>._))
                .MustNotHaveHappened();

            Assert.AreEqual(ProcessResult.Stop, result);
        }


        private List<SpfSchedulerState> CreateSchedulerStates(params string[] args) =>
            args.Select(_ => new SpfSchedulerState(_)).ToList();
    }
}

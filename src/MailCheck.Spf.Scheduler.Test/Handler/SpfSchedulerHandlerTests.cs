using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MailCheck.Spf.Scheduler.Handler;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Spf.Scheduler.Test.Handler
{
    [TestFixture]
    public class SpfSchedulerHandlerTests
    {
        private SpfSchedulerHandler _sut;
        private ISpfSchedulerDao _dao;
        private IMessageDispatcher _dispatcher;
        private ISpfSchedulerConfig _config;
        private ILogger<SpfSchedulerHandler> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ISpfSchedulerDao>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _config = A.Fake<ISpfSchedulerConfig>();
            _log = A.Fake<ILogger<SpfSchedulerHandler>>();

            _sut = new SpfSchedulerHandler(_dao, _dispatcher, _config, _log);
        }

        [Test]
        public async Task ItShouldSaveAndDispatchTheSpfStateIfItDoesntExist()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns<SpfSchedulerState>(null);

            await _sut.Handle(new SpfEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<SpfSchedulerState>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordExpired>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task ItShouldNotSaveOrDispatchTheSpfStateIfItExists()
        {
            A.CallTo(() => _dao.Get(A<string>._)).Returns(new SpfSchedulerState("ncsc.gov.uk"));

            await _sut.Handle(new SpfEntityCreated("ncsc.gov.uk", 1));

            A.CallTo(() => _dao.Save(A<SpfSchedulerState>._)).MustNotHaveHappened();

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordExpired>._, A<string>._))
                .MustNotHaveHappened();
        }

        [Test]
        public async Task ItShouldDeleteTheSpfState()
        {

            await _sut.Handle(new DomainDeleted("ncsc.gov.uk"));

            A.CallTo(() => _dao.Delete(A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<Message>._, A<string>._))
                .MustNotHaveHappened();
        }
    }
}

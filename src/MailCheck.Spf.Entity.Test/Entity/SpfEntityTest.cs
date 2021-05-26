using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Dao;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Entity.Entity.DomainStatus;
using MailCheck.Spf.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Entity.Test.Entity
{
    [TestFixture]
    public class SpfEntityTest
    {
        private const string Id = "abc.com";

        private ISpfEntityDao _spfEntityDao;
        private ISpfEntityConfig _spfEntityConfig;
        private ILogger<SpfEntity> _log;
        private IMessageDispatcher _dispatcher;
        private IChangeNotifiersComposite _changeNotifierComposite;
        private IDomainStatusPublisher _domainStatusPublisher;
        private SpfEntity _spfEntity;

        [SetUp]
        public void SetUp()
        {
            _spfEntityDao = A.Fake<ISpfEntityDao>();
            _spfEntityConfig = A.Fake<ISpfEntityConfig>();
            _log = A.Fake<ILogger<SpfEntity>>();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _changeNotifierComposite = A.Fake<IChangeNotifiersComposite>();
            _domainStatusPublisher = A.Fake<IDomainStatusPublisher>();
            _spfEntity = new SpfEntity(_spfEntityDao, _spfEntityConfig, _changeNotifierComposite, _log, _dispatcher, _domainStatusPublisher);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _spfEntityDao.Get(Id)).Returns<SpfEntityState>(null);
            await _spfEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _spfEntityDao.Save(A<SpfEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<SpfEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _spfEntityDao.Get(Id))
                .Returns(new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.UtcNow));
            await _spfEntity.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _spfEntityDao.Save(A<SpfEntityState>._)).MustNotHaveHappened();
            A.CallTo(() => _dispatcher.Dispatch(A<SpfEntityCreated>._, A<string>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleDomainDeletedDeletesDomain()
        {
            await _spfEntity.Handle(new DomainDeleted(Id));

            A.CallTo(() => _spfEntityDao.Delete(A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<Common.Messaging.Abstractions.Message>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleSpfRecordExpiredRaiseSpfPollPending()
        {
            A.CallTo(() => _spfEntityDao.Get(Id)).Returns(new SpfEntityState(Id, 2, SpfState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                SpfRecords = new SpfRecords(new List<SpfRecord>(), 100, new List<Message>()),
                SpfState = SpfState.Created
            });

            await _spfEntity.Handle(new SpfRecordExpired(Id));

            A.CallTo(() => _spfEntityDao.Save(A<SpfEntityState>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<SpfPollPending>._, A<string>._)).MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public async Task HandleSpfRecordsEvaluatedAndNewEvaluationUpdatesStateAndPublishes()
        {
            A.CallTo(() => _spfEntityDao.Get(Id)).Returns(new SpfEntityState(Id, 2, SpfState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                SpfRecords = CreateSpfRecords()
            });

            List<Term> terms =
                new List<Term>
                {
                    new Redirect("spf1", "abc.com",
                        CreateSpfRecords(new List<Message> {new Message(Guid.Empty, "SPF", MessageType.info, "hello", "markdown")}),
                        false),
                };

            SpfRecords spfRecords = CreateSpfRecords(terms: terms,
                messages: new List<Message> {new Message(Guid.Empty, "SPF", MessageType.info, "hello", "markdown")});

            spfRecords.Records[0].Messages
                .Add(new Message(Guid.Empty, MessageSources.SpfEvaluator, MessageType.error, "EvaluationError", "markdown"));
            spfRecords.Records[0].Terms[0].Explanation = "Explanation";

            SpfRecordsEvaluated spfRecordsEvaluated = new SpfRecordsEvaluated(Id, spfRecords, 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            await _spfEntity.Handle(spfRecordsEvaluated);

            A.CallTo(() => _changeNotifierComposite.Handle(A<SpfEntityState>._, spfRecordsEvaluated))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordEvaluationsChanged>.That.Matches(
                        _ => _.Records.Records[0].Messages[0].Text.Equals(spfRecords.Records[0].Messages[0].Text) &&
                             _.Records.Records[0].Terms[0].Explanation
                                 .Equals(spfRecords.Records[0].Terms[0].Explanation)),
                    A<string>._))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _changeNotifierComposite.Handle(A<SpfEntityState>._, spfRecordsEvaluated))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _domainStatusPublisher.Publish(spfRecordsEvaluated))
                .MustHaveHappenedOnceExactly();

            A.CallTo(() => _spfEntityDao.Save(A<SpfEntityState>._)).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleSpfRecordsEvaluatedAndWithExistingEvaluationUpdatesStateTest()
        {
            A.CallTo(() => _spfEntityDao.Get(Id)).Returns(new SpfEntityState(Id, 2, SpfState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                SpfRecords = CreateSpfRecords()
            });

            SpfRecords spfRecords = CreateSpfRecords();

            SpfRecordsEvaluated spfRecordsEvaluated = new SpfRecordsEvaluated(Id, spfRecords, 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            await _spfEntity.Handle(spfRecordsEvaluated);

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordEvaluationsChanged>._, A<string>._)).MustHaveHappened();
            A.CallTo(() => _changeNotifierComposite.Handle(A<SpfEntityState>._, spfRecordsEvaluated))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _domainStatusPublisher.Publish(spfRecordsEvaluated))
                .MustHaveHappenedOnceExactly();
            A.CallTo(() => _spfEntityDao.Save(A<SpfEntityState>._)).MustHaveHappenedOnceExactly();
        }

        private static SpfRecords CreateSpfRecords(List<Message> messages = null, List<Term> terms = null)
        {
            return new SpfRecords(new List<SpfRecord>
                {
                    new SpfRecord(
                        new List<string>
                        {
                            "v=spf1......"
                        },
                        new Version("1", true),
                        terms ?? new List<Term>
                        {
                            new All(Qualifier.Fail, "", false, false)
                        },
                        messages,
                        false)
                }, 100,
                new List<Message>());
        }
    }
}
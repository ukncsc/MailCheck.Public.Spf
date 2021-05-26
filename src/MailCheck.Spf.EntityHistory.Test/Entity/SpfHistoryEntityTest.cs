using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.EntityHistory.Dao;
using MailCheck.Spf.EntityHistory.Entity;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.EntityHistory.Test.Entity
{
    [TestFixture]
    public class SpfHistoryEntityTest
    {
        private const string Id = "abc.com";

        private ILogger<SpfEntityHistory> _log;
        private SpfEntityHistory _spfEntityHistory;
        private ISpfHistoryEntityDao _spfHistoryEntityDao;

        [SetUp]
        public void SetUp()
        {
            _spfHistoryEntityDao = A.Fake<ISpfHistoryEntityDao>();
            _log = A.Fake<ILogger<SpfEntityHistory>>();
            _spfEntityHistory = new SpfEntityHistory(_log, _spfHistoryEntityDao);
        }

        [Test]
        public async Task HandleDomainCreatedCreatesDomain()
        {
            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns<SpfHistoryEntityState>(null);
            await _spfEntityHistory.Handle(new DomainCreated(Id, "test@test.com", DateTime.Now));

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void HandleDomainCreatedThrowsIfEntityAlreadyExistsForDomain()
        {
            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id));
            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleSpfRecordsEvaluatedAndUpdateWhenNoRecordsExistUpdatesHistoryState()
        {
            var spfRecords1 = CreateSpfRecords().Records[0].RecordsStrings;

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, CreateSpfRecords(), 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>.That.Matches(_ =>
                _.SpfHistory.Count == 1 &&
                _.SpfHistory[0].EndDate == null &&
                _.SpfHistory[0].SpfRecords.SequenceEqual(spfRecords1)
            ))).MustHaveHappenedOnceExactly();
        }

        [Test]
        public async Task HandleSpfRecordsEvaluatedAndExistingSpfRecordsHistoryStateNotUpdated()
        {
            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id, new List<SpfHistoryRecord>
            {
                new SpfHistoryRecord(DateTime.UtcNow.AddDays(-1), null, CreateSpfRecords().Records[0].RecordsStrings)
            }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, CreateSpfRecords(), 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleSpfRecordsEvaluatedAndNewSpfRecordUpdatesHistoryWhichHasOnePreviousRecord()
        {
            var spfRecords1 = CreateSpfRecords().Records[0].RecordsStrings;
            var spfRecords2 = CreateSpfRecords("v=spf2......").Records[0].RecordsStrings;

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord>
                {
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-1), null, spfRecords1
                    )
                }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, CreateSpfRecords("v=spf2......"), 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>.That.Matches(_ =>
                _.SpfHistory.Count == 2 &&
                _.SpfHistory[0].EndDate == null &&
                _.SpfHistory[0].SpfRecords.SequenceEqual(spfRecords2) &&
                _.SpfHistory[1].EndDate == polled.Timestamp &&
                _.SpfHistory[1].SpfRecords.SequenceEqual(spfRecords1)
            ))).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleSpfRecordsEvaluatedAndNewSpfRecordUpdatesHistoryWhichHasTwoPreviousRecord()
        {
            var spfRecords1 = CreateSpfRecords().Records[0].RecordsStrings;
            var spfRecords2 = CreateSpfRecords("v=spf2......").Records[0].RecordsStrings;
            var spfRecords3 = CreateSpfRecords("v=spf3......").Records[0].RecordsStrings;

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord>
                {
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-2), null, spfRecords2),
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-3), DateTime.UtcNow.AddDays(-2), spfRecords1)
                }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, CreateSpfRecords("v=spf3......"), 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>.That.Matches(_ =>
                _.SpfHistory.Count == 3 &&
                _.SpfHistory[0].EndDate == null &&
                _.SpfHistory[0].SpfRecords.SequenceEqual(spfRecords3) &&
                _.SpfHistory[1].EndDate == polled.Timestamp &&
                _.SpfHistory[1].SpfRecords.SequenceEqual(spfRecords2) &&
                _.SpfHistory[2].SpfRecords.SequenceEqual(spfRecords1)
            ))).MustHaveHappenedOnceExactly();
        }


        [Test]
        public async Task HandleSpfRecordsEvaluatedWhenRecordsInDifferentOrderButSameRecordsNoUpdate()
        {
            var spfRecord = CreateSpfRecords("one,two");
            spfRecord.Records.AddRange(CreateSpfRecords().Records);

            var spfRecord2 = CreateSpfRecords("two,one");
            spfRecord2.Records.Reverse();

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord>
                {
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-2), null, spfRecord.Records[0].RecordsStrings)
                }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, spfRecord2, 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleSpfRecordsEvaluatedWhenRecordsInSameOrderNoUpdate()
        {
            var spfRecord = CreateSpfRecords("one,two");
            spfRecord.Records.AddRange(CreateSpfRecords().Records);

            var spfRecord2 = CreateSpfRecords("one,two");
            spfRecord2.Records.Reverse();

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord>
                {
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-2), null, spfRecord.Records[0].RecordsStrings)
                }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, spfRecord2, 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustNotHaveHappened();
        }

        [Test]
        public async Task HandleSpfRecordsEvaluatedWhenNewRecords()
        {
            var spfRecord = CreateSpfRecords("one,two");
            spfRecord.Records.AddRange(CreateSpfRecords().Records);

            var spfRecord2 = CreateSpfRecords("two,three");
            spfRecord2.Records.Reverse();

            A.CallTo(() => _spfHistoryEntityDao.Get(Id)).Returns(new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord>
                {
                    new SpfHistoryRecord(DateTime.UtcNow.AddDays(-2), null, spfRecord.Records[0].RecordsStrings)
                }));

            SpfRecordsPolled polled = new SpfRecordsPolled(Id, spfRecord2, 1, TimeSpan.Zero, new List<Message>());

            await _spfEntityHistory.Handle(polled);

            A.CallTo(() => _spfHistoryEntityDao.Save(A<SpfHistoryEntityState>._)).MustHaveHappenedOnceExactly();
        }

        private static SpfRecords CreateSpfRecords(string record = "v=spf1......")
        {
            return new SpfRecords(new List<SpfRecord>
                {
                    new SpfRecord(
                        record.Split(',').ToList(),
                        new Version("1", true),
                        new List<Term>
                        {
                            new All(Qualifier.Fail,"", false, false)
                        },
                        new List<Message>(), true)
                }, 100,
                    new List<Message>());
        }
    }
}

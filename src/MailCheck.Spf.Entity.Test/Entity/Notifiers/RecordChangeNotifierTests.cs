using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Entity.Entity.Notifiers;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using MailCheck.Spf.Contracts.Evaluator;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class RecordChangeNotifierTests
    {
        private IChangeNotifiersComposite _changeNotifier;
        private IMessageDispatcher _dispatcher;
        private ISpfEntityConfig _spfEntityConfig;

        private readonly string Id = "abc.com";

        [SetUp]
        public void SetUp()
        {
            _dispatcher = FakeItEasy.A.Fake<IMessageDispatcher>();
            _spfEntityConfig = FakeItEasy.A.Fake<ISpfEntityConfig>();

            _changeNotifier = new ChangeNotifiersComposite(new List<IChangeNotifier>
            {
                new RecordChangeNotifier(_dispatcher, _spfEntityConfig),
                new RecordMessagesChangeNotifier(_dispatcher, _spfEntityConfig, new MessageEqualityComparer()),
                new ReferencedRecordMessagesChangeNotifier(_dispatcher, _spfEntityConfig, new MessageEqualityComparer()),
                new ReferencedRecordChangeNotifier(_dispatcher, _spfEntityConfig)
            });
        }

        [Test]
        public void SpfRecordsAreSameTest()
        {
            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords = NotifierTestUtil.CreateSpfRecords()
                };
            SpfRecordsPolled message =
                new SpfRecordsPolled(Id, NotifierTestUtil.CreateSpfRecords(), 1, TimeSpan.FromSeconds(1));


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher);
        }

        [Test]
        public void SpfRecordsAreDifferentTest()
        {
            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords = NotifierTestUtil.CreateSpfRecords("spf1")
                };
            SpfRecordsEvaluated message =
                new SpfRecordsEvaluated(Id, NotifierTestUtil.CreateSpfRecords("spf2"), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher, record:true, added: true, removed: true);

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordRemoved>._, A<string>._)).MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordAdded>.That.Matches(
                       _ => _.Records.Count == 1 && _.Records[0].Equals("spf2")),
                   A<string>._))
               .MustHaveHappenedOnceExactly();

            A.CallTo(() => _dispatcher.Dispatch(A<SpfRecordRemoved>.That.Matches(
                     _ => _.Records.Count == 1 && _.Records[0].Equals("spf1")),
                 A<string>._))
             .MustHaveHappenedOnceExactly();
        }
    }
}

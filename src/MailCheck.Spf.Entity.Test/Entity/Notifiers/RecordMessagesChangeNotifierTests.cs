using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Entity.Entity.Notifiers;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class RecordMessagesChangeNotifierTests
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
        public void SpfRecordsWithSameMessageTest()
        {
            SpfRecords spfRecords =
                NotifierTestUtil.CreateSpfRecords();

            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords = NotifierTestUtil.CreateSpfRecords()
                };
            SpfRecordsEvaluated message = new SpfRecordsEvaluated(Id, spfRecords, 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher);
        }


        [Test]
        public void SpfRecordsWithSameMessageIdAndDifferentMessageTypeDoesRaisesOnlySustainedMessageTest()
        {
            Guid id = Guid.NewGuid();

            SpfRecords spfRecords =
                NotifierTestUtil.CreateSpfRecords(messages: new List<Message>
                {
                    new Message(id, "mailcheck.spf.test", "SPF", MessageType.warning, "hello", "markdown")
                });

            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords =
                        NotifierTestUtil.CreateSpfRecords(messages: new List<Message>
                        {
                            new Message(id, "mailcheck.spf.test", "SPF", MessageType.error, "world", "markdown")
                        })
                };
            SpfRecordsEvaluated message = new SpfRecordsEvaluated(Id, spfRecords, 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher, removed: false, added: false);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryAdded>._, A<string>._))
                .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryRemoved>._, A<string>._))
                .MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "world"), A<string>._)).MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisorySustained>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }
        
        [Test]
        public void SpfRecordsWithDifferentMessageRaisesAddedAndRemovedOnlyTest()
        {
            SpfRecords spfRecords =
                NotifierTestUtil.CreateSpfRecords(messages: new List<Message>
                {
                    new Message(Guid.NewGuid(), "mailcheck.spf.test", "SPF", MessageType.info, "hello", "markdown")
                });

            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords =
                        NotifierTestUtil.CreateSpfRecords(messages: new List<Message>
                        {
                            new Message(Guid.NewGuid(), "mailcheck.spf.test", "SPF", MessageType.error, "world", "markdown")
                        })
                };
            SpfRecordsEvaluated message = new SpfRecordsEvaluated(Id, spfRecords, 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher,removed: true, added: true);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryAdded>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryRemoved>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisorySustained>._, A<string>._))
                .MustNotHaveHappened();

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryRemoved>.That.Matches(
                        _ => _.Messages.Count == 1 && _.Messages[0].Text.Equals("world") && _.Messages.Count == 1 &&
                             _.Messages[0].MessageType.ToString().Equals("error")),
                    A<string>._))
                .MustHaveHappenedOnceExactly();

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfAdvisoryAdded>.That.Matches(
                        _ => _.Messages.Count == 1 && _.Messages[0].Text.Equals("hello") && _.Messages.Count == 1 &&
                             _.Messages[0].MessageType.ToString().Equals("info")),
                    A<string>._))
                .MustHaveHappenedOnceExactly();
        }
    }
}

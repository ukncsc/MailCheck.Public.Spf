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
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class ReferencedRecordMessagesChangeNotifierTests
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
        public void SpfRecordsReferencedWithSameMessageTest()
        {
            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords = NotifierTestUtil.CreateSpfRecords("spf1", terms:
                        new List<Term>
                        {
                            new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                                new List<Term>
                                {
                                    new Include(Qualifier.Fail, "spf3", "abc.com", NotifierTestUtil.CreateSpfRecords("spf3"), false)
                                }), false)
                        })
                };

            SpfRecordsEvaluated message =
                new SpfRecordsEvaluated(Id, NotifierTestUtil.CreateSpfRecords("spf1", terms:
                    new List<Term>
                    {
                        new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                            new List<Term>
                            {
                                new Include(Qualifier.Fail, "spf3", "abc.com", NotifierTestUtil.CreateSpfRecords("spf3"), false)
                            }), false)
                    }), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);

            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher, referencedChange: true);
        }

        [Test]
        public void SpfRecordsReferencedWithSameMessageIdAndDifferentMessageTypeDoesNotRaiseANotificationTest()
        {
            Guid id = Guid.NewGuid();

            SpfEntityState state =
               new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
               {
                   SpfRecords = NotifierTestUtil.CreateSpfRecords("spf1", terms:
                       new List<Term>
                       {
                            new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                            new List<Term>
                            {
                                new Include(Qualifier.Fail, "spf3", "abc.com",
                                    NotifierTestUtil.CreateSpfRecords("spf3",
                                        new List<Message>
                                        {
                                            new Message(id, "SPF", MessageType.warning, "hello world", "markdown")
                                        }), false)
                            }), false)
                       })
               };
            SpfRecordsEvaluated message =
                new SpfRecordsEvaluated(Id, NotifierTestUtil.CreateSpfRecords("spf1", terms:
                    new List<Term>
                    {
                        new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                            new List<Term>
                            {
                                new Include(Qualifier.Fail, "spf3", "abc.com",
                                    NotifierTestUtil.CreateSpfRecords("spf3",
                                        new List<Message>
                                        {
                                            new Message(id, "SPF", MessageType.error, "hello world", "markdown")
                                        }), false)
                            }), false)
                    }), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
        }

        [Test]
        public void SpfRecordsReferencedWithDifferentMessageTest()
        {
            SpfEntityState state =
                new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.Now)
                {
                    SpfRecords = NotifierTestUtil.CreateSpfRecords("spf1", terms:
                        new List<Term>
                        {
                            new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                                new List<Term>
                                {
                                    new Include(Qualifier.Fail, "spf3", "abc.com", NotifierTestUtil.CreateSpfRecords("spf3"), false)
                                }), false)
                        })
                };
            SpfRecordsEvaluated message =
                new SpfRecordsEvaluated(Id, NotifierTestUtil.CreateSpfRecords("spf1", terms:
                    new List<Term>
                    {
                        new Include(Qualifier.Fail, "spf2", "abc.com", NotifierTestUtil.CreateSpfRecords("spf2", terms:
                            new List<Term>
                            {
                                new Include(Qualifier.Fail, "spf3", "abc.com",
                                    NotifierTestUtil.CreateSpfRecords("spf3",
                                        new List<Message>
                                        {
                                            new Message(Guid.NewGuid(), "SPF", MessageType.info, "hello world", "markdown")
                                        }), false)
                            }), false)
                    }), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher, referencedChange: true, added: true);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>.That.Matches(
                       _ => _.Messages.Count == 1 && _.Messages[0].Text.Equals("hello world") && _.Messages[0].MessageType.ToString().Equals("info")),
                   A<string>._))
               .MustHaveHappenedOnceExactly();
        }
    }
}

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
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class ReferencedRecordMessagesChangeNotifierTests
    {
        private ReferencedRecordMessagesChangeNotifier _changeNotifier;
        private IMessageDispatcher _dispatcher;
        private ISpfEntityConfig _spfEntityConfig;

        private readonly string Id = "abc.com";

        [SetUp]
        public void SetUp()
        {
            _dispatcher = FakeItEasy.A.Fake<IMessageDispatcher>();
            _spfEntityConfig = FakeItEasy.A.Fake<ISpfEntityConfig>();

            _changeNotifier = new ReferencedRecordMessagesChangeNotifier(_dispatcher, _spfEntityConfig, new MessageEqualityComparer());
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
        public void SpfRecordsReferencedExistingMessageRaisesSustainedOnlyTest()
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
                                            new Message(id, "SPF", "mailcheck.spf.test", MessageType.warning, "hello world", "markdown")
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
                                            new Message(id, "SPF", "mailcheck.spf.test", MessageType.error, "hello world", "markdown")
                                        }), false)
                            }), false)
                    }), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._)).MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "hello"), A<string>._)).MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisorySustained>._, A<string>._))
                .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void SpfRecordsReferencedWithDifferentMessageRaisesAddedOnlyTest()
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
                                            new Message(Guid.NewGuid(), "mailcheck.spf.test", "SPF", MessageType.info, "hello world", "markdown")
                                        }), false)
                            }), false)
                    }), 1, TimeSpan.MinValue, new List<Message>(), DateTime.UtcNow);


            _changeNotifier.Handle(state, message);

            NotifierTestUtil.VerifyResults(_dispatcher, referencedChange: true, added: true);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>._, A<string>._)).MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>._, A<string>._)).MustNotHaveHappened();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisorySustained>.That.Matches(x => x.Messages.First().Text == "hello"), A<string>._)).MustHaveHappenedOnceExactly();
            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisorySustained>._, A<string>._))
                .MustHaveHappenedOnceExactly();

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>.That.Matches(
                       _ => _.Messages.Count == 1 && _.Messages[0].Text.Equals("hello world") && _.Messages[0].MessageType.ToString().Equals("info")),
                   A<string>._))
               .MustHaveHappenedOnceExactly();
        }

        [Test]
        public void IncludedRecordMessagesAddedCausesNotifications()
        {
            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithIncludesWithoutMessages());
            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithIncludesWithMessages());

            _changeNotifier.Handle(state, evaluationResult);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryAdded>.That.Matches(x => x.Messages.Count == 2), A<string>._)).MustHaveHappenedOnceExactly();
            Assert.AreEqual(1, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void IncludedRecordMessagesRemovedCausesNotifications()
        {
            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithIncludesWithMessages());
            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithIncludesWithoutMessages());

            _changeNotifier.Handle(state, evaluationResult);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisoryRemoved>.That.Matches(x => x.Messages.Count == 2), A<string>._)).MustHaveHappenedOnceExactly();
            Assert.AreEqual(1, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void IncludedRecordMessagesSustainedCausesNotifications()
        {
            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithIncludesWithMessages());
            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithIncludesWithMessages());

            _changeNotifier.Handle(state, evaluationResult);

            FakeItEasy.A.CallTo(() => _dispatcher.Dispatch(A<SpfReferencedAdvisorySustained>.That.Matches(x => x.Messages.Count == 2), A<string>._)).MustHaveHappenedOnceExactly();
            Assert.AreEqual(1, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void IncludedRecordMessagesAbsentCausesNoNotifications()
        {
            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithIncludesWithoutMessages());
            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithIncludesWithoutMessages());

            _changeNotifier.Handle(state, evaluationResult);

            Assert.AreEqual(0, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void ParentRecordMessagesAddedCausesNoNotifications()
        {
            Message rootMessage = new Message(Guid.Parse("11111111-1111-1111-1111-111111111111"), "mailcheck.spf.test", "testSource", MessageType.error, "rootText", "rootMarkdown");

            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithoutMessages());
            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithMessages(), new List<Message> { rootMessage });

            _changeNotifier.Handle(state, evaluationResult);
            Assert.AreEqual(0, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void ParentRecordMessagesRemovedCausesNoNotifications()
        {
            Message rootMessage = new Message(Guid.Parse("22222222-2222-2222-2222-222222222222"), "mailcheck.spf.test", "testSource", MessageType.error, "rootText", "rootMarkdown");

            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithMessages());
            state.Messages = new List<Message> { rootMessage };

            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithoutMessages());

            _changeNotifier.Handle(state, evaluationResult);
            Assert.AreEqual(0, Fake.GetCalls(_dispatcher).Count());
        }

        [Test]
        public void ParentRecordMessagesSustainedCausesNoNotifications()
        {
            Message rootMessage = new Message(Guid.Parse("33333333-3333-3333-3333-333333333333"), "mailcheck.spf.test", "testSource", MessageType.error, "rootText", "rootMarkdown");

            SpfEntityState state = CreateExistingState(CreateSpfRecordsWithMessages());
            state.Messages = new List<Message> { rootMessage };

            SpfRecordsEvaluated evaluationResult = CreateEvaluationResult(CreateSpfRecordsWithMessages(), new List<Message> { rootMessage });

            _changeNotifier.Handle(state, evaluationResult);

            Assert.AreEqual(0, Fake.GetCalls(_dispatcher).Count());
        }

        private SpfEntityState CreateExistingState(SpfRecords rootSpfRecords)
        {
            SpfEntityState state = new SpfEntityState(Id, 1, SpfState.Evaluated, DateTime.MinValue)
            {
                SpfRecords = rootSpfRecords
            };

            return state;
        }

        private SpfRecordsEvaluated CreateEvaluationResult(SpfRecords rootSpfRecords, List<Message> messages = null)
        {
            messages = messages ?? new List<Message>();
            SpfRecordsEvaluated evaluationResult = new SpfRecordsEvaluated(Id, rootSpfRecords, 1, TimeSpan.MinValue, messages, DateTime.MinValue);

            return evaluationResult;
        }

        private SpfRecords CreateSpfRecordsWithoutMessages()
        {
            List<SpfRecord> internalRecords = new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty, true), new List<Term>(), new List<Message>(), true)
            };

            SpfRecords spfRecords = new SpfRecords(internalRecords, 0, new List<Message>());
            return spfRecords;
        }

        private SpfRecords CreateSpfRecordsWithMessages()
        {
            Message individualRecordMessage = new Message(Guid.Parse("44444444-4444-4444-4444-444444444444"), "mailcheck.spf.test", string.Empty, MessageType.error, "recordText", "recordMarkdown");
            List<SpfRecord> internalRecords = new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty, true), new List<Term>(), new List<Message>{ individualRecordMessage }, true)
            };

            Message spfRecordsMessage = new Message(Guid.Parse("55555555-5555-5555-5555-555555555555"), "mailcheck.spf.test", string.Empty, MessageType.error, "spfRecordsText", "spfRecordsMarkdown");
            SpfRecords spfRecords = new SpfRecords(internalRecords, 0, new List<Message> { spfRecordsMessage });
            return spfRecords;
        }

        private SpfRecords CreateSpfRecordsWithIncludesWithoutMessages()
        {
            List<SpfRecord> includeInternalRecords = new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty, true), new List<Term>(), new List<Message>(), false)
            };

            SpfRecords includeSpfRecords = new SpfRecords(includeInternalRecords, 0, new List<Message>());

            Include include = new Include(Qualifier.Fail, string.Empty, string.Empty, includeSpfRecords, true);

            SpfRecords spfRecords = CreateSpfRecordsWithoutMessages();
            spfRecords.Records[0].Terms.Add(include);

            return spfRecords;
        }

        private SpfRecords CreateSpfRecordsWithIncludesWithMessages()
        {
            Message includeIndividualRecordMessage = new Message(Guid.Parse("66666666-6666-6666-6666-666666666666"), "mailcheck.spf.test", string.Empty, MessageType.error, "recordText", "recordMarkdown");
            List<SpfRecord> includeInternalRecords = new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Version(string.Empty, true), new List<Term>(), new List<Message> { includeIndividualRecordMessage }, false)
            };

            Message includeSpfRecordsMessage = new Message(Guid.Parse("77777777-7777-7777-7777-777777777777"), "mailcheck.spf.test", string.Empty, MessageType.error, "spfRecordsText", "spfRecordsMarkdown");
            SpfRecords includeSpfRecords = new SpfRecords(includeInternalRecords, 0, new List<Message> { includeSpfRecordsMessage });

            Include include = new Include(Qualifier.Fail, string.Empty, string.Empty, includeSpfRecords, true);

            SpfRecords spfRecords = CreateSpfRecordsWithoutMessages();
            spfRecords.Records[0].Terms.Add(include);

            return spfRecords;
        }
    }
}

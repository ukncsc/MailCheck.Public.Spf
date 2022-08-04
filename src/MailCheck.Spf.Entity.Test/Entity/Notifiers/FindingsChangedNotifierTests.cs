using System;
using System.Collections.Generic;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using NUnit.Framework;
using LocalNotifier = MailCheck.Spf.Entity.Entity.Notifiers.FindingsChangedNotifier;
using ErrorMessage = MailCheck.Spf.Contracts.SharedDomain.Message;
using MailCheck.Common.Contracts.Findings;
using System.Linq;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Contracts.SharedDomain;
using A = FakeItEasy.A;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Contracts;
using MailCheck.Spf.Contracts.Evaluator;

namespace MailCheck.Spf.Entity.Test.Entity.Notifiers
{
    [TestFixture]
    public class FindingsChangedNotifierTests
    {
        private IMessageDispatcher _messageDispatcher;
        private IFindingsChangedNotifier _findingsChangedNotifier;
        private ISpfEntityConfig _spfEntityConfig;

        private LocalNotifier _notifier;

        private const string Id = "test.gov.uk";

        [SetUp]
        public void SetUp()
        {
            _messageDispatcher = A.Fake<IMessageDispatcher>();
            _findingsChangedNotifier = new FindingsChangedNotifier();
            _spfEntityConfig = A.Fake<ISpfEntityConfig>();
            _notifier = new LocalNotifier(_messageDispatcher, _findingsChangedNotifier, _spfEntityConfig);
        }

        [TestCaseSource(nameof(ExerciseFindingsChangedNotifierTestPermutations))]
        public void ExerciseFindingsChangedNotifier(FindingsChangedNotifierTestCase testCase)
        {
            A.CallTo(() => _spfEntityConfig.WebUrl).Returns("testurl.com");

            SpfRecords stateSpfRecords = CreateSpfRecords();
            stateSpfRecords.Messages = testCase.StateRecordsMessages;
            stateSpfRecords.Records[0].Messages = testCase.StateRecordsRecordMessages ?? new List<ErrorMessage>();

            SpfEntityState state = new SpfEntityState(Id, 2, Contracts.Entity.SpfState.PollPending, DateTime.Now)
            {
                LastUpdated = DateTime.Now.AddDays(-1),
                SpfRecords = stateSpfRecords,
                Messages = testCase.StateMessages
            };

            SpfRecords resultSpfRecords = CreateSpfRecords();
            resultSpfRecords.Messages = testCase.ResultRecordsMessages ?? new List<ErrorMessage>();
            resultSpfRecords.Records[0].Messages = testCase.ResultRecordsRecordMessages ?? new List<ErrorMessage>();

            SpfRecordsEvaluated spfRecordsEvaluated = new SpfRecordsEvaluated(Id, resultSpfRecords, null, TimeSpan.MaxValue, testCase.ResultMessages, DateTime.MinValue);

            _notifier.Handle(state, spfRecordsEvaluated);

            A.CallTo(() => _messageDispatcher.Dispatch(
                A<FindingsChanged>.That.Matches(x =>
                    x.Added.Count == testCase.ExpectedAdded.Count &&
                    x.Removed.Count == testCase.ExpectedRemoved.Count &&
                    x.Sustained.Count == testCase.ExpectedSustained.Count), A<string>._)).MustHaveHappenedOnceExactly();

            for (int i = 0; i < testCase.ExpectedAdded.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Added[i].Name == testCase.ExpectedAdded[i].Name &&
                        x.Added[i].EntityUri == testCase.ExpectedAdded[i].EntityUri &&
                        x.Added[i].SourceUrl == testCase.ExpectedAdded[i].SourceUrl &&
                        x.Added[i].Severity == testCase.ExpectedAdded[i].Severity &&
                        x.Added[i].Title == testCase.ExpectedAdded[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };

            for (int i = 0; i < testCase.ExpectedRemoved.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Removed[i].Name == testCase.ExpectedRemoved[i].Name &&
                        x.Removed[i].EntityUri == testCase.ExpectedRemoved[i].EntityUri &&
                        x.Removed[i].SourceUrl == testCase.ExpectedRemoved[i].SourceUrl &&
                        x.Removed[i].Severity == testCase.ExpectedRemoved[i].Severity &&
                        x.Removed[i].Title == testCase.ExpectedRemoved[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };

            for (int i = 0; i < testCase.ExpectedSustained.Count; i++)
            {
                A.CallTo(() => _messageDispatcher.Dispatch(
                    A<FindingsChanged>.That.Matches(x =>
                        x.Sustained[i].Name == testCase.ExpectedSustained[i].Name &&
                        x.Sustained[i].EntityUri == testCase.ExpectedSustained[i].EntityUri &&
                        x.Sustained[i].SourceUrl == testCase.ExpectedSustained[i].SourceUrl &&
                        x.Sustained[i].Severity == testCase.ExpectedSustained[i].Severity &&
                        x.Sustained[i].Title == testCase.ExpectedSustained[i].Title), A<string>._)).MustHaveHappenedOnceExactly();
            };
        }

        private static IEnumerable<FindingsChangedNotifierTestCase> ExerciseFindingsChangedNotifierTestPermutations()
        {
            ErrorMessage evalError1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.spf.testName1", MessageSources.SpfEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage evalError2 = new ErrorMessage(Guid.NewGuid(), "mailcheck.spf.testName2", MessageSources.SpfEvaluator, MessageType.error, "EvaluationError", string.Empty);
            ErrorMessage pollerWarn1 = new ErrorMessage(Guid.NewGuid(), "mailcheck.spf.testName3", MessageSources.SpfPoller, MessageType.warning, "PollerError", string.Empty);

            Finding findingEvalError1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.spf.testName1",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/spf",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingEvalError2 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.spf.testName2",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/spf",
                Severity = "Urgent",
                Title = "EvaluationError"
            };

            Finding findingPollerWarn1 = new Finding
            {
                EntityUri = "domain:test.gov.uk",
                Name = "mailcheck.spf.testName3",
                SourceUrl = $"https://testurl.com/app/domain-security/{Id}/spf",
                Severity = "Advisory",
                Title = "PollerError"
            };

            FindingsChangedNotifierTestCase test1 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = new List<ErrorMessage>(),
                ResultRecordsMessages = new List<ErrorMessage>(),
                ResultRecordsRecordMessages = new List<ErrorMessage>(),
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedSustained = new List<Finding>(),
                Description = "3 removed messages should produce 3 findings removed"
            };

            FindingsChangedNotifierTestCase test2 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                Description = "3 sustained messages should produce 3 findings sustained"
            };

            FindingsChangedNotifierTestCase test3 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage>(),
                StateRecordsMessages = new List<ErrorMessage>(),
                StateRecordsRecordMessages = new List<ErrorMessage>(),
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding>(),
                Description = "3 added messages should produce 3 findings added"
            };

            FindingsChangedNotifierTestCase test4 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage>(),
                StateRecordsRecordMessages = new List<ErrorMessage>(),
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1 },
                Description = "2 added messages and 1 sustained should produce 2 findings added and 1 finding sustained"
            };

            FindingsChangedNotifierTestCase test5 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = new List<ErrorMessage> { evalError2 },
                StateRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ResultMessages = null,
                ResultRecordsMessages = null,
                ResultRecordsRecordMessages = null,
                ExpectedAdded = new List<Finding>(),
                ExpectedRemoved = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedSustained = new List<Finding>(),
                Description = "3 removed messages due to nulls should produce 3 findings removed"
            };

            FindingsChangedNotifierTestCase test6 = new FindingsChangedNotifierTestCase
            {
                StateMessages = null,
                StateRecordsMessages = null,
                StateRecordsRecordMessages = null,
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError1, findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding>(),
                Description = "3 added messages from nulls should produce 3 findings added"
            };

            FindingsChangedNotifierTestCase test7 = new FindingsChangedNotifierTestCase
            {
                StateMessages = new List<ErrorMessage> { evalError1 },
                StateRecordsMessages = null,
                StateRecordsRecordMessages = null,
                ResultMessages = new List<ErrorMessage> { evalError1 },
                ResultRecordsMessages = new List<ErrorMessage> { evalError2 },
                ResultRecordsRecordMessages = new List<ErrorMessage> { pollerWarn1 },
                ExpectedAdded = new List<Finding> { findingEvalError2, findingPollerWarn1 },
                ExpectedRemoved = new List<Finding>(),
                ExpectedSustained = new List<Finding> { findingEvalError1 },
                Description = "2 added messages from nulls and 1 sustained should produce 2 findings added and 1 finding sustained"
            };

            yield return test1;
            yield return test2;
            yield return test3;
            yield return test4;
            yield return test5;
            yield return test6;
            yield return test7;
        }

        private static SpfRecords CreateSpfRecords(string domain = "test.gov.uk")
        {
            return new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string>(), new Contracts.SharedDomain.Version("1", true), new List<Term>(), new List<ErrorMessage>(), true)
            }, 100, new List<ErrorMessage>()); ;
        }

        public class FindingsChangedNotifierTestCase
        {
            public List<ErrorMessage> StateMessages { get; set; }
            public List<ErrorMessage> StateRecordsMessages { get; set; }
            public List<ErrorMessage> StateRecordsRecordMessages { get; set; }
            public List<ErrorMessage> ResultMessages { get; set; }
            public List<ErrorMessage> ResultRecordsMessages { get; set; }
            public List<ErrorMessage> ResultRecordsRecordMessages { get; set; }
            public List<Finding> ExpectedAdded { get; set; }
            public List<Finding> ExpectedRemoved { get; set; }
            public List<Finding> ExpectedSustained { get; set; }
            public string Description { get; set; }

            public override string ToString()
            {
                return Description;
            }
        }
    }
}
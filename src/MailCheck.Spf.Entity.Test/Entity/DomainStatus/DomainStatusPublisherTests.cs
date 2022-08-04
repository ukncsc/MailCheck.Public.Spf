using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.DomainStatus.Contracts;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.DomainStatus;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusPublisherTests
    {
        private DomainStatusPublisher _domainStatusPublisher;
        private IMessageDispatcher _dispatcher;
        private ISpfEntityConfig _spfEntityConfig;
        private ILogger<DomainStatusPublisher> _logger;
        private IDomainStatusEvaluator _domainStatusEvaluator;

        [SetUp]
        public void SetUp()
        {
            StartUp.StartUp.ConfigureSerializerSettings();
            _dispatcher = A.Fake<IMessageDispatcher>();
            _spfEntityConfig = A.Fake<ISpfEntityConfig>();
            _logger = A.Fake<ILogger<DomainStatusPublisher>>();
            _domainStatusEvaluator = A.Fake<IDomainStatusEvaluator>();
            A.CallTo(() => _spfEntityConfig.SnsTopicArn).Returns("testSnsTopicArn");

            _domainStatusPublisher = new DomainStatusPublisher(_dispatcher, _spfEntityConfig, _domainStatusEvaluator, _logger);
        }

        [Test]
        public void PublisherDeterminesStatusAndDispatchesIt()
        {
            Message rootMessage = CreateInfoMessage();
            Message recordsMessage = CreateInfoMessage();
            Message recordsRecordsMessages = CreateInfoMessage();

            SpfRecordsEvaluated message = new SpfRecordsEvaluated("testDomain", new SpfRecords(new List<SpfRecord> { new SpfRecord(null, null, null, new List<Message> { recordsRecordsMessages }, false) }, 0, new List<Message> { recordsMessage }), null, null, new List<Message> { rootMessage }, DateTime.MinValue);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage) && x.Contains(recordsRecordsMessages)))).Returns(Status.Info);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Info &&
                x.RecordType == "SPF" &&
                x.Id == "testDomain";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void GetMessagesPicksUpMessagesSeveralLayersIn()
        {
            Message rootMessage = CreateInfoMessage();
            Message recordsMessage = CreateInfoMessage();
            Message spfRecordWithIncludeMessage = CreateInfoMessage();
            Message includeTermMessage = CreateErrorMessage();
            Message includeTermSpfRecordMessage = CreateErrorMessage();

            SpfRecord includeTermSpfRecord = new SpfRecord(null, null, null, new List<Message> { includeTermSpfRecordMessage }, null);

            Include includeTerm = new Include(Qualifier.Pass, null, null, new SpfRecords(new List<SpfRecord> { includeTermSpfRecord }, 100, new List<Message> { includeTermMessage }), false);

            SpfRecord spfRecordWithInclude = new SpfRecord(null, null, new List<Term> { includeTerm }, new List<Message> { spfRecordWithIncludeMessage }, false);

            SpfRecords rootSpfRecords = new SpfRecords(new List<SpfRecord> { spfRecordWithInclude }, 100, new List<Message> { recordsMessage });

            SpfRecordsEvaluated message = new SpfRecordsEvaluated("test.gov.uk", rootSpfRecords, null, null, new List<Message> { rootMessage }, DateTime.UtcNow);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage) && x.Contains(spfRecordWithIncludeMessage) && x.Contains(includeTermMessage) && x.Contains(includeTermSpfRecordMessage)))).Returns(Status.Error);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Error &&
                x.RecordType == "SPF" &&
                x.Id == "test.gov.uk";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NullChildrenDoesntErrorGetMessagesOut()
        {
            Message rootMessage = CreateInfoMessage();
            Message recordsMessage = CreateInfoMessage();
            Message spfRecordWithIncludeMessage = CreateInfoMessage();
            Message includeTermMessage = CreateErrorMessage();

            Include includeTerm = new Include(Qualifier.Pass, null, null, new SpfRecords(new List<SpfRecord> { null }, 100, new List<Message> { includeTermMessage }), false);

            SpfRecord spfRecordWithInclude = new SpfRecord(null, null, new List<Term> { includeTerm }, new List<Message> { spfRecordWithIncludeMessage }, false);

            SpfRecords rootSpfRecords = new SpfRecords(new List<SpfRecord> { spfRecordWithInclude }, 100, new List<Message> { recordsMessage });

            SpfRecordsEvaluated message = new SpfRecordsEvaluated("test.gov.uk", rootSpfRecords, null, null, new List<Message> { rootMessage }, DateTime.UtcNow);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage) && x.Contains(spfRecordWithIncludeMessage) && x.Contains(includeTermMessage)))).Returns(Status.Error);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Error &&
                x.RecordType == "SPF" &&
                x.Id == "test.gov.uk";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NullTermsDoesntErrorGetMessagesOut()
        {
            Message rootMessage = CreateInfoMessage();
            Message recordsMessage = CreateInfoMessage();
            Message spfRecordWithIncludeMessage = CreateInfoMessage();

            SpfRecord spfRecordWithInclude = new SpfRecord(null, null, new List<Term> { null }, new List<Message> { spfRecordWithIncludeMessage }, false);

            SpfRecords rootSpfRecords = new SpfRecords(new List<SpfRecord> { spfRecordWithInclude }, 100, new List<Message> { recordsMessage });

            SpfRecordsEvaluated message = new SpfRecordsEvaluated("test.gov.uk", rootSpfRecords, null, null, new List<Message> { rootMessage }, DateTime.UtcNow);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage) && x.Contains(spfRecordWithIncludeMessage)))).Returns(Status.Error);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Error &&
                x.RecordType == "SPF" &&
                x.Id == "test.gov.uk";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void EmptyRecordsDoesntErrorGetMessagesOut()
        {
            Message rootMessage = CreateInfoMessage();
            Message recordsMessage = CreateInfoMessage();

            SpfRecords rootSpfRecords = new SpfRecords(new List<SpfRecord>(), 100, new List<Message> { recordsMessage });

            SpfRecordsEvaluated message = new SpfRecordsEvaluated("test.gov.uk", rootSpfRecords, null, null, new List<Message> { rootMessage }, DateTime.UtcNow);

            A.CallTo(() => _domainStatusEvaluator.GetStatus(A<List<Message>>.That.Matches(x => x.Contains(rootMessage) && x.Contains(recordsMessage)))).Returns(Status.Error);

            _domainStatusPublisher.Publish(message);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Error &&
                x.RecordType == "SPF" &&
                x.Id == "test.gov.uk";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void MechanismsWithEmptySpfRecordsMessagesParsesCorrectly()
        {
            string message = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "cumbria.json"));
            SpfRecordsEvaluated testSpfRecordsEvaluated = JsonConvert.DeserializeObject<SpfRecordsEvaluated>(message);
            _domainStatusPublisher.Publish(testSpfRecordsEvaluated);

            Expression<Func<DomainStatusEvaluation, bool>> predicate = x =>
                x.Status == Status.Success &&
                x.RecordType == "SPF" &&
                x.Id == "cumbria.ac.uk";

            A.CallTo(() => _dispatcher.Dispatch(A<DomainStatusEvaluation>.That.Matches(predicate), "testSnsTopicArn")).MustHaveHappenedOnceExactly();
        }

        private Message CreateInfoMessage()
        {
            return new Message(Guid.Empty, "mailcheck.spf.test", "", MessageType.info, "", "");
        }

        private Message CreateWarningMessage()
        {
            return new Message(Guid.Empty, "mailcheck.spf.test", "", MessageType.warning, "", "");
        }

        private Message CreateErrorMessage()
        {
            return new Message(Guid.Empty, "mailcheck.spf.test", "", MessageType.error, "", "");
        }
    }
}

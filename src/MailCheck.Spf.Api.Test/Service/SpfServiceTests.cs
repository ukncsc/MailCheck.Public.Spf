using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Api.Config;
using MailCheck.Spf.Api.Dao;
using MailCheck.Spf.Api.Domain;
using MailCheck.Spf.Api.Service;
using MailCheck.Spf.Contracts.Entity;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace MailCheck.Spf.Api.Test.Service
{
    [TestFixture]
    public class SpfServiceTests
    {
        private SpfService _spfService;
        private IMessagePublisher _messagePublisher;
        private ISpfApiDao _dao;
        private ISpfApiConfig _config;
        private ILogger<SpfService> _log;

        [SetUp]
        public void SetUp()
        {
            _messagePublisher = A.Fake<IMessagePublisher>();
            _dao = A.Fake<ISpfApiDao>();
            _config = A.Fake<ISpfApiConfig>();
            _log = A.Fake<ILogger<SpfService>>();
            _spfService = new SpfService(_messagePublisher, _dao, _config, _log);
        }

        [Test]
        public async Task PublishesDomainMissingMessageWhenDomainDoesNotExist()
        {
            A.CallTo(() => _dao.GetSpfForDomain("testDomain"))
                .Returns(Task.FromResult<SpfInfoResponse>(null));

            SpfInfoResponse result = await _spfService.GetSpfForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustHaveHappenedOnceExactly();
            Assert.AreEqual(null, result);
        }

        [Test]
        public async Task DoesNotPublishDomainMissingMessageWhenDomainExists()
        {
            SpfInfoResponse spfInfoResponse = new SpfInfoResponse("", SpfState.Created);
            A.CallTo(() => _dao.GetSpfForDomain("testDomain"))
                .Returns(Task.FromResult(spfInfoResponse));

            SpfInfoResponse result = await _spfService.GetSpfForDomain("testDomain");

            A.CallTo(() => _messagePublisher.Publish(A<DomainMissing>._, A<string>._))
                .MustNotHaveHappened();
            Assert.AreSame(spfInfoResponse, result);

        }
    }
}
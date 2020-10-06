using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Api.Config;
using MailCheck.Spf.Api.Controllers;
using MailCheck.Spf.Api.Dao;
using MailCheck.Spf.Api.Domain;
using MailCheck.Spf.Api.Service;
using MailCheck.Spf.Contracts.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Api.Test.Controllers
{
    [TestFixture]
    public class SpfControllerTests
    {
        private SpfController _sut;
        private ISpfApiDao _dao;
        private IMessagePublisher _messagePublisher;
        private ISpfApiConfig _config;
        private ISpfService _spfService;
        private ILogger<SpfController> _log;

        [SetUp]
        public void SetUp()
        {
            _dao = A.Fake<ISpfApiDao>();
            _messagePublisher = A.Fake<IMessagePublisher>();
            _config = A.Fake<ISpfApiConfig>();
            _spfService = A.Fake<ISpfService>();
            _log = A.Fake<ILogger<SpfController>>();
            _sut = new SpfController(_dao, _messagePublisher, _config, _spfService);
        }

        [Test]
        public async Task ItShouldReturnNotFoundWhenThereIsNoSpfState()
        {
            A.CallTo(() => _spfService.GetSpfForDomain(A<string>._))
                .Returns(Task.FromResult<SpfInfoResponse>(null));

            ObjectResult response = (ObjectResult)await _sut.GetSpf(new SpfDomainRequest { Domain = "ncsc.gov.uk" });

            Assert.That(response, Is.TypeOf(typeof(NotFoundObjectResult)));
        }

        [Test]
        public async Task ItShouldReturnTheFirstResultWhenTheSpfStateExists()
        {
            SpfInfoResponse state = new SpfInfoResponse("ncsc.gov.uk", SpfState.Created);

            A.CallTo(() => _spfService.GetSpfForDomain(A<string>._))
                .Returns(Task.FromResult(state));

            ObjectResult response = (ObjectResult)await _sut.GetSpf(new SpfDomainRequest { Domain = "ncsc.gov.uk" });

            Assert.AreSame(response.Value, state);
        }

        [Test]
        public void AllMethodsHaveAuthorisation()
        {
            IEnumerable<MethodInfo> controllerMethods = _sut.GetType().GetMethods().Where(x => x.DeclaringType == typeof(SpfController));

            foreach (MethodInfo methodInfo in controllerMethods)
            {
               Assert.That(methodInfo.CustomAttributes.Any(x=>x.AttributeType == typeof(MailCheckAuthoriseResourceAttribute) || x.AttributeType == typeof(MailCheckAuthoriseRoleAttribute)));
            }
        }
    }
}

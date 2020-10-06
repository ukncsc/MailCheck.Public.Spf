using System.Threading.Tasks;
using MailCheck.Common.Api.Authorisation.Filter;
using MailCheck.Common.Api.Authorisation.Service.Domain;
using MailCheck.Common.Api.Domain;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Api.Config;
using MailCheck.Spf.Api.Dao;
using MailCheck.Spf.Api.Domain;
using MailCheck.Spf.Api.Service;
using MailCheck.Spf.Contracts.Scheduler;
using Microsoft.AspNetCore.Mvc;

namespace MailCheck.Spf.Api.Controllers
{   
    [Route("/api/spf")]
    public class SpfController : Controller
    {
        private readonly ISpfApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ISpfApiConfig _config;
        private readonly ISpfService _spfService;

        public SpfController(ISpfApiDao dao,
            IMessagePublisher messagePublisher,
            ISpfApiConfig config, 
            ISpfService spfService)
        {
            _dao = dao;
            _messagePublisher = messagePublisher;
            _config = config;
            _spfService = spfService;
        }

        [HttpGet("{domain}/recheck")]
        [MailCheckAuthoriseResource(Operation.Update, ResourceType.Spf, "domain")]
        public async Task<IActionResult> RecheckSpf(SpfDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            await _messagePublisher.Publish(new SpfRecordExpired(request.Domain), _config.SnsTopicArn);

            return new OkObjectResult("{}");
        }

        [HttpGet("{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.Spf, "domain")]
        public async Task<IActionResult> GetSpf(SpfDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            SpfInfoResponse response = await _spfService.GetSpfForDomain(request.Domain);

            if (response == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No Spf found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return new ObjectResult(response);
        }

        [HttpPost("domains")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.Spf, "domain")]
        public async Task<IActionResult> GetSpfStates([FromBody] SpfInfoListRequest request)
        {
            return new ObjectResult(await _dao.GetSpfForDomains(request.HostNames));
        }

        [HttpGet("history/{domain}")]
        [MailCheckAuthoriseResource(Operation.Read, ResourceType.SpfHistory, "domain")]
        public async Task<IActionResult> GetSpfHistory(SpfDomainRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ErrorResponse(ModelState.Values));
            }

            string history = await _dao.GetSpfHistory(request.Domain);

            if (history == null)
            {
                return new NotFoundObjectResult(new ErrorResponse($"No Spf History found for {request.Domain}",
                    ErrorStatus.Information));
            }

            return Content(history, "application/json");
        }
    }
}
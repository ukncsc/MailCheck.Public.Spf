using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Api.Config;
using MailCheck.Spf.Api.Dao;
using MailCheck.Spf.Api.Domain;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Api.Service
{
    public interface ISpfService
    {
        Task<SpfInfoResponse> GetSpfForDomain(string requestDomain);
    }

    public class SpfService : ISpfService
    {
        private readonly ISpfApiDao _dao;
        private readonly IMessagePublisher _messagePublisher;
        private readonly ISpfApiConfig _config;
        private readonly ILogger<SpfService> _log;

        public SpfService(IMessagePublisher messagePublisher, ISpfApiDao dao, ISpfApiConfig config, ILogger<SpfService> log)
        {
            _messagePublisher = messagePublisher;
            _dao = dao;
            _config = config;
            _log = log;
        }

        public async Task<SpfInfoResponse> GetSpfForDomain(string requestDomain)
        {
            SpfInfoResponse response = await _dao.GetSpfForDomain(requestDomain);

            if (response is null)
            {
                _log.LogInformation($"Spf entity state does not exist for domain {requestDomain} - publishing DomainMissing");
                await _messagePublisher.Publish(new DomainMissing(requestDomain), _config.MicroserviceOutputSnsTopicArn);
            }

            return response;
        }
    }
}

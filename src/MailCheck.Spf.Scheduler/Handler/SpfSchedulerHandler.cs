using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MailCheck.Spf.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Scheduler.Handler
{
    public class SpfSchedulerHandler : IHandle<SpfEntityCreated>, IHandle<DomainDeleted>
    {
        private readonly ISpfSchedulerDao _dao;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfSchedulerConfig _config;
        private readonly ILogger<SpfSchedulerHandler> _log;

        public SpfSchedulerHandler(ISpfSchedulerDao dao,
            IMessageDispatcher dispatcher,
            ISpfSchedulerConfig config,
            ILogger<SpfSchedulerHandler> log)
        {
            _dao = dao;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted schedule for SPF entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"Schedule already deleted for SPF entity with id: {domain}.");
            }
        }

        public async Task Handle(SpfEntityCreated message)
        {
            string domain = message.Id.ToLower();
            SpfSchedulerState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new SpfSchedulerState(domain);

                await _dao.Save(state);

                _dispatcher.Dispatch(state.ToSpfRecordExpiredMessage(), _config.PublisherConnectionString);

                _log.LogInformation($"New {nameof(SpfSchedulerState)} saved for {domain}");
            }
            else
            {
                _log.LogInformation($"{nameof(SpfSchedulerState)} already exists for {domain}");
            }
        }
    }
}

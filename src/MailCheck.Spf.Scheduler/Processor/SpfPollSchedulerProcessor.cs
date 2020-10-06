using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MailCheck.Spf.Scheduler.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Scheduler.Processor
{
    public class SpfPollSchedulerProcessor : IProcess
    {
        private readonly ISpfPeriodicSchedulerDao _dao;
        private readonly IMessagePublisher _publisher;
        private readonly ISpfPeriodicSchedulerConfig _config;
        private readonly ILogger<SpfPollSchedulerProcessor> _log;

        public SpfPollSchedulerProcessor(ISpfPeriodicSchedulerDao dao,
            IMessagePublisher publisher,
            ISpfPeriodicSchedulerConfig config,
            ILogger<SpfPollSchedulerProcessor> log)
        {
            _dao = dao;
            _publisher = publisher;
            _config = config;
            _log = log;
        }

        public async Task<ProcessResult> Process()
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            List<SpfSchedulerState> expiredRecords = await _dao.GetExpiredSpfRecords();

            _log.LogInformation($"Found {expiredRecords.Count} expired records.");

            if (expiredRecords.Any())
            {
                expiredRecords
                    .Select(_ => _publisher.Publish(_.ToSpfRecordExpiredMessage(), _config.PublisherConnectionString))
                    .Batch(10)
                    .ToList()
                    .ForEach(async _ => await Task.WhenAll(_));

                await _dao.UpdateLastChecked(expiredRecords);

                _log.LogInformation($"Processing for domains {string.Join(',', expiredRecords.Select(_ => _.Id))} took {stopwatch.Elapsed}.");
            }

            stopwatch.Stop();

            return expiredRecords.Any()
                ? ProcessResult.Continue
                : ProcessResult.Stop;
        }
    }
}

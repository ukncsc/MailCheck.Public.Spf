using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Mapping;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Poller
{
    public class PollHandler : IHandle<SpfPollPending>
    {
        private readonly ISpfProcessor _processor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfPollerConfig _config;
        private readonly ILogger<PollHandler> _log;

        public PollHandler(ISpfProcessor processor, 
            IMessageDispatcher dispatcher, 
            ISpfPollerConfig config,
            ILogger<PollHandler> log)
        {
            _processor = processor;
            _dispatcher = dispatcher;
            _config = config;
            _log = log;
        }

        public async Task Handle(SpfPollPending message)
        {
            SpfPollResult spfPollResult = await _processor.Process(message.Id);

            _log.LogInformation("Polled SPF records for {Domain}", message.Id);

            SpfRecordsPolled spfRecordsPolled = spfPollResult.ToSpfRecordsPolled(message.Id);

            _dispatcher.Dispatch(spfRecordsPolled, _config.SnsTopicArn);

            _log.LogInformation("Published SPF records for {Domain}", message.Id);
        }
    }
}

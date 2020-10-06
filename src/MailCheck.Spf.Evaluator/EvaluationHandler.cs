using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Evaluator.Config;

namespace MailCheck.Spf.Evaluator
{
    public class EvaluationHandler : IHandle<SpfRecordsPolled>
    {
        private readonly ISpfEvaluationProcessor _spfEvaluationProcessor;
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEvaluatorConfig _config;

        public EvaluationHandler(ISpfEvaluationProcessor spfEvaluationProcessor,
            IMessageDispatcher dispatcher,
            ISpfEvaluatorConfig config)
        {
            _spfEvaluationProcessor = spfEvaluationProcessor;
            _dispatcher = dispatcher;
            _config = config;
        }

        public async Task Handle(SpfRecordsPolled message)
        {
            await _spfEvaluationProcessor.Process(message.Records);

            _dispatcher.Dispatch(
                new SpfRecordsEvaluated(message.Id, message.Records, message.DnsQueryCount, message.ElapsedQueryTime,
                    message.Messages, message.Timestamp), _config.SnsTopicArn);
        }
    }
}

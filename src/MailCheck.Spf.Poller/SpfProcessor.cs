using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Exception;
using MailCheck.Spf.Poller.Expansion;
using MailCheck.Spf.Poller.Parsing;
using MailCheck.Spf.Poller.Rules;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Poller
{
    public interface ISpfProcessor
    {
        Task<SpfPollResult> Process(string domain);
    }

    public class SpfProcessor : ISpfProcessor
    {
        private readonly IDnsClient _dnsClient;
        private readonly ISpfRecordsParser _spfRecordsParser;
        private readonly ISpfRecordExpander _spfRecordExpander;
        private readonly IEvaluator<SpfPollResult> _pollResultRulesEvaluator;
        private readonly ILogger<SpfProcessor> _log;

        public SpfProcessor(IDnsClient dnsClient,
            ISpfRecordsParser spfRecordsParser,
            ISpfRecordExpander spfRecordExpander,
             IEvaluator<SpfPollResult> pollResultRulesEvaluator, 
            ISpfPollerConfig config,
            ILogger<SpfProcessor> log)
        {
            _dnsClient = dnsClient;
            _spfRecordsParser = spfRecordsParser;
            _spfRecordExpander = spfRecordExpander;
            _pollResultRulesEvaluator = pollResultRulesEvaluator;
            _log = log;
        }

        public Guid Id => Guid.Parse("76390C8C-12D5-47FF-981E-D880D2B77216");

        public async Task<SpfPollResult> Process(string domain)
        {
            Stopwatch stopwatch = Stopwatch.StartNew();

            DnsResult<List<List<string>>> spfDnsRecords = await _dnsClient.GetSpfRecords(domain);
            
            if (spfDnsRecords.IsErrored)
            {
                string message = string.Format(SpfProcessorResource.FailedSpfRecordQueryErrorMessage, domain, spfDnsRecords.Error);
                string markdown = string.Format(SpfProcessorMarkdownResource.FailedSpfRecordQueryErrorMessage, domain, spfDnsRecords.Error);
                
                _log.LogError($"{message} {Environment.NewLine} Audit Trail: {spfDnsRecords.AuditTrail}");
                
                return new SpfPollResult(new Error(Id, "mailcheck.spf.failedSpfRecordQuery", ErrorType.Error, message, markdown));
            }

            if (spfDnsRecords.Value.Count == 0 ||
                spfDnsRecords.Value.TrueForAll(x =>  x.TrueForAll(string.IsNullOrWhiteSpace)))
            {
                _log.LogInformation(
                    $"SPF records missing or empty for {domain}, Name server: {spfDnsRecords.NameServer}");
            }

            SpfRecords root = await _spfRecordsParser.Parse(domain, spfDnsRecords.Value, spfDnsRecords.MessageSize);

            int lookupCount = await _spfRecordExpander.ExpandSpfRecords(domain, root);

            SpfPollResult pollResult = new SpfPollResult(root, lookupCount, stopwatch.Elapsed);

            EvaluationResult<SpfPollResult> errors = await _pollResultRulesEvaluator.Evaluate(pollResult);

            pollResult.Errors.AddRange(errors.Errors);

            return pollResult;
        }
    }
}

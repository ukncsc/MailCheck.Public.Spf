using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Rules;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface ISpfRecordsParser
    {
        Task<SpfRecords> Parse(string domain, List<List<string>> spfRecordsStrings, int messageSize);
    }

    public class SpfRecordsParser : ISpfRecordsParser
    {
        private readonly ISpfRecordParser _spfRecordParser;
        private readonly IEvaluator<DomainSpfRecords> _spfRecordsRulesEvaluator;

        public SpfRecordsParser(ISpfRecordParser spfRecordParser,
            IEvaluator<DomainSpfRecords> spfRecordsRulesEvaluator)
        {
            _spfRecordParser = spfRecordParser;
            _spfRecordsRulesEvaluator = spfRecordsRulesEvaluator;
        }

        public async Task<SpfRecords> Parse(string domain, List<List<string>> spfRecordsStrings, int messageSize)
        {
            SpfRecord[] records = await Task.WhenAll(spfRecordsStrings.Select(_ => _spfRecordParser.Parse(domain, _)));

            SpfRecords spfRecords = new SpfRecords(records.Where(_ => _ != null).ToList(), messageSize);

            EvaluationResult<DomainSpfRecords> results = 
                await _spfRecordsRulesEvaluator.Evaluate(new DomainSpfRecords(domain, spfRecords));

            spfRecords.AddErrors(results.Errors);

            return spfRecords;
        }
    }
}

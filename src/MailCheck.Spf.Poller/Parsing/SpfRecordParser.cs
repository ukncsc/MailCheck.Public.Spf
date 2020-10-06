using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Implicit;
using MailCheck.Spf.Poller.Rules;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface ISpfRecordParser
    {
        Task<SpfRecord> Parse(string domain, List<string> spfRecordParts);
    }

    public class SpfRecordParser : ISpfRecordParser
    {
        private const string Separator = " ";
        private readonly ISpfVersionParser _versionParser;
        private readonly ITermParser _termParser;
        private readonly IEvaluator<DomainSpfRecord> _spfRecordRulesEvaluator;
        private readonly IImplicitProvider<Term> _implicitTermProvider;

        public SpfRecordParser(
            ISpfVersionParser versionParser,
            ITermParser termParser,
            IEvaluator<DomainSpfRecord> spfRecordRulesEvaluator,
            IImplicitProvider<Term> implicitTermProvider)
        {
            _versionParser = versionParser;
            _termParser = termParser;
            _spfRecordRulesEvaluator = spfRecordRulesEvaluator;
            _implicitTermProvider = implicitTermProvider;
        }

        public async Task<SpfRecord> Parse(string domain, List<string> spfRecordParts)
        {
            string record = string.Join(string.Empty, spfRecordParts);

            if (string.IsNullOrEmpty(record))
            {
                return null;
            }

            string[] stringTokens = record.Split(Separator, StringSplitOptions.RemoveEmptyEntries);

            string versionToken = stringTokens.ElementAtOrDefault(0);

            Domain.Version version = _versionParser.Parse(versionToken);

            List<Term> terms = stringTokens.Skip(1).Select(_termParser.Parse).ToList();
            terms = terms.Concat(_implicitTermProvider.GetImplicitValues(terms)).ToList();

            SpfRecord spfRecord = new SpfRecord(spfRecordParts, version, terms);
            EvaluationResult<DomainSpfRecord> result = await _spfRecordRulesEvaluator.Evaluate(new DomainSpfRecord(domain, spfRecord));
            spfRecord.AddErrors(result.Errors);

            return spfRecord;
        }
    }
}

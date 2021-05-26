using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;

namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfIncludeTermExpander : ISpfTermExpanderStrategy
    {
        private readonly IDnsClient _dnsClient;
        private readonly ISpfRecordsParser _recordsParser;
        private readonly SpfMacroExpander _macroExpander = new SpfMacroExpander();

        public Guid Id => Guid.Parse("87147262-AA18-4068-978B-4EC2902CFB01");

        public SpfIncludeTermExpander(IDnsClient dnsClient, ISpfRecordsParser recordsParser)
        {
            _dnsClient = dnsClient;
            _recordsParser = recordsParser;
        }

        public async Task<SpfRecords> Process(string domain, Term term)
        {
            Include include = term as Include;
            string includeDomain = include.DomainSpec.Domain;

            if (_macroExpander.IsMacro(includeDomain))
            {
                return null;
            }

            DnsResult<List<List<string>>> spfDnsRecords = await _dnsClient.GetSpfRecords(includeDomain);

            if (spfDnsRecords.IsErrored)
            {
                string message = string.Format(SpfExpansionResource.FailedSpfRecordQueryErrorMessage, includeDomain, spfDnsRecords.Error);
                string markdown = string.Format(SpfExpansionMarkdownResource.FailedSpfRecordQueryErrorMessage, includeDomain, spfDnsRecords.Error);

                term.AddError(new Error(Id, ErrorType.Error, message, markdown));

                return null;
            }

            include.Records = await _recordsParser.Parse(includeDomain, spfDnsRecords.Value, spfDnsRecords.MessageSize);
           
            return include.Records;
        }

        public Type TermType => typeof(Include);
    }
}
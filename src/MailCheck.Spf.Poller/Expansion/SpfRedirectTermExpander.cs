﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Parsing;

namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfRedirectTermExpander : ISpfTermExpanderStrategy
    {
        public Guid Id => Guid.Parse("D847C5DF-37C6-47C3-B35E-72B6D5BB19CD");

        private readonly IDnsClient _dnsClient;
        private readonly ISpfRecordsParser _recordsParser;

        public SpfRedirectTermExpander(IDnsClient dnsClient, ISpfRecordsParser recordsParser)
        {
            _dnsClient = dnsClient;
            _recordsParser = recordsParser;
        }

        public async Task<SpfRecords> Process(string domain, Term term)
        {
            Redirect redirect = term as Redirect;

            DnsResult<List<List<string>>> spfDnsRecords = await _dnsClient.GetSpfRecords(redirect.DomainSpec.Domain);

            if (spfDnsRecords.IsErrored)
            {
                string message = string.Format(SpfExpansionResource.FailedSpfRecordQueryErrorMessage, redirect.DomainSpec.Domain, spfDnsRecords.Error);
                string markdown = string.Format(SpfExpansionMarkdownResource.FailedSpfRecordQueryErrorMessage, redirect.DomainSpec.Domain, spfDnsRecords.Error);

                term.AddError(new Error(Id, ErrorType.Error, message, markdown));

                return null;
            }

            redirect.Records = await _recordsParser.Parse(redirect.DomainSpec.Domain, spfDnsRecords.Value, spfDnsRecords.MessageSize);

            return redirect.Records;
        }

        public Type TermType => typeof(Redirect);
    }
}
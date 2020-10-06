using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfATermExpander : ISpfTermExpanderStrategy
    {
        private readonly IDnsClient _dnsClient;

        public Guid Id => Guid.Parse("9DCF1968-3D23-4D47-A7E8-B3032A2CC42D");

        public SpfATermExpander(IDnsClient dnsClient)
        {
            _dnsClient = dnsClient;
        }

        public async Task<SpfRecords> Process(string domain, Term term)
        {
            A a = term as A;

            string aDomain = string.IsNullOrEmpty(a.DomainSpec.Domain)
                ? domain
                : a.DomainSpec.Domain;

            DnsResult<List<string>> ips = await _dnsClient.GetARecords(aDomain);

            if (ips.IsErrored)
            {
                string message = string.Format(SpfExpansionResource.FailedARecordQueryErrorMessage, aDomain, ips.Error);
                string markdown = string.Format(SpfExpansionMarkdownResource.FailedARecordQueryErrorMessage, aDomain, ips.Error);

                a.AddError(new Error(Id, ErrorType.Error, message, markdown));
            }
            else
            {
                a.Ip4s = ips.Value;
            }

            return null;
        }

        public Type TermType => typeof(A);
    }
}
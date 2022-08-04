using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfMxTermExpander : ISpfTermExpanderStrategy
    {
        private readonly IDnsClient _dnsClient;
        private readonly SpfMacroExpander _macroExpander = new SpfMacroExpander();

        public SpfMxTermExpander(IDnsClient dnsClient)
        {
            _dnsClient = dnsClient;
        }

        public async Task<SpfRecords> Process(string domain, Term term)
        {
            Mx mx = term as Mx;

            string mxDomain = string.IsNullOrEmpty(mx.DomainSpec.Domain)
                ? domain
                : mx.DomainSpec.Domain;

            if (_macroExpander.IsMacro(mxDomain))
            {
                return null;
            }

            DnsResult<List<string>> mxRecords = await _dnsClient.GetMxRecords(mxDomain);

            if (mxRecords.IsErrored)
            {
                string message = string.Format(SpfExpansionResource.FailedMxRecordQueryErrorMessage, mxDomain, mxRecords.Error);
                string markdown = string.Format(SpfExpansionMarkdownResource.FailedMxRecordQueryErrorMessage, mxDomain, mxRecords.Error);
                Guid id = Guid.Parse("464F1D16-3945-41D3-9C02-D7B781BCB363");

                mx.AddError(new Error(id, "mailcheck.spf.failedMxRecordQuery", ErrorType.Error, message, markdown));
            }
            else
            {
                List<MxHost> mxHosts = new List<MxHost>();
                foreach (var mxRecord in mxRecords.Value)
                {
                    DnsResult<List<string>> ips = await _dnsClient.GetARecords(mxRecord);

                    if (ips.IsErrored)
                    {
                        mxHosts.Add(new MxHost(mxRecord, new List<string>()));

                        string message = string.Format(SpfExpansionResource.FailedARecordQueryErrorMessage, mxRecord, ips.Error);
                        string markdown = string.Format(SpfExpansionMarkdownResource.FailedARecordQueryErrorMessage, mxRecord, ips.Error);
                        Guid id = Guid.Parse("DA9C6FF2-5DD0-4AA0-BE66-2E443C93C9A2");

                        mx.AddError(new Error(id, "mailcheck.spf.failedARecordQuery", ErrorType.Error, message, markdown));
                    }
                    else if (ips.Value.Count > 10)
                    {
                        mxHosts.Add(new MxHost(mxRecord, new List<string>()));

                        string message = string.Format(SpfExpansionResource.TooManyARecordsErrorMessage, ips.Value.Count, mxRecord);
                        string markdown = string.Format(SpfExpansionMarkdownResource.TooManyARecordsErrorMessage, ips.Value.Count, mxRecord);
                        Guid id = Guid.Parse("6ABDDBDE-8147-49C4-A6A2-23DCA683DFDA");

                        mx.AddError(new Error(id, "mailcheck.spf.tooManyARecords", ErrorType.Error, message, markdown));
                    }
                    else
                    {
                        mxHosts.Add(new MxHost(mxRecord,ips.Value));
                    }
                }

                mx.MxHosts = mxHosts;
            }

            return null;
        }

        public Type TermType => typeof(Mx);
    }
}
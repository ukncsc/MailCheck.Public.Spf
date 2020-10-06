using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using MailCheck.Common.Util;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Poller.Dns
{
    public interface IDnsClient
    {
        Task<DnsResult<List<List<string>>>> GetSpfRecords(string domain);
        Task<DnsResult<List<string>>> GetMxRecords(string domain);
        Task<DnsResult<List<string>>> GetARecords(string host);
    }

    public class DnsClient : IDnsClient
    {
        private readonly ILookupClient _lookupClient;
        private readonly ILogger<IDnsClient> _log;

        public DnsClient(ILookupClient lookupClient,ILogger<IDnsClient> log)
        {
            _lookupClient = lookupClient;
            _log = log;
        }

        public async Task<DnsResult<List<List<string>>>> GetSpfRecords(string domain)
        {
            IDnsQueryResponse response = await _lookupClient.QueryAsync(domain, QueryType.TXT);

            if (response.HasError)
            {
                return new DnsResult<List<List<string>>>(response.ErrorMessage);
            }

            return new DnsResult<List<List<string>>>(
                response.Answers
                    .OfType<TxtRecord>()
                    .Where(_ => _.Text.FirstOrDefault()?.StartsWith("v=spf", StringComparison.OrdinalIgnoreCase) ??
                                false)
                    .Select(_ => _.Text.Select(r => r.Escape()).ToList())
                    .ToList(), response.MessageSize);
        }

        public async Task<DnsResult<List<string>>> GetMxRecords(string domain)
        {
            IDnsQueryResponse response = await _lookupClient.QueryAsync(domain, QueryType.MX);

            if (response.HasError)
            {
                return new DnsResult<List<string>>(response.ErrorMessage);
            }

            return new DnsResult<List<string>>(
                response.Answers
                    .OfType<MxRecord>()
                    .Select(_ => _.Exchange.Value.Escape())
                    .ToList(), response.MessageSize);
        }

        public async Task<DnsResult<List<string>>> GetARecords(string host)
        {
            IDnsQueryResponse response = await _lookupClient.QueryAsync(host, QueryType.A);

            if (response.HasError)
            {
                return new DnsResult<List<string>>(response.ErrorMessage);
            }

            return new DnsResult<List<string>>(
                response.Answers
                    .OfType<ARecord>()
                    .Select(_ => _.Address.ToString().Escape())
                    .ToList(), response.MessageSize);
        }
    }
}

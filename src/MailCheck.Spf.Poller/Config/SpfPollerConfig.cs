using System;
using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Poller.Config
{
    public interface ISpfPollerConfig
    {
        string SnsTopicArn { get; }
        TimeSpan DnsRecordLookupTimeout { get; }
        int MaxDnsQueryCount { get; }
        string NameServer { get; }
        bool AllowNullResults { get; }
    }

    public class SpfPollerConfig : ISpfPollerConfig
    {
        public SpfPollerConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            DnsRecordLookupTimeout = TimeSpan.FromSeconds(environmentVariables.GetAsLong("DnsRecordLookupTimeoutSeconds"));
            MaxDnsQueryCount = 20;
            NameServer = environmentVariables.Get("NameServer", false);
            AllowNullResults = environmentVariables.GetAsBoolOrDefault("AllowNullResults");
        }

        public string SnsTopicArn { get; }
        public TimeSpan DnsRecordLookupTimeout { get; }
        public int MaxDnsQueryCount { get; }
        public string NameServer { get; }
        public bool AllowNullResults { get; }
    }
}

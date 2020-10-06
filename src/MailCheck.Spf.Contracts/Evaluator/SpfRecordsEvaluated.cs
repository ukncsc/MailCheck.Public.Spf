using System;
using System.Collections.Generic;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Contracts.Evaluator
{
    public class SpfRecordsEvaluated : Common.Messaging.Abstractions.Message
    {
        public SpfRecordsEvaluated(string id,
            SpfRecords records,
            int? dnsQueryCount,
            TimeSpan? elapsedQueryTime,
            List<Message> messages,
            DateTime lastUpdated)
            : base(id)
        {
            Records = records;
            DnsQueryCount = dnsQueryCount;
            ElapsedQueryTime = elapsedQueryTime;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public SpfRecords Records { get; }

        public int? DnsQueryCount { get; }

        public TimeSpan? ElapsedQueryTime { get; }

        public List<Message> Messages { get; }

        public DateTime LastUpdated { get; }

        public SpfState State => SpfState.Evaluated;
    }
}

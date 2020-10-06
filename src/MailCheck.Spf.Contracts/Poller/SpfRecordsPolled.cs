using System;
using System.Collections.Generic;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Contracts.Poller
{
    public class SpfRecordsPolled : Common.Messaging.Abstractions.Message
    {
        public SpfRecordsPolled(string id,
            SpfRecords records,
            int? dnsQueryCount,
            TimeSpan? elapsedQueryTime,
            List<Message> messages = null) 
            : base(id)
        {
            Records = records;
            DnsQueryCount = dnsQueryCount;
            ElapsedQueryTime = elapsedQueryTime;
            Messages = messages ?? new List<Message>();
        }

        public SpfRecords Records { get; }
        public int? DnsQueryCount { get; }
        public TimeSpan? ElapsedQueryTime { get; }
        public List<Message> Messages { get; }
    }
}
using System;
using System.Collections.Generic;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain;
using Evnt = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Entity.Entity
{
    public class SpfEntityState
    {
        public SpfEntityState(string id, int version, SpfState spfState, DateTime created)
        {
            Id = id;
            Version = version;
            SpfState = spfState;
            Created = created;
            Messages = new List<Message>();
        }

        public virtual string Id { get; }

        public virtual int Version { get; set; }

        public virtual SpfState SpfState { get; set; }

        public virtual DateTime Created { get; }

        public virtual SpfRecords SpfRecords { get; set; }

        public virtual int? DnsQueryCount { get; set; }

        public virtual TimeSpan? ElapsedQueryTime { get; set; }

        public virtual List<Message> Messages { get; set; }

        public virtual DateTime? LastUpdated { get; set; }

        public Evnt UpdatePollPending()
        {
            SpfState = SpfState.PollPending;

            return new SpfPollPending(Id);
        }

        public Evnt UpdateSpfEvaluation(SpfRecords spfRecords, int? dnsQueryCount, TimeSpan? elapsedQueryTime,
            List<Message> messages, DateTime lastUpdated)
        {
            SpfRecords = spfRecords;
            DnsQueryCount = dnsQueryCount;
            ElapsedQueryTime = elapsedQueryTime;
            LastUpdated = lastUpdated;
            Messages = messages;
            LastUpdated = lastUpdated;
            SpfState = SpfState.Evaluated;

            return new SpfRecordEvaluationsChanged(Id, spfRecords);
        }
    }
}
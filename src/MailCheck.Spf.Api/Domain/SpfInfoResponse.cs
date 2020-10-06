using System;
using System.Collections.Generic;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfInfoResponse
    {
        public SpfInfoResponse(string id, SpfState spfState, SpfRecords spfRecords = null, List<Message> messages = null, DateTime? lastUpdated = null)
        {
            Id = id;
            Status = spfState;
            SpfRecords = spfRecords;
            Messages = messages;
            LastUpdated = lastUpdated;
        }

        public string Id { get; }

        public SpfState Status { get; }

        public SpfRecords SpfRecords { get; }

        public List<Message> Messages { get; }

        public DateTime? LastUpdated { get; }
    }
}

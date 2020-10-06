using System;

namespace MailCheck.Spf.Contracts.Entity
{
    public class LastUpdatedChanged : Common.Messaging.Abstractions.Message
    {
        public LastUpdatedChanged(string id, DateTime lastUpdated) 
            : base(id)
        {
            LastUpdated = lastUpdated;
        }

        public DateTime LastUpdated { get; }

        public SpfState State => SpfState.Unchanged;
    }
}
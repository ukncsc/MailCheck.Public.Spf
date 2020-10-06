using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Spf.Contracts.Entity
{
    public class SpfEntityCreated : VersionedMessage
    {
        public SpfEntityCreated(string id, int version) 
            : base(id, version)
        {
        }

        public SpfState State => SpfState.Created;
    }
}

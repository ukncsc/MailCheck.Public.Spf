using MailCheck.Common.Messaging.Abstractions;

namespace MailCheck.Spf.Contracts.Scheduler
{
    public class SpfRecordExpired : Message
    {
        public SpfRecordExpired(string id)
            : base(id) { }
    }
}

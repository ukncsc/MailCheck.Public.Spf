using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Contracts.Entity
{
    public class SpfRecordEvaluationsChanged : Common.Messaging.Abstractions.Message
    {
        public SpfRecordEvaluationsChanged(string id, SpfRecords records) : base(id)
        {
            Records = records;
        }

        public SpfRecords Records { get; }

        public SpfState State => SpfState.Evaluated;
    }
}
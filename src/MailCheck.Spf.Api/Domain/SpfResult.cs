using System.Collections.Generic;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfResult
    {
        public SpfResult(List<SpfRecord> records, List<Message> messages)
        {
            Records = records;
            Messages = messages;
        }

        public List<SpfRecord> Records { get; }

        public List<Message> Messages { get; }
    }
}

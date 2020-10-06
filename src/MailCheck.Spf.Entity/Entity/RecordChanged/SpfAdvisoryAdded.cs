using MailCheck.Common.Messaging.Abstractions;
using System.Collections.Generic;

namespace MailCheck.Spf.Entity.Entity.RecordChanged
{
    public class SpfAdvisoryAdded : Message
    {
        public SpfAdvisoryAdded(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}

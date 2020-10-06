using MailCheck.Common.Messaging.Abstractions;
using System.Collections.Generic;

namespace MailCheck.Spf.Entity.Entity.RecordChanged
{
    public class SpfReferencedAdvisoryAdded : Message
    {
        public SpfReferencedAdvisoryAdded(string id, List<AdvisoryMessage> messages) : base(id)
        {
            Messages = messages;
        }

        public List<AdvisoryMessage> Messages { get; }
    }
}

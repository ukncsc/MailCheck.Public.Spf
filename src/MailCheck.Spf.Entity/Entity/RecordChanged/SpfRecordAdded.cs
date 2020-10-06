using MailCheck.Common.Messaging.Abstractions;
using System.Collections.Generic;

namespace MailCheck.Spf.Entity.Entity.RecordChanged
{
    public class SpfRecordAdded : Message
    {
        public SpfRecordAdded(string id, List<string> records) : base(id)
        {
            Records = records;
        }

        public List<string> Records { get; }
    }
}
 
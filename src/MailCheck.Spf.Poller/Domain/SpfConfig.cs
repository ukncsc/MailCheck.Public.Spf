using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class SpfRecords : SpfEntity
    {
        public SpfRecords(List<SpfRecord> records, int messageSize)
        {
            Records = records;
            MessageSize = messageSize;
        }

        public List<SpfRecord> Records { get; }

        public int MessageSize { get; }

        public override int AllErrorCount => Records.Sum(_ => _.AllErrorCount) + ErrorCount;

        public override IReadOnlyList<Error> AllErrors => Records.SelectMany(_ => _.AllErrors).Concat(Errors).ToArray();
    }
}
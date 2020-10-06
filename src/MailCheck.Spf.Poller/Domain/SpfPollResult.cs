using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class SpfPollResult
    {
        public SpfPollResult(params Error[] errors)
            :this(null, null, null, errors.ToList())
        {}

        public SpfPollResult(SpfRecords records, int queryCount, TimeSpan elapsed)
            : this(records, queryCount, elapsed, null)
        {}

        private SpfPollResult(SpfRecords records, int? queryCount, TimeSpan? elapsed, List<Error> errors)
        {
            Records = records ?? new SpfRecords(new List<SpfRecord>(), 0);
            QueryCount = queryCount;
            Elapsed = elapsed;
            Errors = errors ?? new List<Error>();
        }

        public SpfRecords Records { get; }
        public int? QueryCount { get; }
        public TimeSpan? Elapsed { get; }
        public List<Error> Errors { get; }
    }
}
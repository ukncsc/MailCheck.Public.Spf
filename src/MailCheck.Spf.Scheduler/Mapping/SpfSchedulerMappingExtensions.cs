using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Scheduler.Dao.Model;

namespace MailCheck.Spf.Scheduler.Mapping
{
    public static class SpfSchedulerMappingExtensions
    {
        public static SpfRecordExpired ToSpfRecordExpiredMessage(this SpfSchedulerState state) =>
            new SpfRecordExpired(state.Id);
    }
}

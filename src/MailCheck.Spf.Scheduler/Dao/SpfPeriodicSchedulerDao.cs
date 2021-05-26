using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao.Model;
using MailCheck.Common.Util;

namespace MailCheck.Spf.Scheduler.Dao
{
    public interface ISpfPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<SpfSchedulerState> entitiesToUpdate);
        Task<List<SpfSchedulerState>> GetExpiredSpfRecords();
    }
    
    public class SpfPeriodicSchedulerDao : ISpfPeriodicSchedulerDao
    {
        private readonly IDatabase _database;
        private readonly IClock _clock;
        private readonly ISpfPeriodicSchedulerConfig _config;

        public SpfPeriodicSchedulerDao(IDatabase database,
            ISpfPeriodicSchedulerConfig config, IClock clock)
        {
            _database = database;
            _config = config;
            _clock = clock;
        }

        public async Task UpdateLastChecked(List<SpfSchedulerState> entitiesToUpdate)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var parameters = entitiesToUpdate
                    .Select(ent => new {id = ent.Id, lastChecked = GetAdjustedLastCheckedTime()}).ToArray();
                await connection.ExecuteAsync(SpfPeriodicSchedulerDaoResources.UpdateSpfRecordsLastCheckedDistributed,
                    parameters);
            }
        }

        public async Task<List<SpfSchedulerState>> GetExpiredSpfRecords()
        {
            DateTime nowMinusInterval = _clock.GetDateTimeUtc().AddSeconds(-_config.RefreshIntervalSeconds);
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                var records = (await connection.QueryAsync<string>(SpfPeriodicSchedulerDaoResources.SelectSpfRecordsToSchedule,
                    new {now_minus_interval = nowMinusInterval, limit = _config.DomainBatchSize})).ToList();

                return records.Select(record => new SpfSchedulerState(record)).ToList();
            }
        }

        private DateTime GetAdjustedLastCheckedTime()
        {
            return _clock.GetDateTimeUtc().AddSeconds(-(new Random().NextDouble() * 3600));
        }
    }
}

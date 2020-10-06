using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Scheduler.Dao
{
    public interface ISpfPeriodicSchedulerDao
    {
        Task UpdateLastChecked(List<SpfSchedulerState> entitiesToUpdate);
        Task<List<SpfSchedulerState>> GetExpiredSpfRecords();
    }

    public class SpfPeriodicSchedulerDao : ISpfPeriodicSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly ISpfPeriodicSchedulerConfig _config;

        public SpfPeriodicSchedulerDao(IConnectionInfoAsync connectionInfo, ISpfPeriodicSchedulerConfig config)
        {
            _connectionInfo = connectionInfo;
            _config = config;
        }

        public async Task<List<SpfSchedulerState>> GetExpiredSpfRecords()
        {
            MySqlParameter[] parameters = {
                new MySqlParameter("refreshIntervalSeconds", _config.RefreshIntervalSeconds),
                new MySqlParameter("limit", _config.DomainBatchSize)
            };

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            List<SpfSchedulerState> states = new List<SpfSchedulerState>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                SpfPeriodicSchedulerDaoResources.SelectSpfRecordsToSchedule, parameters))
            {
                while (await reader.ReadAsync())
                {
                    states.Add(CreateSpfSchedulerState(reader));
                }
            }

            return states;
        }

        public async Task UpdateLastChecked(List<SpfSchedulerState> entitiesToUpdate)
        {
            string query = string.Format(SpfPeriodicSchedulerDaoResources.UpdateSpfRecordsLastChecked,
                string.Join(',', entitiesToUpdate.Select((_, i) => $"@domainName{i}")));

            MySqlParameter[] parameters = entitiesToUpdate
                .Select((_, i) => new MySqlParameter($"domainName{i}", _.Id.ToLower()))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString, query, parameters);
        }

        private SpfSchedulerState CreateSpfSchedulerState(DbDataReader reader)
        {
            return new SpfSchedulerState(reader.GetString("id"));
        }
    }
}

using System;
using System.Threading.Tasks;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Spf.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Scheduler.Dao
{
    public interface ISpfSchedulerDao
    {
        Task<SpfSchedulerState> Get(string domain);
        Task Save(SpfSchedulerState state);
        Task Delete(string domain);
    }

    public class SpfSchedulerDao : ISpfSchedulerDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;

        public SpfSchedulerDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<SpfSchedulerState> Get(string domain)
        {
            string id = (string)await MySqlHelper.ExecuteScalarAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                SpfSchedulerDaoResources.SelectSpfRecord,
                new MySqlParameter("id", domain));

            return id == null
                ? null
                : new SpfSchedulerState(id);
        }

        public async Task Save(SpfSchedulerState state)
        {
            int numberOfRowsAffected = await MySqlHelper.ExecuteNonQueryAsync(
                await _connectionInfo.GetConnectionStringAsync(),
                SpfSchedulerDaoResources.InsertSpfRecord,
                new MySqlParameter("id", state.Id.ToLower()));

            if (numberOfRowsAffected == 0)
            {
                throw new InvalidOperationException($"Didn't save duplicate {nameof(SpfSchedulerState)} for {state.Id}");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SpfSchedulerDaoResources.DeleteSpfRecord,
                new MySqlParameter("id", domain));
        }
    }
}

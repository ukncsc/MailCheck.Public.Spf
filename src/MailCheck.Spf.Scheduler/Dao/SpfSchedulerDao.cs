using System;
using System.Threading.Tasks;
using Dapper;
using MailCheck.Common.Data;
using MailCheck.Spf.Scheduler.Dao.Model;

namespace MailCheck.Spf.Scheduler.Dao
{
    public interface ISpfSchedulerDao
    {
        Task<SpfSchedulerState> Get(string domain);
        Task Save(SpfSchedulerState state);
        Task<int> Delete(string domain);
    }

    public class SpfSchedulerDao : ISpfSchedulerDao
    {
        private readonly IDatabase _database;

        public SpfSchedulerDao(IDatabase database)
        {
            _database = database;
        }

        public async Task<SpfSchedulerState> Get(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                string id = await connection.QueryFirstOrDefaultAsync<string>(
                    SpfSchedulerDaoResources.SelectSpfRecord, new { id = domain });
                
                return id == null
                    ? null
                    : new SpfSchedulerState(id);
            }
        }

        public async Task Save(SpfSchedulerState state)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                int numberOfRowsAffected = await connection.ExecuteAsync(SpfSchedulerDaoResources.InsertSpfRecord,
                    new {id = state.Id.ToLower()});
                
                if (numberOfRowsAffected == 0)
                {
                    throw new InvalidOperationException($"Didn't save duplicate {nameof(SpfSchedulerState)} for {state.Id}");
                }
            }
        }

        public async Task<int> Delete(string domain)
        {
            using (var connection = await _database.CreateAndOpenConnectionAsync())
            {
                return await connection.ExecuteAsync(SpfSchedulerDaoResources.DeleteSpfRecord,
                    new { id = domain });
            }
        }
    }
}

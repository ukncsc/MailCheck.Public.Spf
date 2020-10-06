using System;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Spf.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Entity.Dao
{
    public interface ISpfEntityDao
    {
        Task<SpfEntityState> Get(string domain);
        Task Save(SpfEntityState state);
        Task Delete(string domain);
    }

    public class SpfEntityDao : ISpfEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public SpfEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<SpfEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, SpfEntityDaoResouces.SelectSpfEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<SpfEntityState>(state);
        }

        public async Task Save(SpfEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state);

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString, SpfEntityDaoResouces.InsertSpfEntity,
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", serializedState));

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Didn't update SpfEntityState because version {state.Version} has already been persisted.");
            }
        }

        public async Task Delete(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SpfEntityDaoResouces.DeleteSpfEntity,
                new MySqlParameter("domain", domain));
        }
    }
}

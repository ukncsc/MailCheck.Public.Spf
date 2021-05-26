using System;
using System.Diagnostics;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Spf.Entity.Entity;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Entity.Dao
{
    public interface ISpfEntityDao
    {
        Task<SpfEntityState> Get(string domain);
        Task Save(SpfEntityState state);
        Task<int> Delete(string domain);
    }

    public class SpfEntityDao : ISpfEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;
        private readonly ILogger<SpfEntityDao> _logger;

        public SpfEntityDao(IConnectionInfoAsync connectionInfoAsync, ILogger<SpfEntityDao> logger)
        {
            _connectionInfoAsync = connectionInfoAsync;
            _logger = logger;
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
            _logger.LogInformation($"Starting Save of SpfEntityState for {state.Id}");

            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();
            
            Stopwatch sw = Stopwatch.StartNew();
            
            string serializedState = JsonConvert.SerializeObject(state);

            _logger.LogInformation($"Serialized SpfEntityState for {state.Id} in {sw.ElapsedMilliseconds}ms, size: {serializedState.Length}");
            sw.Restart();

            int rowsAffected = await MySqlHelper.ExecuteNonQueryAsync(connectionString, SpfEntityDaoResouces.InsertSpfEntity,
                new MySqlParameter("domain", state.Id.ToLower()),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", serializedState));

            _logger.LogInformation($"Saved SpfEntityState for {state.Id} in {sw.ElapsedMilliseconds}ms, size: {serializedState.Length}");
            sw.Stop();

            if (rowsAffected == 0)
            {
                throw new InvalidOperationException(
                    $"Didn't update SpfEntityState because version {state.Version} has already been persisted.");
            }
        }

        public async Task<int> Delete(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();
            
            return await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SpfEntityDaoResouces.DeleteSpfEntity,
                new MySqlParameter("domain", domain));
        }
    }
}

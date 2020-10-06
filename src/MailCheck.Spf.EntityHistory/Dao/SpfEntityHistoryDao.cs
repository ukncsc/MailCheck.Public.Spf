using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Spf.EntityHistory.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.EntityHistory.Dao
{
    public interface ISpfHistoryEntityDao
    {
        Task<SpfHistoryEntityState> Get(string domain);
        Task Save(SpfHistoryEntityState state);
    }

    public class SpfHistoryEntityDao : ISpfHistoryEntityDao
    {
        private readonly IConnectionInfoAsync _connectionInfoAsync;

        public SpfHistoryEntityDao(IConnectionInfoAsync connectionInfoAsync)
        {
            _connectionInfoAsync = connectionInfoAsync;
        }

        public async Task<SpfHistoryEntityState> Get(string domain)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string state = (string)await MySqlHelper.ExecuteScalarAsync(connectionString, SpfEntityHistoryDaoResouces.SelectSpfHistoryEntity,
                new MySqlParameter("domain", domain));

            return state == null
                ? null
                : JsonConvert.DeserializeObject<SpfHistoryEntityState>(state);
        }

        public async Task Save(SpfHistoryEntityState state)
        {
            string connectionString = await _connectionInfoAsync.GetConnectionStringAsync();

            string serializedState = JsonConvert.SerializeObject(state, new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                SpfEntityHistoryDaoResouces.InsertSpfEntityHistory,
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", serializedState));
        }
    }
}

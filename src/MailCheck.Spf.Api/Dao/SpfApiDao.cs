using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Spf.Api.Domain;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Api.Dao
{
    public interface ISpfApiDao
    {
        Task<List<SpfInfoResponse>> GetSpfForDomains(List<string> domains);
        Task<SpfInfoResponse> GetSpfForDomain(string domain);
        Task<string> GetSpfHistory(string domain);
    }

    public class SpfApiDao : ISpfApiDao
    {
        private readonly IConnectionInfoAsync _connectionInfo;
        private readonly JsonSerializerSettings _serializerSettings = new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public SpfApiDao(IConnectionInfoAsync connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<SpfInfoResponse>> GetSpfForDomains(List<string> domain)
        {
            string query = string.Format(SpfApiDaoResources.SelectSpfStates,
                string.Join(',', domain.Select((_, i) => $"@domain{i}")));

            MySqlParameter[] parameters = domain
                .Select((_, i) => new MySqlParameter($"domain{i}", _))
                .ToArray();

            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString, query, parameters))
            {
                List<SpfInfoResponse> states = new List<SpfInfoResponse>();

                while (await reader.ReadAsync())
                {
                    if (!reader.IsDbNull("state"))
                    {
                        states.Add(JsonConvert.DeserializeObject<SpfInfoResponse>(reader.GetString("state")));
                    }
                }

                return states;
            }
        }

        public async Task<SpfInfoResponse> GetSpfForDomain(string domain)
        {
            List<SpfInfoResponse> responses = await GetSpfForDomains(new List<string>{domain});
            return responses.FirstOrDefault();
        }

        public async Task<string> GetSpfHistory(string domain)
        {
            string connectionString = await _connectionInfo.GetConnectionStringAsync();

            return (string) await MySqlHelper.ExecuteScalarAsync(connectionString, 
                SpfApiDaoResources.SelectSpfHistoryStates, new MySqlParameter("domain", domain));
        }
    }
}
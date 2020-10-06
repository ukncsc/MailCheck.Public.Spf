using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Entity.Seeding.History
{
    public interface IHistoryReaderDao
    {
        Task<List<HistoryItem>> GetHistory();
    }

    public class HistoryReaderDao : IHistoryReaderDao
    {
        private readonly IConnectionInfo _connectionInfo;

        public HistoryReaderDao(IConnectionInfo connectionInfo)
        {
            _connectionInfo = connectionInfo;
        }

        public async Task<List<HistoryItem>> GetHistory()
        {
            string command =
                "SELECT name as \'entity_id\', GROUP_CONCAT(record) as records, start_date\nFROM domain d\nLEFT JOIN dns_record_spf spf ON spf.domain_id = d.id\nWHERE (d.monitor = b\'1\' OR d.publish = b\'1\')\nAND spf.end_date IS NOT NULL\nGROUP BY name, start_date\nORDER BY name, start_date;";

            List<HistoryItem> historyItems = new List<HistoryItem>();
            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(_connectionInfo.ConnectionString, command))
            {
                while (await reader.ReadAsync())
                {
                    historyItems.Add(new HistoryItem(
                        reader.GetString("entity_id"),
                        reader.GetString("records")?.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList(),
                        reader.GetDateTime("start_date")));
                }
            }

            return historyItems;
        }
    }
}

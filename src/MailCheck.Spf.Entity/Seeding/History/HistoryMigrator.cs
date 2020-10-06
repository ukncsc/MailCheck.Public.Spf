using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Util;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Entity;

namespace MailCheck.Spf.Entity.Seeding.History
{
    public interface IHistoryMigrator
    {
        Task Migrate();
    }

    public class HistoryMigrator : IHistoryMigrator
    {
        private readonly IHistoryReaderDao _historyReaderDao;
        private readonly IHistoryWriterDao _historyWriterDao;

        public HistoryMigrator(IHistoryReaderDao historyReaderDao, IHistoryWriterDao historyWriterDao)
        {
            _historyReaderDao = historyReaderDao;
            _historyWriterDao = historyWriterDao;
        }

        public async Task Migrate()
        {
            List<HistoryItem> historyItems = await _historyReaderDao.GetHistory();

            List<SpfEntityState> states = historyItems
                .GroupBy(_ => _.Id)
                .Select(CreateSpfEntityState)
                .SelectMany(_ => _)
                .OrderBy(_ => _.LastUpdated)
                .ToList();

            IEnumerable<IEnumerable<SpfEntityState>> batches = states.Batch(500);

            foreach (IEnumerable<SpfEntityState> batch in batches)
            {
                await _historyWriterDao.WriteHistory(batch.ToList());
            }
        }

        private IEnumerable<SpfEntityState> CreateSpfEntityState(IGrouping<string, HistoryItem> domainHistory)
        {
            HistoryItem first = domainHistory.First();

            foreach (HistoryItem history in domainHistory)
            {
                yield return new SpfEntityState(domainHistory.Key, 0, SpfState.Evaluated, first.StartDate)
                {
                    LastUpdated = history.StartDate,
                    DnsQueryCount = 0,
                    ElapsedQueryTime = TimeSpan.MinValue,
                    Messages = new List<Message>(),
                    SpfRecords = new SpfRecords(history.Records.Select(CreateSpfRecord).ToList(), 0, new List<Message>())
                };
            }
        }

        private SpfRecord CreateSpfRecord(string recordString)
        {
            return new SpfRecord(new List<string>{recordString},null, null, null, true);
        }
    }
}

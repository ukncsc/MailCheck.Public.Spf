using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.EntityHistory.Dao;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.EntityHistory.Entity
{
    public class SpfEntityHistory : IHandle<DomainCreated>,
        IHandle<SpfRecordsPolled>
    {
        private readonly ISpfHistoryEntityDao _dao;
        private readonly ILogger<SpfEntityHistory> _log;

        public SpfEntityHistory(
            ILogger<SpfEntityHistory> log,
            ISpfHistoryEntityDao dao)
        {
            _dao = dao;
            _log = log;
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            SpfHistoryEntityState state = await _dao.Get(domain);

            if (state == null)
            {
                state = new SpfHistoryEntityState(domain);
                await _dao.Save(state);
                _log.LogInformation($"Created SpfEntityHistory for {domain}.");
            }
            else
            {
                _log.LogInformation($"Ignoring {nameof(DomainCreated)} as SpfEntityHistory already exists for {domain}.");
            }
        }

        public async Task Handle(SpfRecordsPolled message)
        {
            string messageId = message.Id.ToLower();

            SpfHistoryEntityState entityHistory = await LoadHistoryState(messageId);

            List<string> records = new List<string>();

            message.Records?.Records.ForEach(x => records.AddRange(x.RecordsStrings));

            if (entityHistory.UpdateHistory(records, message.Timestamp))
            {
                await _dao.Save(entityHistory);
            }
        }

        private async Task<SpfHistoryEntityState> LoadHistoryState(string id)
        {
            SpfHistoryEntityState entityHistoryState =
                await _dao.Get(id) ?? new SpfHistoryEntityState(id);

            return entityHistoryState;
        }
    }
}
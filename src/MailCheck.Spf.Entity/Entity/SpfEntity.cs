﻿using System;
using System.Threading.Tasks;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Dao;
using MailCheck.Spf.Entity.Entity.DomainStatus;
using MailCheck.Spf.Entity.Entity.Notifiers;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Entity.Entity
{
    public class SpfEntity :
        IHandle<DomainCreated>,
        IHandle<DomainDeleted>,
        IHandle<SpfRecordExpired>,
        IHandle<SpfRecordsEvaluated>
    {
        private readonly ISpfEntityDao _dao;
        private readonly ILogger<SpfEntity> _log;
        private readonly ISpfEntityConfig _spfEntityConfig;
        private readonly IChangeNotifiersComposite _changeNotifierComposite;
        private readonly IMessageDispatcher _dispatcher;
        private readonly IDomainStatusPublisher _domainStatusPublisher;

        public SpfEntity(ISpfEntityDao dao,
            ISpfEntityConfig spfEntityConfig,
            IChangeNotifiersComposite changeNotifierComposite,
            ILogger<SpfEntity> log,
            IMessageDispatcher dispatcher, 
            IDomainStatusPublisher domainStatusPublisher)
        {
            _dao = dao;
            _log = log;
            _spfEntityConfig = spfEntityConfig;
            _changeNotifierComposite = changeNotifierComposite;
            _dispatcher = dispatcher;
            _domainStatusPublisher = domainStatusPublisher;
        }

        public async Task Handle(DomainDeleted message)
        {
            string domain = message.Id.ToLower();
            int rows = await _dao.Delete(domain);
            if (rows == 1)
            {
                _log.LogInformation($"Deleted SPF entity with id: {domain}.");
            }
            else
            {
                _log.LogInformation($"SPF entity already deleted with id: {domain}.");
            }
        }

        public async Task Handle(DomainCreated message)
        {
            string domain = message.Id.ToLower();

            SpfEntityState state = await _dao.Get(domain);

            if (state != null)
            {
                _log.LogInformation($"Ignoring {nameof(DomainCreated)} as SpfEntity already exists for {domain}.");
            }
            else
            {
                state = new SpfEntityState(domain, 1, SpfState.Created, DateTime.UtcNow);
                await _dao.Save(state);
                _log.LogInformation($"Created SpfEntity for {domain}.");
            }
            
            SpfEntityCreated spfEntityCreated = new SpfEntityCreated(domain, state.Version);
            _dispatcher.Dispatch(spfEntityCreated, _spfEntityConfig.SnsTopicArn);
            _log.LogInformation($"Dispatching SpfEntityCreated for {domain}.");
        }

        public async Task Handle(SpfRecordExpired message)
        {
            string id = message.Id.ToLower();

            SpfEntityState state = await LoadState(id, nameof(message));

            Message evnt = state.UpdatePollPending();

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(evnt, _spfEntityConfig.SnsTopicArn);
        }

        public async Task Handle(SpfRecordsEvaluated message)
        {
            string id = message.Id.ToLower();

            SpfEntityState state = await LoadState(id, nameof(message));

            _changeNotifierComposite.Handle(state, message);

            _domainStatusPublisher.Publish(message);

            Message evnt = state.UpdateSpfEvaluation(message.Records, message.DnsQueryCount, message.ElapsedQueryTime,
                message.Messages, message.LastUpdated);

            state.Version++;

            await _dao.Save(state);

            _dispatcher.Dispatch(evnt, _spfEntityConfig.SnsTopicArn);
        }

        private async Task<SpfEntityState> LoadState(string id, string messageType)
        {
            SpfEntityState state = await _dao.Get(id);

            if (state == null)
            {
                _log.LogError("Ignoring {EventName} as SPF Entity does not exist for {Id}.", messageType, id);
                throw new InvalidOperationException(
                    $"Cannot handle event {messageType} as SPF Entity doesnt exists for {id}.");
            }

            return state;
        }
    }
}
﻿using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class ReferencedRecordMessagesChangeNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEntityConfig _spfEntityConfig;
        private readonly IEqualityComparer<Message> _messageEqualityComparer;

        public ReferencedRecordMessagesChangeNotifier(IMessageDispatcher dispatcher, ISpfEntityConfig spfEntityConfig, IEqualityComparer<Message> messageEqualityComparer)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
        }

        public async void Handle(SpfEntityState state, Common.Messaging.Abstractions.Message message)
        {
            if (message is SpfRecordsEvaluated evaluated)
            {
                List<Message> currentRecordsMessages = await Process(state.SpfRecords);
                List<Message> newRecordsMessages = await Process(evaluated.Records);

                List<Message> removedMessages = currentRecordsMessages.Except(newRecordsMessages, _messageEqualityComparer).ToList();
                List<Message> addedMessages = newRecordsMessages.Except(currentRecordsMessages, _messageEqualityComparer).ToList();
                List<Message> sustainedMessages = currentRecordsMessages.Intersect(newRecordsMessages, _messageEqualityComparer).ToList();

                if (addedMessages.Any())
                {
                    _dispatcher.Dispatch(new SpfReferencedAdvisoryAdded(state.Id, addedMessages.Select(x => new AdvisoryMessage(x.MessageType, x.Text)).ToList()), _spfEntityConfig.SnsTopicArn);
                }

                if (removedMessages.Any())
                {
                    _dispatcher.Dispatch(new SpfReferencedAdvisoryRemoved(state.Id, removedMessages.Select(x => new AdvisoryMessage(x.MessageType, x.Text)).ToList()), _spfEntityConfig.SnsTopicArn);
                }

                if (sustainedMessages.Any())
                {
                    _dispatcher.Dispatch(new SpfReferencedAdvisorySustained(state.Id, sustainedMessages.Select(x => new AdvisoryMessage(x.MessageType, x.Text)).ToList()), _spfEntityConfig.SnsTopicArn);
                }
            }
        }
        
        private async Task<List<Message>> Process(SpfRecords spfRecordRoot)
        {
            List<Message> allMessages = new List<Message>();

            Task AddToList(SpfRecords records)
            {
                if (spfRecordRoot != null)
                {
                    foreach (SpfRecord spfRecord in records.Records)
                    {
                        if (!spfRecordRoot.Records.Contains(spfRecord))
                        {
                            allMessages.AddRange(records.Messages);
                            allMessages.AddRange(spfRecord.Messages);
                        }
                    }
                }

                return Task.CompletedTask;
            }

            SpfRecordsDepthFirstJobProcessor processor = new SpfRecordsDepthFirstJobProcessor();
            await processor.Process(spfRecordRoot, AddToList);

            return allMessages;
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class RecordMessagesChangeNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEntityConfig _spfEntityConfig;
        private readonly IEqualityComparer<Message> _messageEqualityComparer;

        public RecordMessagesChangeNotifier(IMessageDispatcher dispatcher, ISpfEntityConfig spfEntityConfig, IEqualityComparer<Message> messageEqualityComparer)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
            _messageEqualityComparer = messageEqualityComparer;
        }

        public void Handle(SpfEntityState state, Common.Messaging.Abstractions.Message message)
        {
            if (message is SpfRecordsEvaluated evaluated)
            {
                List<Message> currentRecordsMessages = new List<Message>();

                if (state.SpfRecords != null)
                {
                    currentRecordsMessages.AddRange(state.SpfRecords.Messages);
                    currentRecordsMessages.AddRange(state.SpfRecords.Records.SelectMany(x => x.Messages).ToList());
                }

                currentRecordsMessages.AddRange(state.Messages);

                List<Message> newRecordsMessages = new List<Message>();

                if (evaluated.Records != null)
                {
                    newRecordsMessages.AddRange(evaluated.Records.Messages);
                    newRecordsMessages.AddRange(evaluated.Records.Records.SelectMany(x => x.Messages).ToList());
                }

                newRecordsMessages.AddRange(evaluated.Messages);

                List<Message> removedMessages = currentRecordsMessages.Except(newRecordsMessages, _messageEqualityComparer).ToList();
                List<Message> addedMessages = newRecordsMessages.Except(currentRecordsMessages, _messageEqualityComparer).ToList();

                if (addedMessages.Any())
                {
                    _dispatcher.Dispatch(
                        new SpfAdvisoryAdded(state.Id,
                            addedMessages.Select(x => new AdvisoryMessage(x.MessageType, x.Text)).ToList()),
                        _spfEntityConfig.SnsTopicArn);
                }

                if (removedMessages.Any())
                {
                    _dispatcher.Dispatch(
                        new SpfAdvisoryRemoved(state.Id,
                            removedMessages.Select(x => new AdvisoryMessage(x.MessageType, x.Text)).ToList()),
                        _spfEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}
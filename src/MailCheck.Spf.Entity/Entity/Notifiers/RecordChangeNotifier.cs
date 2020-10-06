using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class RecordChangeNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEntityConfig _spfEntityConfig;

        public RecordChangeNotifier(IMessageDispatcher dispatcher, ISpfEntityConfig spfEntityConfig)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
        }

        public void Handle(SpfEntityState state, Message message)
        {
            if (message is SpfRecordsEvaluated evaluated)
            {
                List<string> currentRecords = state.SpfRecords?.Records.SelectMany(x => x.RecordsStrings).ToList() ?? new List<string>();
                List<string> newRecords = evaluated.Records?.Records.SelectMany(x => x.RecordsStrings).ToList() ?? new List<string>();

                List<string> removedRecords = currentRecords.Except(newRecords).ToList();
                List<string> addedRecords = newRecords.Except(currentRecords).ToList();

                if (addedRecords.Any())
                {
                    _dispatcher.Dispatch(new SpfRecordAdded(state.Id, addedRecords), _spfEntityConfig.SnsTopicArn);
                }

                if (removedRecords.Any())
                {
                    _dispatcher.Dispatch(new SpfRecordRemoved(state.Id, removedRecords), _spfEntityConfig.SnsTopicArn);
                }
            }
        }
    }
}
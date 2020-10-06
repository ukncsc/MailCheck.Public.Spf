using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.RecordChanged;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class ReferencedRecordChangeNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEntityConfig _spfEntityConfig;

        public ReferencedRecordChangeNotifier(IMessageDispatcher dispatcher, ISpfEntityConfig spfEntityConfig)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
        }

        public async void Handle(SpfEntityState state, Message message)
        {
            if (message is SpfRecordsEvaluated evaluated)
            {
                List<SpfRecord> currentRecords = await Process(state.SpfRecords);
                List<SpfRecord> newRecords = await Process(evaluated.Records);

                if (!currentRecords.CollectionEqual(newRecords, new SpfRecordsReferencedEqualityComparer()))
                {
                    List<string> currentReferencedRecords = currentRecords.SelectMany(x => x.RecordsStrings).ToList();
                    List<string> newReferencedRecords = newRecords.SelectMany(x => x.RecordsStrings).ToList();

                    List<string> removedRecords = currentReferencedRecords.Except(newReferencedRecords).ToList();
                    List<string> addedRecords = newReferencedRecords.Except(currentReferencedRecords).ToList();

                    if (addedRecords.Any())
                    {
                        _dispatcher.Dispatch(new SpfReferencedRecordAdded(state.Id, addedRecords), _spfEntityConfig.SnsTopicArn);
                    }

                    if (removedRecords.Any())
                    {
                        _dispatcher.Dispatch(new SpfReferencedRecordRemoved(state.Id, removedRecords), _spfEntityConfig.SnsTopicArn);
                    }
                }
            }
        }

        private async Task<List<SpfRecord>> Process(SpfRecords spfRecordRoot)
        {
            List<SpfRecord> allSpfRecords = new List<SpfRecord>();

            Task AddToList(SpfRecords records)
            {
                if (records != null)
                {
                    foreach (SpfRecord spfRecord in records.Records)
                    {
                        if (!spfRecordRoot.Records.Contains(spfRecord))
                        {
                            allSpfRecords.Add(spfRecord);
                        }
                    }
                }

                return Task.CompletedTask;
            }

            SpfRecordsDepthFirstJobProcessor processor = new SpfRecordsDepthFirstJobProcessor();
            await processor.Process(spfRecordRoot, AddToList);

            return allSpfRecords;
        }
    }
}
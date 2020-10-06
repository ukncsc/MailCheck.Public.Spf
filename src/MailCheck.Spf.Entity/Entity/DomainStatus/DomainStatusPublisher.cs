using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.DomainStatus.Contracts;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using Microsoft.Extensions.Logging;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Entity.DomainStatus
{
    public interface IDomainStatusPublisher
    {
        void Publish(SpfRecordsEvaluated message);
    }

    public class DomainStatusPublisher : IDomainStatusPublisher
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly ISpfEntityConfig _spfEntityConfig;
        private readonly IDomainStatusEvaluator _domainStatusEvaluator;
        private readonly ILogger<DomainStatusPublisher> _log;

        public DomainStatusPublisher(IMessageDispatcher dispatcher, ISpfEntityConfig spfEntityConfig, IDomainStatusEvaluator domainStatusEvaluator, ILogger<DomainStatusPublisher> log)
        {
            _dispatcher = dispatcher;
            _spfEntityConfig = spfEntityConfig;
            _domainStatusEvaluator = domainStatusEvaluator;
            _log = log;
        }

        public void Publish(SpfRecordsEvaluated message)
        {
            List<Message> rootMessages = message.Messages;
            List<Message> recordsMessages = message.Records?.Messages;
            IEnumerable<Message> recordsRecordsMessages = message.Records?.Records?.SelectMany(x => x.Messages);
            List<SpfRecord> recordsRecords = message.Records?.Records;
            List<Message> recursiveMessages = new List<Message>();

            if (recordsRecords != null && recordsRecords.Count > 0)
            {
                foreach (SpfRecord record in recordsRecords)
                {
                    IEnumerable<Message> recordMessages = GetMessages(record);
                    recursiveMessages.AddRange(recordMessages);
                }
            }
            
            IEnumerable<Message> messages = (rootMessages ?? new List<Message>())
                .Concat(recordsMessages ?? new List<Message>())
                .Concat(recordsRecordsMessages ?? new List<Message>())
                .Concat(recursiveMessages ?? new List<Message>());

            Status status = _domainStatusEvaluator.GetStatus(messages.ToList());

            DomainStatusEvaluation domainStatusEvaluation = new DomainStatusEvaluation(message.Id, "SPF", status);

            _log.LogInformation($"Publishing SPF domain status for {message.Id} of {status}");

            _dispatcher.Dispatch(domainStatusEvaluation, _spfEntityConfig.SnsTopicArn);
        }

        private IEnumerable<Message> GetMessages(SpfRecord record)
        {
            List<Message> messages = new List<Message>();
            if (record != null)
            {
                foreach (Term term in record?.Terms)
                {
                    if (term is Include include)
                    {
                        if (include.Records?.Messages != null)
                        {
                            messages.AddRange(include.Records?.Messages);
                        }

                        List<SpfRecord> childRecords = include.Records?.Records;
                        List<Message> childMessages = new List<Message>();

                        if (childRecords != null && childRecords.Count > 0)
                        {
                            foreach (SpfRecord childRecord in childRecords)
                            {
                                IEnumerable<Message> childRecordMessages = GetMessages(childRecord);
                                if (childRecordMessages != null)
                                {
                                    childMessages.AddRange(childRecordMessages);
                                }
                            }
                        }
                        messages.AddRange(childMessages);
                    }
                }
            }
            
            return messages;
        }
    }
}

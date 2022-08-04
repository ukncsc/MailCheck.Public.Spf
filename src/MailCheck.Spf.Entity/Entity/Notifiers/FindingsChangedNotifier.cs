using System.Collections.Generic;
using System.Linq;
using MailCheck.Common.Contracts.Findings;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Processors.Notifiers;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using ErrorMessage = MailCheck.Spf.Contracts.SharedDomain.Message;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class FindingsChangedNotifier : IChangeNotifier
    {
        private readonly IMessageDispatcher _dispatcher;
        private readonly IFindingsChangedNotifier _findingsChangedNotifier;
        private readonly ISpfEntityConfig _spfEntityConfig;

        public FindingsChangedNotifier(
            IMessageDispatcher dispatcher,
            IFindingsChangedNotifier findingsChangedNotifier,
            ISpfEntityConfig spfEntityConfig)
        {
            _dispatcher = dispatcher;
            _findingsChangedNotifier = findingsChangedNotifier;
            _spfEntityConfig = spfEntityConfig;
        }

        public void Handle(SpfEntityState state, Message message)
        {
            string messageId = state.Id.ToLower();

            if (message is SpfRecordsEvaluated evaluationResult)
            {
                FindingsChanged findingsChanged = _findingsChangedNotifier.Process(messageId, "SPF",
                    ExtractFindingsFromState(messageId, state),
                    ExtractFindingsFromResult(messageId, evaluationResult));
                _dispatcher.Dispatch(findingsChanged, _spfEntityConfig.SnsTopicArn);
            }
        }

        private IList<Finding> ExtractFindingsFromState(string domain, SpfEntityState state)
        {
            var rootMessages = state.Messages ?? new List<ErrorMessage>();
            var recordsMessages = state.SpfRecords?.Messages?.ToList() ?? new List<ErrorMessage>();
            var recordsRecords = state.SpfRecords?.Records;

            return ExtractFindingsFromMessages(domain, rootMessages, recordsMessages, recordsRecords);
        }

        private IList<Finding> ExtractFindingsFromResult(string domain, SpfRecordsEvaluated result)
        {
            var rootMessages = result.Messages ?? new List<ErrorMessage>();
            var recordsMessages = result.Records?.Messages?.ToList() ?? new List<ErrorMessage>();
            var recordsRecords = result.Records?.Records;

            return ExtractFindingsFromMessages(domain, rootMessages, recordsMessages, recordsRecords);
        }

        private List<Finding> ExtractFindingsFromMessages(string domain, List<ErrorMessage> rootMessages, List<ErrorMessage> recordsMessages, List<SpfRecord> recordsRecords)
        {
            List<ErrorMessage> messages = new List<ErrorMessage>();

            messages.AddRange(rootMessages);
            messages.AddRange(recordsMessages);

            if (recordsRecords != null)
            {
                messages.AddRange(recordsRecords.SelectMany(x => x.Messages).ToList());
            }

            List<Finding> findings = messages.Select(msg => new Finding
            {
                Name = msg.Name,
                SourceUrl = $"https://{_spfEntityConfig.WebUrl}/app/domain-security/{domain}/spf",
                Title = msg.Text,
                EntityUri = $"domain:{domain}",
                Severity = AdvisoryMessageTypeToFindingSeverityMapping[msg.MessageType]
            }).ToList();

            return findings;
        }

        internal static readonly Dictionary<MessageType, string> AdvisoryMessageTypeToFindingSeverityMapping = new Dictionary<MessageType, string>
        {
            [MessageType.info] = "Informational",
            [MessageType.warning] = "Advisory",
            [MessageType.error] = "Urgent",
            [MessageType.positive] = "Positive",
        };
    }
}
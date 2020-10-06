using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Evaluator.Explainers;

namespace MailCheck.Spf.Evaluator.Rules
{
    public class ShouldHaveHardFailAllEnabled : IRule<SpfRecord>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public ShouldHaveHardFailAllEnabled(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public Guid Id => Guid.Parse("36B13D1B-F3EB-4C1A-AF67-60B798B82E71");

        public Task<List<Message>> Evaluate(SpfRecord record)
        {
            All all = record.Terms.OfType<All>().FirstOrDefault();

            List<Message> messages = new List<Message>();

            if (record.IsRoot &&
                all != null && 
                (all.Qualifier == Qualifier.Pass ||
                all.Qualifier == Qualifier.Neutral ||
                all.Qualifier == Qualifier.Unknown))
            {
                string failExplanation = _qualifierExplainer.Explain(Qualifier.Fail, true);
                string softFailExplanation = _qualifierExplainer.Explain(Qualifier.SoftFail, true);
                string allExplanation = _qualifierExplainer.Explain(all.Qualifier, true);

                string errorMessage = string.Format(SpfRulesResource.ShouldHaveHardFailAllEnabledErrorMessage, failExplanation, softFailExplanation, all.Value, allExplanation);
                string markDown = string.Format(SpfRulesMarkDownResource.ShouldHaveHardFailAllEnabledErrorMessage, failExplanation, softFailExplanation, all.Value, allExplanation);

                messages.Add(new Message(Id, MessageSources.SpfEvaluator, MessageType.warning, errorMessage, markDown));
            }

            return Task.FromResult(messages);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}
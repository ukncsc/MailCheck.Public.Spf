using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class IncludeTermExplainer : BaseTermExplainerStrategy<Include>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public IncludeTermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(Include tConcrete)
        {
            return string.Format(SpfExplainerResource.IncludeExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier), tConcrete.Domain);
        }
    }
}
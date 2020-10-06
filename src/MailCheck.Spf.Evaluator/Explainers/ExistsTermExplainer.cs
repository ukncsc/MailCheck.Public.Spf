using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class ExistsTermExplainer : BaseTermExplainerStrategy<Exists>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public ExistsTermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(Exists tConcrete)
        {
            return string.Format(SpfExplainerResource.ExistsExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier), tConcrete.Domain);
        }
    }
}
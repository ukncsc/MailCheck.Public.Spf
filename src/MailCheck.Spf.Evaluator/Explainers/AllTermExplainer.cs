using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class AllTermExplainer : BaseTermExplainerStrategy<All>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public AllTermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(All tConcrete)
        {
            return string.Format(SpfExplainerResource.AllExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier));
        }
    }
}
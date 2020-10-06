using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class ATermExplainer : BaseTermExplainerStrategy<A>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public ATermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(A tConcrete)
        {
            string domain = tConcrete.Domain ?? "this domain";

            return string.Format(SpfExplainerResource.AExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier), domain,
               tConcrete.Ip4Cidr?.ToString() ?? "invalid", tConcrete.Ip6Cidr?.ToString() ?? "invalid");
        }
    }
}
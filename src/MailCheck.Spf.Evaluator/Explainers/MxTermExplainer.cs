using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class MxTermExplainer : BaseTermExplainerStrategy<Mx>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public MxTermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(Mx tConcrete)
        {
            string domain = tConcrete?.Domain ?? "this domain";

            return string.Format(SpfExplainerResource.MxExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier), domain,
                tConcrete.Ip4Cidr?.ToString() ?? "invalid", tConcrete.Ip6Cidr?.ToString() ?? "invalid");
        }
    }
}
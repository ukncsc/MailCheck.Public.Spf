using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class Ip6TermExplainer : BaseTermExplainerStrategy<Ip6>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public Ip6TermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(Ip6 tConcrete)
        {
            return string.Format(SpfExplainerResource.IpExplanation, _qualifierExplainer.Explain(tConcrete.Qualifier),
                tConcrete.Ip, tConcrete.Ip6Cidr?.ToString() ?? "invalid");
        }
    }
}
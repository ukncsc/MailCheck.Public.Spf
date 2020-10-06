using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class PtrTermExplainer : BaseTermExplainerStrategy<Ptr>
    {
        private readonly IQualifierExplainer _qualifierExplainer;

        public PtrTermExplainer(IQualifierExplainer qualifierExplainer)
        {
            _qualifierExplainer = qualifierExplainer;
        }

        public override string GetExplanation(Ptr tConcrete)
        {
            string domain = tConcrete?.Domain ?? "this domain";

            return $"{_qualifierExplainer.Explain(tConcrete.Qualifier)} ip addresses mentioned in PTR record for {domain}.";
        }
    }
}
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class UnknownTermExplainer : BaseTermExplainerStrategy<UnknownTerm>
    {
        public override string GetExplanation(UnknownTerm tConcrete)
        {
            return string.Format(SpfExplainerResource.UnknownTermExplanation, tConcrete.Value);
        }
    }
}
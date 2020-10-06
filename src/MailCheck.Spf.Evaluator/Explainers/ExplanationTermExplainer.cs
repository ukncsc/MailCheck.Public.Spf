using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class ExplanationTermExplainer : BaseTermExplainerStrategy<Explanation>
    {
        public override string GetExplanation(Explanation tConcrete)
        {
            return string.Format(SpfExplainerResource.ExplanationExplanation, tConcrete.Domain);
        }
    }
}
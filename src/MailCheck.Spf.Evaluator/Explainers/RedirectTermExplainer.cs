using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class RedirectTermExplainer : BaseTermExplainerStrategy<Redirect>
    {
        public override string GetExplanation(Redirect tConcrete)
        {
            return $"SPF record for {tConcrete.Domain} replaces the SPF record for this domain.";
        }
    }
}
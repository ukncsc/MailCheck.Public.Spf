using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public class VersionExplainer : IExplainer<Version>
    {
        public bool TryExplain(Version t, out string explanation)
        {
            if (t.Valid)
            {
                explanation = SpfExplainerResource.VersionExplanation;
                return true;
            }

            explanation = null;
            return false;
        }
    }
}
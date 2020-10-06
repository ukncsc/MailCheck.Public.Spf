using MailCheck.Spf.Contracts.SharedDomain;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public interface ISpfRecordExplainer
    {
        void Explain(SpfRecord record);
    }

    public class SpfRecordExplainer : ISpfRecordExplainer
    {
        private readonly IExplainer<Version> _versionExplainer;
        private readonly IExplainer<Term> _termExplainer;

        public SpfRecordExplainer(IExplainer<Version> versionExplainer,
            IExplainer<Term> termExplainer)
        {
            _versionExplainer = versionExplainer;
            _termExplainer = termExplainer;
        }

        public void Explain(SpfRecord record)
        {
            if (_versionExplainer.TryExplain(record.Version, out string versionExplanation))
            {
                record.Version.Explanation = versionExplanation;
            }

            foreach (Term term in record.Terms)
            {
                if (_termExplainer.TryExplain(term, out string termExplanation))
                {
                    term.Explanation = termExplanation;
                }
            }
        }
    }
}

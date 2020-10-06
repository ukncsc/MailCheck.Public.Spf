using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public abstract class BaseTermExplainerStrategy<TConcrete> :  BaseExplainerStrategy<Term, TConcrete>
        where TConcrete : Term
    {
        public override bool TryExplain(Term t, out string explanation)
        {
            TConcrete concrete = ToTConcrete(t);

            if (concrete.Valid)
            {
                explanation = GetExplanation(concrete);
                return true;
            }
            explanation = null;
            return false;
        }

        public abstract string GetExplanation(TConcrete tConcrete);
    }
}

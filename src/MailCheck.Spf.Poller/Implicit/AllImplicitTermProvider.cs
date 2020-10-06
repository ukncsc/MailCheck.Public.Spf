using System.Collections.Generic;
using System.Linq;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Implicit
{
    public class AllImplicitTermProvider : ImplicitTermProviderStrategyBase<All>
    {
        public AllImplicitTermProvider()
            : base(GetImplicitAll)
        {
        }

        private static All GetImplicitAll(List<Term> terms)
        {
            return terms.OfType<Redirect>().Any()
                ? null
                : All.Default;
        }
    }
}

using System;
using System.Collections.Generic;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Implicit
{
    public abstract class ImplicitTermProviderStrategyBase<TConcrete>
        : ImplicitProviderStrategyBase<Term, TConcrete>
        where TConcrete : Term
    {
        protected ImplicitTermProviderStrategyBase(Func<List<Term>, TConcrete> defaultValueFactory)
            : base(defaultValueFactory)
        { }
    }
}
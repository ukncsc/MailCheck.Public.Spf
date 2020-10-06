namespace MailCheck.Spf.Contracts.SharedDomain
{
    public abstract class Modifier : Term
    {
        protected Modifier(TermType termType, string value, bool valid) 
            : base(termType, value, valid)
        {
        }
    }
}
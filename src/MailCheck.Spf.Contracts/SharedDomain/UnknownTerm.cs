namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class UnknownTerm : Term
    {
        public UnknownTerm(string value, bool valid) 
            : base(TermType.Unknown, value, valid)
        {
        }
    }
}
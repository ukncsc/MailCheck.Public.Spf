namespace MailCheck.Spf.Poller.Domain
{
    public abstract class Mechanism : Term
    {
        protected Mechanism(string value, Qualifier qualifier)
            : base(value)
        {
            Qualifier = qualifier;
        }

        public Qualifier Qualifier { get; }
    }
}
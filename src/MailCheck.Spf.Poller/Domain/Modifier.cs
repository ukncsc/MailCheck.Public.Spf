namespace MailCheck.Spf.Poller.Domain
{
    public abstract class Modifier : Term
    {
        protected Modifier(string value)
            : base(value){}
    }
}
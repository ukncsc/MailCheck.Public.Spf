namespace MailCheck.Spf.Poller.Domain
{
    public class All : OptionalDefaultMechanism
    {
        public static All Default = new All("?all", Qualifier.Neutral, true);

        public All(string value, Qualifier qualifier, bool isImplicit = false)
            : base(value, qualifier, isImplicit){}
    }
}
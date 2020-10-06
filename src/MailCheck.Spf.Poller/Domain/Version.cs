namespace MailCheck.Spf.Poller.Domain
{
    public class Version : SpfEntity
    {
        public Version(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return $"{nameof(Value)}: {Value}";
        }
    }
}
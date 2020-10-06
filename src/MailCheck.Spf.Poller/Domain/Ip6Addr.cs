namespace MailCheck.Spf.Poller.Domain
{
    public class Ip6Addr : SpfEntity
    {
        public Ip6Addr(string value)
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
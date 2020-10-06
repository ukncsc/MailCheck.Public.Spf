namespace MailCheck.Spf.Poller.Domain
{
    public class Ip4Addr : SpfEntity
    {
        public Ip4Addr(string value)
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
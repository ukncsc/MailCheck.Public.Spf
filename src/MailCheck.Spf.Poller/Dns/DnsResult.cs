namespace MailCheck.Spf.Poller.Dns
{
    public class DnsResult<T>
        where T : class
    {
        public DnsResult(T value, int messageSize)
            : this(value, null, null, null)
        {
            MessageSize = messageSize;
        }

        public DnsResult(T value, int messageSize, string nameServer, string auditTrail)
            : this(value, null, nameServer, auditTrail)
        {
            MessageSize = messageSize;
        }
        public DnsResult(string error)
            : this(error, null, null) { }

        public DnsResult(string error, string nameServer, string auditTrail)
            : this(null, error, nameServer, auditTrail) { }

        private DnsResult(T value, string error, string nameServer, string auditTrail)
        {
            Error = error;
            NameServer = nameServer;
            AuditTrail = auditTrail;
            Value = value;
        }

        public string NameServer { get; }

        public string AuditTrail { get; }

        public int MessageSize { get; }

        public string Error { get; }

        public bool IsErrored => Error != null;

        public T Value { get; }
    }
}
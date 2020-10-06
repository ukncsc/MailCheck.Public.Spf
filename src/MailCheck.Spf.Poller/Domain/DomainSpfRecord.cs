namespace MailCheck.Spf.Poller.Domain
{
    public class DomainSpfRecord
    {
        public DomainSpfRecord(string domain, SpfRecord record)
        {
            Domain = domain;
            Record = record;
        }

        public string Domain { get; }
        public SpfRecord Record { get; }
    }
}
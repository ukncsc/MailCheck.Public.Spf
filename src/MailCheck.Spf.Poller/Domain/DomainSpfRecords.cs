namespace MailCheck.Spf.Poller.Domain
{
    public class DomainSpfRecords
    {
        public DomainSpfRecords(string domain, SpfRecords spfRecords)
        {
            Domain = domain;
            SpfRecords = spfRecords;
        }

        public string Domain { get; }
        public SpfRecords SpfRecords { get; }
    }
}
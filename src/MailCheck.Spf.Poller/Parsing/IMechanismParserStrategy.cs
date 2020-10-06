using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IMechanismParserStrategy
    {
        string Mechanism { get; }
        Term Parse(string mechanism, Qualifier qualifier, string arguments);
    }
}
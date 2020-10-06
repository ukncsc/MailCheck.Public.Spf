using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IModifierParserStrategy
    {
        Term Parse(string modifier, string arguments);
        string Modifier { get; }
    }
}
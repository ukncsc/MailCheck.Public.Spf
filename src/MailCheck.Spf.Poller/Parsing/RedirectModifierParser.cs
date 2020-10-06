using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public class RedirectModifierParser : IModifierParserStrategy
    {
        private readonly IDomainSpecParser _domainSpecParser;

        public RedirectModifierParser(IDomainSpecParser domainSpecParser)
        {
            _domainSpecParser = domainSpecParser;
        }

        public Term Parse(string modifier, string arguments)
        {
            DomainSpec domainSpec = _domainSpecParser.Parse(arguments, true);

            return new Redirect(modifier, domainSpec);
        }

        public string Modifier => "redirect";
    }
}
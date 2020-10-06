using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public class PtrMechanismParser : IMechanismParserStrategy
    {
        private readonly IDomainSpecParser _domainSpecParser;

        public PtrMechanismParser(IDomainSpecParser domainSpecParser)
        {
            _domainSpecParser = domainSpecParser;
        }

        //"ptr" [ ":" domain-spec]
        public Term Parse(string mechanism, Qualifier qualifier, string arguments)
        {
            DomainSpec domainSpec = _domainSpecParser.Parse(arguments, false);

            return new Ptr(mechanism, qualifier, domainSpec);
        }

        public string Mechanism => "ptr";
    }
}
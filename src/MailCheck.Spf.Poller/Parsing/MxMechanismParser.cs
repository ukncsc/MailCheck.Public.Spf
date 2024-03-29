using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public class MxMechanismParser : IMechanismParserStrategy
    {
        private readonly IDomainSpecDualCidrBlockMechanismParser _mechanismParser;

        public MxMechanismParser(IDomainSpecDualCidrBlockMechanismParser mechanismParser)
        {
            _mechanismParser = mechanismParser;
        }

        // "mx" [ ":" domain-spec ] [ dual-cidr-length ]
        public Term Parse(string mechanism, Qualifier qualifier, string arguments)
        {
            return _mechanismParser.Parse(mechanism, qualifier, arguments, (s, q, ds, dc) => new Mx(s, q, ds, dc));
        }

        public string Mechanism => "mx";
    }
}
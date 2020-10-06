using System;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface ITermParser
    {
        Term Parse(string term);
    }

    public class TermParser : ITermParser
    {
        private readonly IMechanismParser _mechanismParser;
        private readonly IModifierParser _modifierParser;

        public Guid Id => Guid.Parse("A443B27F-64F6-452E-9B24-1607E5A214D4");

        public TermParser(IMechanismParser mechanismParser, 
            IModifierParser modifierParser)
        {
            _mechanismParser = mechanismParser;
            _modifierParser = modifierParser;
        }

        public Term Parse(string stringTerm)
        {
            Term term;
            if (_mechanismParser.TryParse(stringTerm, out term))
            {
                return term;
            }

            if(_modifierParser.TryParse(stringTerm, out term))
            {
                return term;
            }

            UnknownTerm unknownTerm = new UnknownTerm(stringTerm);
            string errorMessage = string.Format(SpfParserResource.UnknownTermErrorMessage, stringTerm);
            unknownTerm.AddError(new Error(Id, ErrorType.Error, errorMessage, SpfParserMarkdownResource.UnknownTermErrorMessage));
            return unknownTerm;
        }
    }
}
using System;
using System.Text.RegularExpressions;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IDomainSpecParser
    {
        DomainSpec Parse(string domainSpec, bool mandatory);
    }

    //Note this doesnt expand macros or perform dns look ups
    //
    public class DomainSpecParserPassive : IDomainSpecParser
    {
        //Regex taken from Eddy Minet's .Net implementation http://www.openspf.org/Implementations
        private readonly Regex _macroRegex = new Regex(@"%{1}\{(?<macro_letter>[slodipvhcrt])(?<digits>[0-9]*)(?<transformer>r)?(?<delimiter>[.+,\/_=-]?)\}", RegexOptions.IgnoreCase);
        
        //Credited to bkr : http://stackoverflow.com/questions/11809631/fully-qualified-domain-name-validation
        private readonly Regex _domainRegex = new Regex(@"(?=^.{4,253}$)(^((?!-)[a-zA-Z0-9-_]{1,63}(?<!-)\.)+[a-zA-Z]{2,63}\.?$)");

        public DomainSpec Parse(string domainSpecString, bool mandatory)
        {
            DomainSpec domainSpec = new DomainSpec(domainSpecString);
            if (string.IsNullOrEmpty(domainSpecString))
            {
                if (mandatory)
                {
                    string message = string.Format(SpfParserResource.NoValueErrorMessage, "domain");
                    string markdown = string.Format(SpfParserMarkdownResource.NoValueErrorMessage, "domain");
                    Guid id = Guid.Parse("5E60D21F-A543-4263-9525-FE6E2E330E52");

                    domainSpec.AddError(new Error(id, "mailcheck.spf.noValueDomainSpec", ErrorType.Error, message, markdown));
                }
            }
            else if (!_domainRegex.IsMatch(domainSpecString) && !_macroRegex.IsMatch(domainSpecString))
            {
                string message = string.Format(SpfParserResource.InvalidValueErrorMessage, "domain or macro", domainSpecString);
                string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "domain or macro", domainSpecString);
                Guid id = Guid.Parse("38A767C4-B795-4B57-8779-5D535B4347BB");

                domainSpec.AddError(new Error(id, "mailcheck.spf.invalidValueDomainSpec", ErrorType.Error, message, markdown));
            }

            return domainSpec;
        }
    }
}
using System;
using System.Text.RegularExpressions;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface ISpfVersionParser
    {
        Domain.Version Parse(string version);
    }

    public class SpfVersionParser : ISpfVersionParser
    {
        public Guid Id => Guid.Parse("D63E3BDC-DFFA-4DF6-929A-79385E4DB88E");

        private readonly Regex _regex = new Regex("^v=spf1$", RegexOptions.IgnoreCase);

        public Domain.Version Parse(string versionString)
        {
            Domain.Version version = new Domain.Version(versionString);
            if (versionString == null || !_regex.IsMatch(versionString))
            {
                string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "SPF version", version);
                string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "SPF version", version);

                version.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }
            return version;
        }
    }
}
using System;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public class AllMechanismParser : IMechanismParserStrategy
    {
        public Guid Id => Guid.Parse("CAEBF984-AC83-4B12-950B-40A98F0CF4C8");

        //"all"
        public Term Parse(string mechanism, Qualifier qualifier, string arguments)
        {
            All all = new All(mechanism, qualifier);

            if (!string.IsNullOrEmpty(arguments))
            {
                string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, Mechanism, mechanism);
                string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, Mechanism, mechanism);

                all.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return all;
        }

        public string Mechanism => "all";
    }
}
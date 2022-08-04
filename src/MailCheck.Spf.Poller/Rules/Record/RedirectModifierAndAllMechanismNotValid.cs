using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class RedirectModifierAndAllMechanismNotValid : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("46A853C6-4AC0-438E-A6CF-AEC96D7E692E");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecord)
        {
            List<Error> errors = new List<Error>();
            if (spfRecord.Record.Terms.OfType<All>().Any(_ => !_.IsImplicit) && spfRecord.Record.Terms.OfType<Redirect>().Any())
            {
                string errorMessage = string.Format(SpfRulesResource.RedirectModifierAndAllMechanismNotValidErrorMessage, spfRecord.Domain);
                string markdown = string.Format(SpfRulesMarkdownResource.RedirectModifierAndAllMechanismNotValidErrorMessage, spfRecord.Domain);

                errors.Add(new Error(Id, "mailcheck.spf.redirectModifierAndAllMechanismNotValid", ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 7;

        public bool IsStopRule => false;
    }
}
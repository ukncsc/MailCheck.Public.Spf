using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class AllMustBeLastMechanism : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("E6400237-EE85-4EAA-9F01-7CC738B3475B");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecords)
        {
            bool isRedirect = spfRecords.Record.Terms.OfType<Redirect>().Any();
            Mechanism lastMechanism = spfRecords.Record.Terms.OfType<Mechanism>().LastOrDefault();

            List<Error> errors = new List<Error>();

            if (!(isRedirect || lastMechanism == null || lastMechanism is All))
            {
                string errorMessage = string.Format(SpfRulesResource.AllMustBeLastMechanismErrorMessage, spfRecords.Domain, lastMechanism.Value);
                string markdown = string.Format(SpfRulesMarkdownResource.AllMustBeLastMechanismErrorMessage, spfRecords.Domain, lastMechanism.Value);

                errors.Add(new Error(Id, "mailcheck.spf.allMustBeLastMechanism", ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;

        public bool IsStopRule => false;
    }
}
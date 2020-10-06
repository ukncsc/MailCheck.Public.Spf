using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class RedirectDoesntOccurMoreThanOnce : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("079EBE0E-0B6F-4908-8EC2-7F0CB9FB295D");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecord)
        {
            int redirectCount = spfRecord.Record.Terms.OfType<Redirect>().Count();
            List<Error> errors = new List<Error>();
            if (redirectCount > 1)
            {
                string errorMessage = string.Format(SpfRulesResource.RedirectDoesntOccurMoreThanOnceErrorMessage, spfRecord.Domain, redirectCount);
                string markdown = string.Format(SpfRulesMarkdownResource.RedirectDoesntOccurMoreThanOnceErrorMessage, spfRecord.Domain, redirectCount);

                errors.Add(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 6;

        public bool IsStopRule => false;
    }
}
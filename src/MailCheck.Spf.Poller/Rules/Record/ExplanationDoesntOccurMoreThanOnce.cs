using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class ExplanationDoesntOccurMoreThanOnce : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("46469EC8-D5DA-41DA-A80D-13F26F52B0DA");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecord)
        {
            int explanationCount = spfRecord.Record.Terms.OfType<Explanation>().Count();

            List<Error> errors = new List<Error>();
            if (explanationCount > 1)
            {
                string errorMessage = string.Format(SpfRulesResource.ExplanationDoesntOccurMoreThanOnceErrorMessage, spfRecord.Domain, explanationCount);
                string markdown = string.Format(SpfRulesMarkdownResource.ExplanationDoesntOccurMoreThanOnceErrorMessage, spfRecord.Domain, explanationCount);

                errors.Add(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 3;

        public bool IsStopRule => false;
    }
}
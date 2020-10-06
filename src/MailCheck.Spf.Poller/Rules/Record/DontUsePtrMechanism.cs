using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class DontUsePtrMechanism : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("56FEA94F-8E4A-4F2F-9AA6-B2746F4C6097");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecords)
        {
            List<Error> errors = new List<Error>();

            if (spfRecords.Record.Terms.OfType<Ptr>().Any())
            {
                string errorMessage = string.Format(SpfRulesResource.DontUsePtrMechanismErrorMessage, spfRecords.Domain);
                string markdown = string.Format(SpfRulesMarkdownResource.DontUsePtrMechanismErrorMessage, spfRecords.Domain);

                errors.Add(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;

        public bool IsStopRule => false;
    }
}
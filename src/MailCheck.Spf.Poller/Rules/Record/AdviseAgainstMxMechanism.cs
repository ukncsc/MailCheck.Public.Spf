using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Record
{
    public class AdviseAgainstMxMechanism : IRule<DomainSpfRecord>
    {
        public Guid Id => Guid.Parse("cf85f547-8e87-4c34-a543-08b0a0201f0a");

        public Task<List<Error>> Evaluate(DomainSpfRecord spfRecords)
        {
            List<Error> errors = new List<Error>();

            if (spfRecords.Record.Terms.OfType<Mx>().Any())
            {
                string errorMessage = SpfRulesResource.ShouldNotUseMxInfoMessage;
                string markdown = SpfRulesMarkdownResource.ShouldNotUseMxInfoMessage;

                errors.Add(new Error(Id, ErrorType.Info, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;

        public bool IsStopRule => false;
    }
}
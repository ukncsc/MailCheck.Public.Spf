using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Records
{
    public class ShouldBeSmallEnoughForUdp : IRule<DomainSpfRecords>
    {
        public Guid Id => Guid.Parse("DD1F7B6C-EA08-4B23-9020-F9C8107A66EC");

        public Task<List<Error>> Evaluate(DomainSpfRecords domainSpfRecords)
        {
            List<Error> errors = new List<Error>();

            if (domainSpfRecords.SpfRecords.MessageSize > 450)
            {
                string errorMessage = string.Format(SpfRulesResource.ShouldBeSmallEnoughForUdp, domainSpfRecords.Domain, domainSpfRecords.SpfRecords.MessageSize);
                string markdown = string.Format(SpfRulesMarkdownResource.ShouldBeSmallEnoughForUdp, domainSpfRecords.Domain);

                errors.Add(new Error(Id, ErrorType.Info, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 2;

        public bool IsStopRule => false;
    }
}
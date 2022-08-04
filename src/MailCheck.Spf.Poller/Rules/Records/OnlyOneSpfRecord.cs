using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.Records
{
    public class OnlyOneSpfRecord : IRule<DomainSpfRecords>
    {
        public Guid Id => Guid.Parse("B260C36F-DF64-4A50-9486-AC29DFFFFD69");

        public Task<List<Error>> Evaluate(DomainSpfRecords domainSpfRecords)
        {
            int recordCount = domainSpfRecords.SpfRecords.Records.Count;
            List<Error> errors = new List<Error>();
            if (recordCount != 1)
            {
                string errorMessage = string.Format(SpfRulesResource.OnlyOneSpfRecordErrorMessage, domainSpfRecords.Domain, recordCount);
                string markdown = string.Format(SpfRulesMarkdownResource.OnlyOneSpfRecordErrorMessage, domainSpfRecords.Domain, recordCount);

                errors.Add(new Error(Id, "mailcheck.spf.onlyOneSpfRecord", ErrorType.Warning, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;

        public bool IsStopRule => false;
    }
}
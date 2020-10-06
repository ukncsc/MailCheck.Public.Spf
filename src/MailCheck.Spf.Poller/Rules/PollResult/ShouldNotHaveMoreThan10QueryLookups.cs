using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.PollResult
{
    public class ShouldNotHaveMoreThan10QueryLookups : IRule<SpfPollResult>
    {
        public Guid Id => Guid.Parse("6BA5A47E-D212-4131-AC74-38F298A57894");

        private readonly ISpfPollerConfig _config;

        public ShouldNotHaveMoreThan10QueryLookups(ISpfPollerConfig config)
        {
            _config = config;
        }

        public Task<List<Error>> Evaluate(SpfPollResult spfRecords)
        {
            List<Error> errors = new List<Error>();

            string recordCountString = spfRecords.QueryCount >= _config.MaxDnsQueryCount
                   ? $"at least {_config.MaxDnsQueryCount}"
                   : spfRecords.QueryCount.ToString();

            if (spfRecords.QueryCount > 10)
            {


                string errorMessage = string.Format(SpfRulesResource.ShouldNotHaveMoreThan10QueryLookupsErrorMessage, recordCountString);
                string markdown = string.Format(SpfRulesMarkdownResource.ShouldNotHaveMoreThan10QueryLookupsErrorMessage, recordCountString);
                errors.Add(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }
            else if (spfRecords.QueryCount > 9)
            {
                string errorMessage = string.Format(SpfRulesResource.CloseTo10QueryLookupsErrorMessage, recordCountString);
                string markdown = string.Format(SpfRulesMarkdownResource.CloseTo10QueryLookupsErrorMessage, recordCountString);
                errors.Add(new Error(Id, ErrorType.Warning, errorMessage, markdown));
            }
            else
            {
                string errorMessage = string.Format(SpfRulesResource.QueryLookupsInfoMessage, recordCountString);
                string markdown = string.Format(SpfRulesMarkdownResource.QueryLookupsInfoMessage, recordCountString);
                errors.Add(new Error(Id, ErrorType.Info, errorMessage, markdown));
            }




            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}

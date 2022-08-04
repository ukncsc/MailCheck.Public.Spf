using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Rules.PollResult
{
    public class ShouldNotHaveMoreThan10QueryLookups : IRule<SpfPollResult>
    {
        public Guid InfoId => Guid.Parse("6BA5A47E-D212-4131-AC74-38F298A57894");
        public Guid WarningId => Guid.Parse("8ab5ea1a-659d-4259-ba74-df25a6e7a617");
        public Guid ErrorId => Guid.Parse("f16e0f37-3b6e-46b8-bc55-0edefde112cf");

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
                errors.Add(new Error(ErrorId, "mailcheck.spf.shouldNotHaveMoreThan10QueryLookups", ErrorType.Error, errorMessage, markdown));
            }
            else if (spfRecords.QueryCount > 9)
            {
                string errorMessage = string.Format(SpfRulesResource.CloseTo10QueryLookupsErrorMessage, recordCountString);
                string markdown = string.Format(SpfRulesMarkdownResource.CloseTo10QueryLookupsErrorMessage, recordCountString);
                errors.Add(new Error(WarningId, "mailcheck.spf.closeTo10QueryLookups", ErrorType.Warning, errorMessage, markdown));
            }
            else if (spfRecords.QueryCount > 6)
            {
                string errorMessage = string.Format(SpfRulesResource.QueryLookupsInfoMessage, recordCountString);
                string markdown = string.Format(SpfRulesMarkdownResource.QueryLookupsInfoMessage, recordCountString);
                errors.Add(new Error(InfoId, "mailcheck.spf.moreThan6QueryLookups", ErrorType.Info, errorMessage, markdown));
            }

            return Task.FromResult(errors);
        }

        public int SequenceNo => 1;
        public bool IsStopRule => false;
    }
}

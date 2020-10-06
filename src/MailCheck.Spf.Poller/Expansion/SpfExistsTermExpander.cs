using System;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Expansion
{
    public class SpfExistsTermExpander : ISpfTermExpanderStrategy
    {
        //Exists causes look up that can be resolved from this context
        //this is a placeholder to ensure dns query count is incremented
        public Task<SpfRecords> Process(string domain, Term term)
        {
            return Task.FromResult<SpfRecords>(null);
        }

        public Type TermType => typeof(Exists);
    }
}
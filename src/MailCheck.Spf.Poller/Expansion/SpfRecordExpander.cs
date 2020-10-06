using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Expansion
{
    public interface ISpfRecordExpander
    {
        Task<int> ExpandSpfRecords(string domain, SpfRecords root);
    }

    public class SpfRecordExpander : ISpfRecordExpander
    {
        private readonly ISpfPollerConfig _config;
        private readonly Lazy<Dictionary<Type, ISpfTermExpanderStrategy>> _strategies;

        public SpfRecordExpander(IEnumerable<ISpfTermExpanderStrategy> strategies, 
            ISpfPollerConfig config)
        {
            _config = config;
            _strategies = new Lazy<Dictionary<Type, ISpfTermExpanderStrategy>>(() => strategies.ToDictionary(_ => _.TermType));
        }

        public async Task<int> ExpandSpfRecords(string domain, SpfRecords root)
        {
            Stack<SpfRecords> spfRecordsStack = new Stack<SpfRecords>(new[] { root });

            int lookupCount = 0;

            do
            {
                SpfRecords spfRecords = spfRecordsStack.Pop();

                foreach (SpfRecord spfRecord in spfRecords.Records)
                {
                    foreach (Term term in spfRecord.Terms)
                    {
                        if (_strategies.Value.TryGetValue(term.GetType(), out ISpfTermExpanderStrategy spfTermExpanderStrategy))
                        {
                            SpfRecords records = await spfTermExpanderStrategy.Process(domain, term);

                            if (records != null)
                            {
                                spfRecordsStack.Push(records);
                            }

                            lookupCount++;
                        }
                    }
                }
            } while (spfRecordsStack.Count > 0 && lookupCount < _config.MaxDnsQueryCount);

            return lookupCount;
        }
    }
}
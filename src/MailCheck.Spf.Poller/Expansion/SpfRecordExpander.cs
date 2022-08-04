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

        public Guid RecursionErrorId => Guid.Parse("35b3a09e-a314-412c-91b8-5c016b7f8d7c");

        public SpfRecordExpander(IEnumerable<ISpfTermExpanderStrategy> strategies,
            ISpfPollerConfig config)
        {
            _config = config;
            _strategies = new Lazy<Dictionary<Type, ISpfTermExpanderStrategy>>(() => strategies.ToDictionary(_ => _.TermType));
        }

        public async Task<int> ExpandSpfRecords(string domain, SpfRecords root)
        {
            Stack<ExpandableSpfRecords> expandableSpfRecordsStack = new Stack<ExpandableSpfRecords>(new[] { new ExpandableSpfRecords(root, new HashSet<string> { domain }) });

            int lookupCount = 0;

            do
            {
                ExpandableSpfRecords expandableSpfRecords = expandableSpfRecordsStack.Pop();

                foreach (SpfRecord spfRecord in expandableSpfRecords.SpfRecords.Records)
                {
                    foreach (Term term in spfRecord.Terms)
                    {
                        if (_strategies.Value.TryGetValue(term.GetType(), out ISpfTermExpanderStrategy spfTermExpanderStrategy))
                        {
                            HashSet<string> branchAntecedents = new HashSet<string>(expandableSpfRecords.Antecedents);
                            if (term is Include include && !branchAntecedents.Add(include.DomainSpec.Domain))
                            {
                                string markdown = string.Format(SpfExpansionMarkdownResource.RecursionDetectedErrorMessage, term.Value);
                                term.AddError(new Error(RecursionErrorId, "mailcheck.spf.recursionDetected", ErrorType.Error, SpfExpansionResource.RecursionDetectedErrorMessage, markdown));
                                break;
                            }

                            SpfRecords records = await spfTermExpanderStrategy.Process(domain, term);

                            if (records != null)
                            {
                                expandableSpfRecordsStack.Push(new ExpandableSpfRecords(records, branchAntecedents));
                            }

                            lookupCount++;
                        }
                    }
                }
            } while (expandableSpfRecordsStack.Count > 0 && lookupCount < _config.MaxDnsQueryCount);

            return lookupCount;
        }

        private class ExpandableSpfRecords
        {
            public ExpandableSpfRecords(SpfRecords spfRecords, HashSet<string> antecedents)
            {
                SpfRecords = spfRecords;
                Antecedents = antecedents;
            }

            public SpfRecords SpfRecords { get; }

            public HashSet<string> Antecedents { get; }
        }
    }
}
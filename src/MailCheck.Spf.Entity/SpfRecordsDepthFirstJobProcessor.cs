using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Entity
{
    public interface ISpfRecordsJobsProcessor
    {
        Task Process(SpfRecords root, params Func<SpfRecords, Task>[] jobs);
    }

    public class SpfRecordsDepthFirstJobProcessor : ISpfRecordsJobsProcessor
    {
        public async Task Process(SpfRecords root, params Func<SpfRecords, Task>[] jobs)
        {
            Stack<SpfRecords> spfRecordsStack = new Stack<SpfRecords>(new[] { root });

            do
            {
                SpfRecords spfRecords = spfRecordsStack.Pop();

                foreach (Func<SpfRecords, Task> job in jobs)
                {
                    await job(spfRecords);
                }

                if (spfRecords != null)
                {
                    foreach (SpfRecord spfRecord in spfRecords.Records)
                    {
                        foreach (Term term in spfRecord.Terms)
                        {
                            if (term is Include include)
                            {
                                if (include.Records != null)
                                {
                                    spfRecordsStack.Push(include.Records);
                                }
                            }
                            else if (term is Redirect redirect)
                            {
                                if (redirect.Records != null)
                                {
                                    spfRecordsStack.Push(redirect.Records);
                                }
                            }
                        }
                    }
                }
            } while (spfRecordsStack.Count > 0);
        }
    }
}
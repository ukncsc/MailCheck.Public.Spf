using System;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Expansion
{
    public interface ISpfTermExpanderStrategy
    {
        Task<SpfRecords> Process(string domain, Term term);
        Type TermType { get; }
    }
}
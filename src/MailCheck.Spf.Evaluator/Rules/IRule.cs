using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Rules
{
    public interface IRule<in T>
    {
        Guid Id { get; }
        Task<List<Message>> Evaluate(T t);
        int SequenceNo { get; }
        bool IsStopRule { get; }
    }
}
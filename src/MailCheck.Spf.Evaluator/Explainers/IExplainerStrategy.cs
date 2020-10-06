using System;

namespace MailCheck.Spf.Evaluator.Explainers
{
    public interface IExplainerStrategy<in T> : IExplainer<T>
    {
        Type Type { get; }
    }
}
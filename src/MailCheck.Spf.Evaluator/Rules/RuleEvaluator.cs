using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Evaluator.Rules
{
    public interface IEvaluator<T>
    {
        Task<EvaluationResult<T>> Evaluate(T item);
    }

    public class Evaluator<T> : IEvaluator<T>
    {
        private readonly List<IRule<T>> _rules;

        public Evaluator(IEnumerable<IRule<T>> rules)
        {
            _rules = rules.OrderBy(_ => _.SequenceNo).ToList();
        }

        public virtual async Task<EvaluationResult<T>> Evaluate(T item)
        {
            List<Message> errors = new List<Message>();
            foreach (IRule<T> rule in _rules)
            {
                List<Message> ruleErrors = await rule.Evaluate(item);

                if (ruleErrors.Any())
                {
                    errors.AddRange(ruleErrors);

                    if (rule.IsStopRule)
                    {
                        break;
                    }
                }
            }
            return new EvaluationResult<T>(item, errors);
        }
    }
}
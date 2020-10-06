using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public abstract class SpfEntity
    {
        private readonly List<Error> _errors = new List<Error>();

        public bool Valid => !_errors.Any();

        public int ErrorCount => _errors.Count;

        public IReadOnlyList<Error> Errors => _errors.ToArray();

        public bool AllValid => AllErrorCount == 0;

        public virtual int AllErrorCount => ErrorCount;

        public virtual IReadOnlyList<Error> AllErrors => Errors;

        public void AddError(Error error)
        {
            _errors.Add(error);
        }

        public void AddErrors(List<Error> errors)
        {
            _errors.AddRange(errors);
        }
    }
}
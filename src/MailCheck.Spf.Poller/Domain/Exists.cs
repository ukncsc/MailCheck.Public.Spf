using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class Exists : Mechanism
    {
        public Exists(string value, Qualifier qualifier, DomainSpec domainSpec) 
            : base(value, qualifier)
        {
            DomainSpec = domainSpec;
        }

        public DomainSpec DomainSpec { get; }

        public override IReadOnlyList<Error> AllErrors => DomainSpec.AllErrors.Concat(Errors).ToArray();

        public override int AllErrorCount => DomainSpec.AllErrorCount + ErrorCount;
    }
}
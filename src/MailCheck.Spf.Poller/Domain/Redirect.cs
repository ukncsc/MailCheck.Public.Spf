using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class Redirect : Modifier
    {
        public Redirect(string value, DomainSpec domainSpec) 
            : base(value)
        {
            DomainSpec = domainSpec;
        }

        public DomainSpec DomainSpec { get; }

        public SpfRecords Records { get; set; }

        public override IReadOnlyList<Error> AllErrors => DomainSpec.AllErrors.Concat(Errors).ToArray();

        public override int AllErrorCount => DomainSpec.AllErrorCount + ErrorCount;
    }
}
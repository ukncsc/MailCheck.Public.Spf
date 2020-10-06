using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class Mx : Mechanism
    {
        public Mx(string value, Qualifier qualifier, DomainSpec domainSpec, DualCidrBlock dualCidrBlock)
            : base(value, qualifier)
        {
            DomainSpec = domainSpec;
            DualCidrBlock = dualCidrBlock;
        }

        public DomainSpec DomainSpec { get; }

        public DualCidrBlock DualCidrBlock { get; }

        public List<MxHost> MxHosts { get; set; }

        public override IReadOnlyList<Error> AllErrors => DualCidrBlock.AllErrors.Concat(DomainSpec.AllErrors.Concat(Errors)).ToArray();

        public override int AllErrorCount => DualCidrBlock.AllErrorCount + DomainSpec.AllErrorCount + ErrorCount;
    }
}
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Poller.Domain
{
    public class Ip4 : Mechanism
    {
        public Ip4(string value, Qualifier qualifier, Ip4Addr ipAddress, Ip4CidrBlock cidrBlock) 
            : base(value, qualifier)
        {
            IpAddress = ipAddress;
            CidrBlock = cidrBlock;
        }

        public Ip4Addr IpAddress { get; }

        public Ip4CidrBlock CidrBlock { get; }

        public override IReadOnlyList<Error> AllErrors => IpAddress.AllErrors.Concat(CidrBlock.AllErrors.Concat(Errors)).ToArray();

        public override int AllErrorCount => IpAddress.AllErrorCount + CidrBlock.AllErrorCount + ErrorCount;
    }
}
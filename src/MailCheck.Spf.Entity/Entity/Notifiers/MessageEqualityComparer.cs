using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Spf.Entity.Entity.Notifiers
{
    public class MessageEqualityComparer : IEqualityComparer<Contracts.SharedDomain.Message>
    {
        public bool Equals(Contracts.SharedDomain.Message x, Contracts.SharedDomain.Message y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Contracts.SharedDomain.Message obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

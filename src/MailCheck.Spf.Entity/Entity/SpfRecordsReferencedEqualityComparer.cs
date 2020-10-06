using System.Collections.Generic;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Entity.Entity
{
    public class SpfRecordsReferencedEqualityComparer : IEqualityComparer<SpfRecord>
    {
        public bool Equals(SpfRecord x, SpfRecord y)
        {
            return x.RecordsStrings.CollectionEqual(y.RecordsStrings);
        }

        public int GetHashCode(SpfRecord obj)
        {
            return obj.GetHashCode();
        }
    }
}
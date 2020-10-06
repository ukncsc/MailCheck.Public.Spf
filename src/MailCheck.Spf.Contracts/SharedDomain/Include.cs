using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Include : Mechanism, IEquatable<Include>
    {
        public Include(Qualifier qualifier, string value, string domain, SpfRecords records, bool valid) 
            : base(TermType.Include, qualifier, value, valid)
        {
            Domain = domain;
            Records = records;
        }

        public string Domain { get; }

        public SpfRecords Records { get; }

        public override bool Equals(Term obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Include) obj);
        }

        public bool Equals(Include other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && 
                   string.Equals(Domain, other.Domain) && 
                   Equals(Records, other.Records);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Include) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Records != null ? Records.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Include left, Include right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Include left, Include right)
        {
            return !Equals(left, right);
        }
    }
}
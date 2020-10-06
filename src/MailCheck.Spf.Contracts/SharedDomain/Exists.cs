using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Exists : Mechanism, IEquatable<Exists>
    {
        public Exists(Qualifier qualifier, string value, string domain, bool valid) 
            : base(TermType.Exists,qualifier, value, valid)
        {
            Domain = domain;
        }

        public string Domain { get; }

        public bool Equals(Exists other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && 
                   string.Equals(Domain, other.Domain);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Exists) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Exists left, Exists right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Exists left, Exists right)
        {
            return !Equals(left, right);
        }
    }
}
using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Explanation : Modifier, IEquatable<Explanation>
    {
        public Explanation(string value, string domain, bool valid) 
            : base(TermType.Explanation, value, valid)
        {
            Domain = domain;
        }

        public string Domain { get; }

        public bool Equals(Explanation other)
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
            return Equals((Explanation) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Explanation left, Explanation right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Explanation left, Explanation right)
        {
            return !Equals(left, right);
        }
    }
}
using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public abstract class Mechanism : Term, IEquatable<Mechanism>
    {
        protected Mechanism(TermType termType, Qualifier qualifier, string value, bool valid) 
            : base(termType, value, valid)
        {
            Qualifier = qualifier;
        }

        public Qualifier Qualifier { get; }

        public bool Equals(Mechanism other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Qualifier == other.Qualifier;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Mechanism) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ (int) Qualifier;
            }
        }

        public static bool operator ==(Mechanism left, Mechanism right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Mechanism left, Mechanism right)
        {
            return !Equals(left, right);
        }
    }
}
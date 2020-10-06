using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class All : Mechanism, IEquatable<All>
    {
        public All(Qualifier qualifier, string value, bool valid, bool @implicit) 
            : base(TermType.All, qualifier, value, valid)
        {
            Implicit = @implicit;
        }

        public bool Implicit { get; }

        public bool Equals(All other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Implicit == other.Implicit;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((All) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode() * 397) ^ Implicit.GetHashCode();
            }
        }

        public static bool operator ==(All left, All right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(All left, All right)
        {
            return !Equals(left, right);
        }
    }
}
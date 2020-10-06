using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Ip6 : Mechanism, IEquatable<Ip6>
    {
        public Ip6(Qualifier qualifier, string value, string ip, int? ip6Cidr, bool valid) 
            : base(TermType.Ip6, qualifier, value, valid)
        {
            Ip = ip;
            Ip6Cidr = ip6Cidr;
        }

        public string Ip { get; }

        public int? Ip6Cidr { get; }

        public bool Equals(Ip6 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && 
                   string.Equals(Ip, other.Ip) && 
                   Ip6Cidr == other.Ip6Cidr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Ip6) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Ip != null ? Ip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ip6Cidr.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Ip6 left, Ip6 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Ip6 left, Ip6 right)
        {
            return !Equals(left, right);
        }
    }
}
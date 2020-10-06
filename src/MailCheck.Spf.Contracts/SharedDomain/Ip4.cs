using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Ip4 : Mechanism, IEquatable<Ip4>
    {
        public Ip4(Qualifier qualifier, string value, string ip, int? ip4Cidr, bool valid) 
            : base(TermType.Ip4, qualifier, value, valid)
        {
            Ip = ip;
            Ip4Cidr = ip4Cidr;
        }

        public string Ip { get; }

        public int? Ip4Cidr { get; }

        public bool Equals(Ip4 other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && 
                   string.Equals(Ip, other.Ip) && 
                   Ip4Cidr == other.Ip4Cidr;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Ip4) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Ip != null ? Ip.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ip4Cidr.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Ip4 left, Ip4 right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Ip4 left, Ip4 right)
        {
            return !Equals(left, right);
        }
    }
}
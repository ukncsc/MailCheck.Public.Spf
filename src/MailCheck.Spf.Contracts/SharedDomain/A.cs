using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class A : Mechanism, IEquatable<A>
    {
        public A(
            Qualifier qualifier, 
            string value, 
            string domain, 
            int? ip4Cidr, 
            int? ip6Cidr, 
            List<string> ips,
            bool valid) 
            : base(TermType.A, qualifier, value, valid)
        {
            Domain = domain;
            Ip4Cidr = ip4Cidr;
            Ip6Cidr = ip6Cidr;
            Ips = ips ?? new List<string>();
        }

        public string Domain { get; }

        public int? Ip4Cidr { get; }

        public int? Ip6Cidr { get; }

        public List<string> Ips { get; }

        public bool Equals(A other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && 
                   string.Equals(Domain, other.Domain) && 
                   Ip4Cidr == other.Ip4Cidr && 
                   Ip6Cidr == other.Ip6Cidr && 
                   Ips.CollectionEqual(other.Ips);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((A) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = base.GetHashCode();
                hashCode = (hashCode * 397) ^ (Domain != null ? Domain.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Ip4Cidr.GetHashCode();
                hashCode = (hashCode * 397) ^ Ip6Cidr.GetHashCode();
                hashCode = (hashCode * 397) ^ (Ips != null ? Ips.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(A left, A right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(A left, A right)
        {
            return !Equals(left, right);
        }
    }
}
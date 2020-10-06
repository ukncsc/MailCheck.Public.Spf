using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class MxHost : IEquatable<MxHost>
    {
        public MxHost(string hostname, List<string> ips)
        {
            Hostname = hostname;
            Ips = ips ?? new List<string>();
        }

        public string Hostname { get; }

        public List<string> Ips { get; }

        public bool Equals(MxHost other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Hostname, other.Hostname) && 
                   Ips.CollectionEqual(other.Ips);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((MxHost) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Hostname != null ? Hostname.GetHashCode() : 0) * 397) ^ (Ips != null ? Ips.GetHashCode() : 0);
            }
        }

        public static bool operator ==(MxHost left, MxHost right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(MxHost left, MxHost right)
        {
            return !Equals(left, right);
        }
    }
}
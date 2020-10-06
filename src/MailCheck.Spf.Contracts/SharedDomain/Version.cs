using System;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class Version : IEquatable<Version>
    {
        public Version(string value, bool valid)
        {
            Value = value;
            Valid = valid;
        }

        public string Value { get; }

        public bool Valid { get; }

        public string Explanation { get; set; }

        public bool Equals(Version other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Value, other.Value) && 
                   Valid == other.Valid && 
                   string.Equals(Explanation, other.Explanation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Version) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Valid.GetHashCode();
                hashCode = (hashCode * 397) ^ (Explanation != null ? Explanation.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Version left, Version right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Version left, Version right)
        {
            return !Equals(left, right);
        }
    }
}

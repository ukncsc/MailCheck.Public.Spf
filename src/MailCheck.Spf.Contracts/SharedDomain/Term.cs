using System;
using System.Collections.Generic;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public abstract class Term : IEquatable<Term>
    {
        protected Term(TermType termType, string value, bool valid)
        {
            TermType = termType;
            Value = value;
            Valid = valid;
        }

        public TermType TermType { get; }

        public string Value { get; }

        public bool Valid { get; }

        public string Explanation { get; set; }

        public virtual bool Equals(Term other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TermType == other.TermType && 
                   string.Equals(Value, other.Value) && 
                   Valid == other.Valid && 
                   string.Equals(Explanation, other.Explanation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Term) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (int) TermType;
                hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Valid.GetHashCode();
                hashCode = (hashCode * 397) ^ (Explanation != null ? Explanation.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Term left, Term right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Term left, Term right)
        {
            return !Equals(left, right);
        }
    }
}
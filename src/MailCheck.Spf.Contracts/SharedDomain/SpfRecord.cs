using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class SpfRecord : IEquatable<SpfRecord>
    {
        public SpfRecord(List<string> recordsStrings, Version version, List<Term> terms, List<Message> messages, bool? isRoot)
        {
            RecordsStrings = recordsStrings ?? new List<string>();
            Record = string.Join(string.Empty, RecordsStrings);
            Version = version;
            IsRoot = isRoot ?? false;
            Terms = terms ?? new List<Term>();
            Messages = messages ?? new List<Message>();
        }

        public List<string> RecordsStrings { get; }

        [JsonIgnore]
        public string Record { get; }

        public Version Version { get; }

        public bool IsRoot { get; }

        public List<Term> Terms { get; }

        public List<Message> Messages { get; set; }

        public bool Equals(SpfRecord other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return RecordsStrings.CollectionEqual(other.RecordsStrings) &&
                   Equals(Version, other.Version) &&
                   Equals(IsRoot, other.IsRoot) &&
                   Terms.CollectionEqual(other.Terms) &&
                   Messages.CollectionEqual(other.Messages);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpfRecord) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (RecordsStrings != null ? RecordsStrings.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Version != null ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ IsRoot.GetHashCode();
                hashCode = (hashCode * 397) ^ (Terms != null ? Terms.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Messages != null ? Messages.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SpfRecord left, SpfRecord right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SpfRecord left, SpfRecord right)
        {
            return !Equals(left, right);
        }
    }
}
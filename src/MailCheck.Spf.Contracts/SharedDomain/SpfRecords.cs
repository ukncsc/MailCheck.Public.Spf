using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MailCheck.Spf.Contracts.SharedDomain
{
    public class SpfRecords : IEquatable<SpfRecords>
    {
        public SpfRecords(List<SpfRecord> records, int payloadSizeBytes, List<Message> messages)
        {
            Records = records ?? new List<SpfRecord>();
            PayloadSizeBytes = payloadSizeBytes;
            Messages = messages ?? new List<Message>();
        }

        public List<SpfRecord> Records { get; }

        public int PayloadSizeBytes { get; }

        public List<Message> Messages { get; set; }
        
        public bool Equals(SpfRecords other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Records.CollectionEqual(other.Records) &&
                   PayloadSizeBytes == other.PayloadSizeBytes &&
                   Messages.CollectionEqual(other.Messages);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((SpfRecords)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Records != null ? Records.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ PayloadSizeBytes;
                hashCode = (hashCode * 397) ^ (Messages != null ? Messages.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(SpfRecords left, SpfRecords right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SpfRecords left, SpfRecords right)
        {
            return !Equals(left, right);
        }
    }
}
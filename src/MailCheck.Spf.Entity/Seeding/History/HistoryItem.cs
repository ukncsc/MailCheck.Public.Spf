using System;
using System.Collections.Generic;

namespace MailCheck.Spf.Entity.Seeding.History
{
    public class HistoryItem
    {
        public HistoryItem(string id, List<string> records, DateTime startDate)
        {
            Id = id;
            Records = records ?? new List<string>();
            StartDate = startDate;
        }

        public string Id { get; }
        public List<string> Records { get; }
        public DateTime StartDate { get; }
    }
}
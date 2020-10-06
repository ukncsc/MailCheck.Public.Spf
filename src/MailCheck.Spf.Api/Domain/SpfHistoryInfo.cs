using System;
using System.Collections.Generic;
using System.Linq;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfHistoryInfo
    {
        public SpfHistoryInfo(DateTime startDate, DateTime? endDate, List<string> spfRecords = null)
        {
            StartDate = startDate;
            EndDate = endDate;
            SpfRecords = spfRecords ?? new List<string>();
        }

        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string> SpfRecords { get; set; }
    }
}
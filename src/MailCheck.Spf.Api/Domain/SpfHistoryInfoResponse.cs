using System;
using System.Collections.Generic;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfHistoryInfoResponse
    {
        public SpfHistoryInfoResponse(string id, List<SpfHistoryInfo> spfHistory = null)
        {
            Id = id;
            SpfHistory = spfHistory ?? new List<SpfHistoryInfo>();
        }

        public string Id { get; }

        public List<SpfHistoryInfo> SpfHistory { get; }
    }
    
}

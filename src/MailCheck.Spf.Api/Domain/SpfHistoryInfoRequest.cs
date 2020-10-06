using System;
using System.Collections.Generic;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfHistoryInfoRequest
    {
        public int UserId { get; set; }
        public string Domain { get; set; }
    }
}

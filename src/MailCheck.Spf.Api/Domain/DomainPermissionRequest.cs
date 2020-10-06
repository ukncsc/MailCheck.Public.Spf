using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Spf.Api.Domain
{
    public class DomainPermissionRequest
    {
        public DomainPermissionRequest(int userId, string domain)
        {
            UserId = userId;
            Domain = domain;
        }

        public int UserId { get; set; }
        public string Domain { get; set; }
    }
}

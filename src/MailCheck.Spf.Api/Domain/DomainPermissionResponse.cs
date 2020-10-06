using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Spf.Api.Domain
{
    public class DomainPermissionResponse
    {
        public string Domain { get; }
        public bool AggregatePermission { get; }
        public bool DomainPermission { get; }

        public DomainPermissionResponse(string domain, bool aggregatePermission, bool domainPermission)
        {
            Domain = domain;
            AggregatePermission = aggregatePermission;
            DomainPermission = domainPermission;
        }
    }
}

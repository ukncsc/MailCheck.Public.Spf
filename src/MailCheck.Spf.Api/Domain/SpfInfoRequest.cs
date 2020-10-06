using System;
using System.Collections.Generic;
using System.Text;

namespace MailCheck.Spf.Api.Domain
{
    public class SpfInfoListRequest
    {
        public SpfInfoListRequest()
        {
            HostNames = new List<string>();
        }

        public List<string> HostNames { get; set; }
    }
}

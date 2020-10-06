using System.Collections.Generic;

namespace MailCheck.Spf.Poller.Domain
{
    public class MxHost
    {
        public MxHost(string host, List<string> ip4s)
        {
            Host = host;
            Ip4S = ip4s;
        }

        public string Host { get; }
        public List<string> Ip4S { get; }
    }
}
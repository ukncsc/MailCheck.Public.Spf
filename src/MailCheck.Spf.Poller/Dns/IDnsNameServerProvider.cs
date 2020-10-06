using System.Collections.Generic;
using System.Net;

namespace MailCheck.Dkim.Poller.Dns
{
    public interface IDnsNameServerProvider
    {
        List<IPAddress> GetNameServers();
    }
}
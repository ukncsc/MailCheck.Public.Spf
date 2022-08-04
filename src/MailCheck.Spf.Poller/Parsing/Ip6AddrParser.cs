using System;
using System.Net;
using System.Net.Sockets;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IIp6AddrParser
    {
        Ip6Addr Parse(string ipAddressString);
    }

    public class Ip6AddrParser : IIp6AddrParser
    {
        public Guid Id => Guid.Parse("4A07C1EE-B9EF-4F88-BEFD-ECF978D2721A");

        public Ip6Addr Parse(string ipAddressString)
        {
            Ip6Addr ip6Addr = new Ip6Addr(ipAddressString);
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAddressString, out ipAddress))
            {
                if (ipAddress.AddressFamily != AddressFamily.InterNetworkV6)
                {
                    string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ipv6 address", ipAddressString);
                    string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ipv6 address", ipAddressString);
                    ip6Addr.AddError(new Error(Id, "mailcheck.spf.ipv6AddrInvalid", ErrorType.Error, errorMessage, markdown));
                }
            }
            else
            {
                string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ip address", ipAddressString);
                string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ip address", ipAddressString);

                ip6Addr.AddError(new Error(Id, "mailcheck.spf.ipv6AddrInvalid", ErrorType.Error, errorMessage, markdown));
            }
            return ip6Addr;
        }
    }
}
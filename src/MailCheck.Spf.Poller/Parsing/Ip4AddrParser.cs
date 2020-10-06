using System;
using System.Net;
using System.Net.Sockets;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IIp4AddrParser
    {
        Ip4Addr Parse(string ipAddressString);
    }

    public class Ip4AddrParser : IIp4AddrParser
    {
        public Guid Id => Guid.Parse("1D50B64C-70C4-45FB-B43D-F90FB741DCF2");

        public Ip4Addr Parse(string ipAddressString)
        {
            Ip4Addr ip4Addr = new Ip4Addr(ipAddressString);
            IPAddress ipAddress;
            if (IPAddress.TryParse(ipAddressString, out ipAddress))
            {
                if (ipAddress.AddressFamily != AddressFamily.InterNetwork)
                {
                    string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ipv4 address", ipAddressString);
                    string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ipv4 address", ipAddressString);

                    ip4Addr.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));
                }
            }
            else
            {
                string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ip address", ipAddressString);
                string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ip address", ipAddressString);

                ip4Addr.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));
            }

            return ip4Addr;
        }
    }
}
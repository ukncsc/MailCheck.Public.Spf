using System;
using System.Text.RegularExpressions;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IIp6CidrBlockParser
    {
        Ip6CidrBlock Parse(string cidrBlock);
    }

    public class Ip6CidrBlockParser : IIp6CidrBlockParser
    {
        private readonly Regex _regex = new Regex("^(0|[1-9]{1}|[1-9]{1}[0-9]{1}|[1]{1}[0-2]{1}[0-8]{1})$");
        private const int DefaultIp6CidrBlock = 128;

        public Guid Id => Guid.Parse("C4581C65-B293-4425-A32E-DE3E8DB1C7B1");

        public Ip6CidrBlock Parse(string cidrBlock)
        {
            if (string.IsNullOrEmpty(cidrBlock))
            {
                return new Ip6CidrBlock(DefaultIp6CidrBlock);
            }

            if (_regex.IsMatch(cidrBlock))
            {
                return new Ip6CidrBlock(int.Parse(cidrBlock));
            }

            Ip6CidrBlock ip6CidrBlock = new Ip6CidrBlock(null);

            string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ipv6 cidr block", $"{cidrBlock}. Value must be in the range 0-128");
            string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ipv6 cidr block", $"{cidrBlock}. Value must be in the range 0-128");

            ip6CidrBlock.AddError(new Error(Id, ErrorType.Error, errorMessage, markdown));

            return ip6CidrBlock;
        }
    }
}
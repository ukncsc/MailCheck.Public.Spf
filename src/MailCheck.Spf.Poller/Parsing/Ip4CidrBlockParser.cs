using System;
using System.Text.RegularExpressions;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IIp4CidrBlockParser
    {
        Ip4CidrBlock Parse(string cidrBlock);
    }

    public class Ip4CidrBlockParser : IIp4CidrBlockParser
    {
        public Guid Id => Guid.Parse("66B83868-2499-4EE4-AC71-D303B1675CEC");

        private readonly Regex _regex = new Regex("^(0|[1-9]{1}|[1-2]{1}[0-9]{1}|[3]{1}[0-2]{1})$");
        private const int DefaultIp4CidrBlock = 32;

        public Ip4CidrBlock Parse(string cidrBlock)
        {
            if (string.IsNullOrEmpty(cidrBlock))
            {
                return new Ip4CidrBlock(DefaultIp4CidrBlock);
            }

            if (_regex.IsMatch(cidrBlock))
            {
                return new Ip4CidrBlock(int.Parse(cidrBlock));
            }

            Ip4CidrBlock ip4CidrBlock = new Ip4CidrBlock(null);
            string errorMessage = string.Format(SpfParserResource.InvalidValueErrorMessage, "ipv4 cidr block", $"{cidrBlock}. Value must be in the range 0-32");
            string markdown = string.Format(SpfParserMarkdownResource.InvalidValueErrorMessage, "ipv4 cidr block", $"{cidrBlock}. Value must be in the range 0-32");

            ip4CidrBlock.AddError(new Error(Id, "mailcheck.spf.ipv4CidrInvalid", ErrorType.Error, errorMessage, markdown));
            return ip4CidrBlock;
        }
    }
}
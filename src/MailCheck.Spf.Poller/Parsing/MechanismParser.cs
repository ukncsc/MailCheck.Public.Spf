using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IMechanismParser
    {
        bool TryParse(string mechanism, out Term term);
    }

    public class MechanismParser : IMechanismParser
    {
        private readonly Regex _mechanismRegex =
            new Regex(
                @"^(?<qualifier>[+?~-]?)(?<mechanism>(all)|(include)|(A)|(MX)|(PTR)|(IP4)|(IP6)|(exists))(:?(?<argument>.+))?$",
                RegexOptions.IgnoreCase);

        private readonly IQualifierParser _qualifierParser;
        private readonly Dictionary<string, IMechanismParserStrategy> _parserStrategies;

        public Guid Id => Guid.Parse("761D1A5A-5326-4817-AF78-57B6F520EE60");
        
        public MechanismParser(IQualifierParser qualifierParser,
            IEnumerable<IMechanismParserStrategy> parserStrategies)
        {
            _qualifierParser = qualifierParser;
            _parserStrategies = parserStrategies.ToDictionary(_ => _.Mechanism);
        }

        public bool TryParse(string mechanism, out Term term)
        {
            Match match = _mechanismRegex.Match(mechanism);
            if (match.Success)
            {
                string qualifierToken = match.Groups["qualifier"].Value;
                string mechanismToken = match.Groups["mechanism"].Value.ToLower();
                string argumentToken = match.Groups["argument"].Value;

                Qualifier qualifier = _qualifierParser.Parse(qualifierToken);
                
                if (!_parserStrategies.TryGetValue(mechanismToken, out IMechanismParserStrategy strategy))
                {
                    throw new ArgumentException($"No strategy found to process {mechanismToken}");
                }

                term = strategy.Parse(mechanism, qualifier, argumentToken);

                if (qualifier == Qualifier.Unknown)
                {
                    string message = string.Format(SpfParserResource.UnknownQualifierErrorMessage, qualifierToken);
                    string markdown = string.Format(SpfParserMarkdownResource.UnknownQualifierErrorMessage, qualifierToken);

                    term.AddError(new Error(Id, ErrorType.Error, message, markdown));
                }

                return true;
            }
            term = null;
            return false;
        }
    }
}
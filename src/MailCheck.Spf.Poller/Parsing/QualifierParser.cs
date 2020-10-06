using System;
using MailCheck.Spf.Poller.Domain;

namespace MailCheck.Spf.Poller.Parsing
{
    public interface IQualifierParser
    {
        Qualifier Parse(string qualifier);
    }

    public class QualifierParser : IQualifierParser
    {
        public Qualifier Parse(string qualifier)
        {
            switch (qualifier)
            {
                case "":
                case "+":
                    return Qualifier.Pass;
                case "-":
                    return Qualifier.Fail;
                case "?":
                    return Qualifier.Neutral;
                case "~":
                    return Qualifier.SoftFail;
                default:
                    return Qualifier.Unknown;
            }
        }
    }
}
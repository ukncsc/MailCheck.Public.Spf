using System;
using System.Linq;
using MailCheck.Spf.Contracts;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Poller.Domain;

using ContractSpfRecords = MailCheck.Spf.Contracts.SharedDomain.SpfRecords;
using ContractSpfRecord = MailCheck.Spf.Contracts.SharedDomain.SpfRecord;
using ContractTerm = MailCheck.Spf.Contracts.SharedDomain.Term;
using ContractA = MailCheck.Spf.Contracts.SharedDomain.A;
using ContractQualifier = MailCheck.Spf.Contracts.SharedDomain.Qualifier;
using ContractAll = MailCheck.Spf.Contracts.SharedDomain.All;
using ContractExists = MailCheck.Spf.Contracts.SharedDomain.Exists;
using ContractExplanation = MailCheck.Spf.Contracts.SharedDomain.Explanation;
using ContractInclude = MailCheck.Spf.Contracts.SharedDomain.Include;
using ContractIp4 = MailCheck.Spf.Contracts.SharedDomain.Ip4;
using ContractIp6 = MailCheck.Spf.Contracts.SharedDomain.Ip6;
using ContractMx = MailCheck.Spf.Contracts.SharedDomain.Mx;
using ContractMxHost = MailCheck.Spf.Contracts.SharedDomain.MxHost;
using ContractPtr = MailCheck.Spf.Contracts.SharedDomain.Ptr;
using ContractRedirect = MailCheck.Spf.Contracts.SharedDomain.Redirect;
using ContractUnknownTerm = MailCheck.Spf.Contracts.SharedDomain.UnknownTerm;
using ContractVersion = MailCheck.Spf.Contracts.SharedDomain.Version;

using A = MailCheck.Spf.Poller.Domain.A;
using All = MailCheck.Spf.Poller.Domain.All;
using Exists = MailCheck.Spf.Poller.Domain.Exists;
using Explanation = MailCheck.Spf.Poller.Domain.Explanation;
using Include = MailCheck.Spf.Poller.Domain.Include;
using Ip4 = MailCheck.Spf.Poller.Domain.Ip4;
using Ip6 = MailCheck.Spf.Poller.Domain.Ip6;
using Mx = MailCheck.Spf.Poller.Domain.Mx;
using MxHost = MailCheck.Spf.Poller.Domain.MxHost;
using Ptr = MailCheck.Spf.Poller.Domain.Ptr;
using Qualifier = MailCheck.Spf.Poller.Domain.Qualifier;
using Redirect = MailCheck.Spf.Poller.Domain.Redirect;
using SpfRecord = MailCheck.Spf.Poller.Domain.SpfRecord;
using SpfRecords = MailCheck.Spf.Poller.Domain.SpfRecords;
using Term = MailCheck.Spf.Poller.Domain.Term;
using Version = MailCheck.Spf.Poller.Domain.Version;

namespace MailCheck.Spf.Poller.Mapping
{
    public static class Mapping
    {
        public static SpfRecordsPolled ToSpfRecordsPolled(this SpfPollResult spfPollResult, string domain)
        {
            return new SpfRecordsPolled(domain, 
                spfPollResult.Records.ToContract(true), 
                spfPollResult.QueryCount, 
                spfPollResult.Elapsed,
                spfPollResult.Errors.Select(_ => _.ToContract()).ToList());
        }

        private static ContractSpfRecords ToContract(this SpfRecords spfRecords, bool root = false)
        {
            return new ContractSpfRecords(
                spfRecords.Records.Select(_ => _.ToContractSpfRecord(root)).ToList(),
                spfRecords.MessageSize,
                spfRecords.Errors.Select(_ => _.ToContract()).ToList());
        }

        private static ContractSpfRecord ToContractSpfRecord(this SpfRecord spfRecord, bool root = false)
        {
            return new ContractSpfRecord(spfRecord.RecordStrings, 
                spfRecord.Version?.ToContractVersion(),
                spfRecord.Terms.Select(_ => _.ToContract()).ToList(),
                spfRecord.AllErrors.Select(_ => _.ToContract()).ToList(),
                root);
        }

        private static ContractVersion ToContractVersion(this Version version)
        {
            return new ContractVersion(version.Value, version.AllValid);
        }

        private static ContractTerm ToContract(this Term term)
        {
            switch (term)
            {
                case A a:
                    return a.ToContract();
                case All all:
                    return all.ToContract();
                case Exists exists:
                    return exists.ToContract();
                case Explanation explanation :
                    return explanation.ToContract();
                case Include include:
                    return include.ToContract();
                case Ip4 ip4:
                    return ip4.ToContract();
                case Ip6 ip6:
                    return ip6.ToContract();
                case Mx mx:
                    return mx.ToContract();
                case Ptr ptr:
                    return ptr.ToContract();
                case Redirect redirect:
                    return redirect.ToContract();
            }

            return new ContractUnknownTerm(term.Value, term.AllValid);
        }

        private static ContractA ToContract(this A a)
        {
            return new ContractA(a.Qualifier.ToContract(),
                a.Value,
                a.DomainSpec.Domain,
                a.DualCidrBlock?.Ip4CidrBlock?.Value,
                a.DualCidrBlock?.Ip6CidrBlock?.Value,
                a.Ip4s,
                a.AllValid);
        }

        private static ContractAll ToContract(this All all)
        {
            return new ContractAll(all.Qualifier.ToContract(), 
                all.Value, 
                all.AllValid, 
                all.IsImplicit);
        }

        private static ContractExists ToContract(this Exists exists)
        {
            return new ContractExists(exists.Qualifier.ToContract(), 
                exists.Value, 
                exists.DomainSpec?.Domain,
                exists.AllValid);
        }

        private static ContractExplanation ToContract(this Explanation explanation)
        {
            return new ContractExplanation(explanation.Value, explanation.DomainSpec?.Domain, explanation.AllValid);
        }

        private static ContractInclude ToContract(this Include include)
        {
            return new ContractInclude(include.Qualifier.ToContract(), 
                include.Value, 
                include.DomainSpec?.Domain,
                include.Records?.ToContract(),
                include.AllValid);
        }

        private static ContractIp4 ToContract(this Ip4 ip4)
        {
            return new ContractIp4(
                ip4.Qualifier.ToContract(), 
                ip4.Value, 
                ip4.IpAddress?.Value,
                ip4.CidrBlock?.Value,
                ip4.AllValid);
        }

        private static ContractIp6 ToContract(this Ip6 ip6)
        {
            return new ContractIp6(
                ip6.Qualifier.ToContract(),
                ip6.Value,
                ip6.IpAddress?.Value,
                ip6.CidrBlock?.Value,
                ip6.AllValid);
        }

        private static ContractMx ToContract(this Mx mx)
        {
            return new ContractMx(
                mx.Qualifier.ToContract(),
                mx.Value,
                mx.DomainSpec?.Domain,
                mx.DualCidrBlock?.Ip4CidrBlock?.Value,
                mx.DualCidrBlock?.Ip6CidrBlock?.Value,
                mx.MxHosts?.Select(_ => _.ToContract()).ToList(),
                mx.AllValid);
        }

        private static ContractMxHost ToContract(this MxHost mxHost)
        {
            return new ContractMxHost(mxHost.Host, mxHost.Ip4S);
        }

        private static ContractPtr ToContract(this Ptr ptr)
        {
            return new ContractPtr(
                ptr.Qualifier.ToContract(),
                ptr.Value,
                ptr.DomainSpec?.Domain,
                ptr.AllValid);
        }

        private static ContractRedirect ToContract(this Redirect redirect)
        {
            return new ContractRedirect(redirect.Value, 
                redirect.DomainSpec?.Domain,
                redirect.Records?.ToContract(),
                redirect.AllValid);
        }

        private static ContractQualifier ToContract(this Qualifier qualifier)
        {
            switch (qualifier)
            {
                case Qualifier.Pass:
                    return ContractQualifier.Pass;
                case Qualifier.Fail:
                    return ContractQualifier.Fail;
                case Qualifier.SoftFail:
                    return ContractQualifier.SoftFail;
                case Qualifier.Neutral:
                    return ContractQualifier.Neutral;
                default:
                    return ContractQualifier.Unknown;
            }
        }

        private static Message ToContract(this Error error)
        {
            return new Message(error.Id, error.Name, MessageSources.SpfPoller, error.ErrorType.ToContract(), error.Message, error.Markdown);
        }

        private static MessageType ToContract(this ErrorType errorType)
        {
            switch (errorType)
            {
                case ErrorType.Error:
                    return MessageType.error;
                case ErrorType.Warning:
                    return MessageType.warning;
                case ErrorType.Info:
                    return MessageType.info;
                default:
                    return MessageType.error;
            }
        }
    }
}
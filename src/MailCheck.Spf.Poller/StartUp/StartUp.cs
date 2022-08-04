using System;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using Amazon.SimpleNotificationService;
using DnsClient;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Util;
using MailCheck.Dkim.Poller.Dns;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Expansion;
using MailCheck.Spf.Poller.Implicit;
using MailCheck.Spf.Poller.Parsing;
using MailCheck.Spf.Poller.Rules;
using MailCheck.Spf.Poller.Rules.PollResult;
using MailCheck.Spf.Poller.Rules.Record;
using MailCheck.Spf.Poller.Rules.Records;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Spf.Poller.StartUp
{
    internal class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TermConverter());

                return serializerSetting;
            };

            services
                .AddTransient<SpfProcessor>()
                .AddSingleton(CreateLookupClient)
                .AddTransient<ISpfRecordsParser, SpfRecordsParser>()
                .AddTransient<IAuditTrailParser, AuditTrailParser>()
                .AddTransient<IDnsClient, Dns.DnsClient>()
                .AddTransient<IDnsNameServerProvider, LinuxDnsNameServerProvider>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<ISpfRecordParser, SpfRecordParser>()
                .AddTransient<ISpfVersionParser, SpfVersionParser>()
                .AddTransient<IMechanismParserStrategy, AllMechanismParser>()
                .AddTransient<IMechanismParserStrategy, IncludeMechanismParser>()
                .AddTransient<IMechanismParserStrategy, AMechanismParser>()
                .AddTransient<IMechanismParserStrategy, MxMechanismParser>()
                .AddTransient<IMechanismParserStrategy, PtrMechanismParser>()
                .AddTransient<IMechanismParserStrategy, Ip4MechanismParser>()
                .AddTransient<IMechanismParserStrategy, Ip6MechanismParser>()
                .AddTransient<IMechanismParserStrategy, ExistsMechanismParser>()
                .AddTransient<ITermParser, TermParser>()
                .AddTransient<IMechanismParser, MechanismParser>()
                .AddTransient<IQualifierParser, QualifierParser>()
                .AddTransient<IDomainSpecDualCidrBlockMechanismParser, DomainSpecDualCidrBlockMechanismParser>()
                .AddTransient<IModifierParser, ModifierParser>()
                .AddTransient<IModifierParserStrategy, RedirectModifierParser>()
                .AddTransient<IModifierParserStrategy, ExplanationModifierParser>()
                .AddTransient<IDomainSpecParser, DomainSpecParserPassive>()
                .AddTransient<IDualCidrBlockParser, DualCidrBlockParser>()
                .AddTransient<IIp4CidrBlockParser, Ip4CidrBlockParser>()
                .AddTransient<IIp6CidrBlockParser, Ip6CidrBlockParser>()
                .AddTransient<IIp4AddrParser, Ip4AddrParser>()
                .AddTransient<IIp6AddrParser, Ip6AddrParser>()
                .AddTransient<ISpfProcessor, SpfProcessor>()
                .AddTransient<ISpfRecordExpander, SpfRecordExpander>()
                .AddTransient<ISpfTermExpanderStrategy, SpfATermExpander>()
                .AddTransient<ISpfTermExpanderStrategy, SpfExistsTermExpander>()
                .AddTransient<ISpfTermExpanderStrategy, SpfIncludeTermExpander>()
                .AddTransient<ISpfTermExpanderStrategy, SpfRedirectTermExpander>()
                .AddTransient<ISpfTermExpanderStrategy, SpfMxTermExpander>()
                .AddTransient<IHandle<SpfPollPending>, PollHandler>()
                .AddTransient<ISpfPollerConfig, SpfPollerConfig>()
                .AddTransient<IRule<DomainSpfRecords>, OnlyOneSpfRecord>()
                .AddTransient<IRule<DomainSpfRecords>, ShouldBeSmallEnoughForUdp>()
                .AddTransient<IEvaluator<DomainSpfRecords>, Evaluator<DomainSpfRecords>>()
                .AddTransient<IRule<SpfPollResult>, ShouldNotHaveMoreThan10QueryLookups>()
                .AddTransient<IEvaluator<SpfPollResult>, Evaluator<SpfPollResult>>()
                .AddTransient<IEvaluator<DomainSpfRecord>, Evaluator<DomainSpfRecord>>()
                .AddTransient<IRule<DomainSpfRecord>, AllMustBeLastMechanism>()
                .AddTransient<IRule<DomainSpfRecord>, DontUsePtrMechanism>()
                .AddTransient<IRule<DomainSpfRecord>, ExplanationDoesntOccurMoreThanOnce>()
                .AddTransient<IRule<DomainSpfRecord>, RedirectDoesntOccurMoreThanOnce>()
                .AddTransient<IRule<DomainSpfRecord>, ModifiersOccurAfterMechanisms>()
                .AddTransient<IRule<DomainSpfRecord>, RedirectModifierAndAllMechanismNotValid>()
                .AddTransient<IImplicitProvider<Term>, ImplicitProvider<Term>>()
                .AddTransient<IImplicitProviderStrategy<Term>, AllImplicitTermProvider>();
        }

        private static ILookupClient CreateLookupClient(IServiceProvider provider)
        {
            LookupClient lookupClient =  RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new LookupClient(NameServer.GooglePublicDns, NameServer.GooglePublicDnsIPv6)
                {
                    Timeout = provider.GetRequiredService<ISpfPollerConfig>().DnsRecordLookupTimeout
                }
                : new LookupClient(new LookupClientOptions(provider.GetService<IDnsNameServerProvider>()
                    .GetNameServers()
                    .Select(_ => new IPEndPoint(_, 53)).ToArray())
                {
                    ContinueOnEmptyResponse = false,
                    ContinueOnDnsError = false,
                    UseCache = false,
                    UseTcpOnly = true,
                    EnableAuditTrail = true,
                    Retries = 0,
                    Timeout = provider.GetRequiredService<ISpfPollerConfig>().DnsRecordLookupTimeout
                });

            return new AuditTrailLoggingLookupClientWrapper(lookupClient, provider.GetService<IAuditTrailParser>(), provider.GetService<ILogger<AuditTrailLoggingLookupClientWrapper>>());
        }
    }
}

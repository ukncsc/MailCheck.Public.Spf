using System.Collections.Generic;
using System.Threading.Tasks;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Implicit;
using MailCheck.Spf.Poller.Parsing;
using MailCheck.Spf.Poller.Rules;
using MailCheck.Spf.Poller.Rules.Record;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace MailCheck.Spf.Poller.Test.Parsers
{
    [TestFixture(Category = "Integration")]
    public class SpfRecordParserTests
    {
        [Test]
        public async Task SmokeTest()
        {
            ISpfRecordParser parser = CreateParser();

            SpfRecord spfRecord = await parser.Parse("", new List<string>{ "" });
        }

        private ISpfRecordParser CreateParser()
        {
            return new ServiceCollection()
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
                .AddTransient<IEvaluator<DomainSpfRecord>, Evaluator<DomainSpfRecord>>()
                .AddTransient<IRule<DomainSpfRecord>, AllMustBeLastMechanism>()
                .AddTransient<IRule<DomainSpfRecord>, DontUsePtrMechanism>()
                .AddTransient<IRule<DomainSpfRecord>, ExplanationDoesntOccurMoreThanOnce>()
                .AddTransient<IRule<DomainSpfRecord>, RedirectDoesntOccurMoreThanOnce>()
                .AddTransient<IRule<DomainSpfRecord>, ModifiersOccurAfterMechanisms>()
                .AddTransient<IRule<DomainSpfRecord>, RedirectModifierAndAllMechanismNotValid>()
                .AddTransient<IImplicitProvider<Term>, ImplicitProvider<Term>>()
                .AddTransient<IImplicitProviderStrategy<Term>, AllImplicitTermProvider>()
                .BuildServiceProvider()
                .GetRequiredService<ISpfRecordParser>();
        }
    }

}

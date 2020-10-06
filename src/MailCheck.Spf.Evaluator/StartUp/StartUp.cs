using Amazon.SimpleNotificationService;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using MailCheck.Spf.Evaluator.Config;
using MailCheck.Spf.Evaluator.Explainers;
using MailCheck.Spf.Evaluator.Rules;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace MailCheck.Spf.Evaluator.StartUp
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
                .AddTransient<IHandle<SpfRecordsPolled>, EvaluationHandler>()
                .AddTransient<ISpfRecordsJobsProcessor, SpfRecordsDepthFirstJobProcessor>()
                .AddTransient<ISpfEvaluationProcessor, SpfEvaluationProcessor> ()
                .AddTransient<ISpfRecordExplainer, SpfRecordExplainer>()
                .AddTransient<IExplainer<Version>, VersionExplainer>()
                .AddTransient<IExplainer<Term>, Explainer<Term>>()
                .AddTransient<IExplainerStrategy<Term>, AllTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, IncludeTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, ATermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, MxTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, PtrTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, Ip4TermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, Ip6TermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, ExistsTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, RedirectTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, ExplanationTermExplainer>()
                .AddTransient<IExplainerStrategy<Term>, UnknownTermExplainer>()
                .AddTransient<ISpfRecordExplainer, SpfRecordExplainer>()
                .AddTransient<IQualifierExplainer, QualifierExplainer>()
                .AddTransient<IEvaluator<SpfRecord>, Evaluator<SpfRecord>>()
                .AddTransient<IRule<SpfRecord>, ShouldHaveHardFailAllEnabled>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<ISpfEvaluatorConfig, SpfEvaluatorConfig>();
        }
    }
}

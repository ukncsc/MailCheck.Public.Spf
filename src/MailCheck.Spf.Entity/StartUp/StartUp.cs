using Amazon.SimpleNotificationService;
using Amazon.SimpleSystemsManagement;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Implementations;
using MailCheck.Common.Environment.Abstractions;
using MailCheck.Common.Environment.Implementations;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.SSM;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Dao;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Entity.Entity.Notifiers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;
using MailCheck.Spf.Entity.Entity.DomainStatus;
using MailCheck.Common.Processors.Notifiers;
using FindingsChangedNotifier = MailCheck.Common.Processors.Notifiers.FindingsChangedNotifier;
using LocalFindingsChangedNotifier = MailCheck.Spf.Entity.Entity.Notifiers.FindingsChangedNotifier;
using MessageEqualityComparer = MailCheck.Spf.Entity.Entity.Notifiers.MessageEqualityComparer;

namespace MailCheck.Spf.Entity.StartUp
{
    public class StartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigureSerializerSettings();

            services
                .AddTransient<IConnectionInfoAsync, MySqlEnvironmentParameterStoreConnectionInfoAsync>()
                .AddTransient<IEnvironment, EnvironmentWrapper>()
                .AddTransient<IEnvironmentVariables, EnvironmentVariables>()
                .AddSingleton<IAmazonSimpleSystemsManagement, CachingAmazonSimpleSystemsManagementClient>()
                .AddTransient<IAmazonSimpleNotificationService, AmazonSimpleNotificationServiceClient>()
                .AddTransient<IEqualityComparer<Contracts.SharedDomain.Message>, MessageEqualityComparer>()
                .AddTransient<IChangeNotifier, RecordChangeNotifier>()
                .AddTransient<IChangeNotifier, ReferencedRecordChangeNotifier>()
                .AddTransient<IChangeNotifier, RecordMessagesChangeNotifier>()
                .AddTransient<IChangeNotifier, ReferencedRecordMessagesChangeNotifier>()
                .AddTransient<IChangeNotifier, LocalFindingsChangedNotifier>()
                .AddTransient<IFindingsChangedNotifier, FindingsChangedNotifier>()
                .AddTransient<IChangeNotifiersComposite, ChangeNotifiersComposite>()
                .AddTransient<ISpfEntityDao, SpfEntityDao>()
                .AddTransient<ISpfEntityConfig, SpfEntityConfig>()
                .AddTransient<IDomainStatusPublisher, DomainStatusPublisher>()
                .AddTransient<IDomainStatusEvaluator, DomainStatusEvaluator>()
                .AddTransient<SpfEntity>();
        }

        public static void ConfigureSerializerSettings()
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
        }
    }
}

using MailCheck.Common.Data;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Handler;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Spf.Scheduler.StartUp
{
    internal class SpfSqsSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            SpfSchedulerCommonStartUp.ConfigureCommonServices(services);

            services
                .AddTransient<ISpfSchedulerConfig, SpfSchedulerConfig>()
                .AddTransient<SpfSchedulerHandler>()
                .AddTransient<ISpfSchedulerDao, SpfSchedulerDao>()
                .AddSingleton<IDatabase, DefaultDatabase<MySqlProvider>>();
        }
    }
}

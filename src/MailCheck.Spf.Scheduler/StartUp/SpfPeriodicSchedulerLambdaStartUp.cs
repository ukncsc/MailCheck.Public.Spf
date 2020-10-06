﻿using MailCheck.Common.Messaging.Abstractions;
using MailCheck.Common.Messaging.Sns;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Processor;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Spf.Scheduler.StartUp
{
    internal class SpfPeriodicSchedulerLambdaStartUp : IStartUp
    {
        public void ConfigureServices(IServiceCollection services)
        {
            SpfSchedulerCommonStartUp.ConfigureCommonServices(services);

            services
                .AddTransient<ISpfPeriodicSchedulerConfig, SpfPeriodicSchedulerConfig>()
                .AddTransient<IProcess, SpfPollSchedulerProcessor>()
                .AddTransient<ISpfPeriodicSchedulerDao, SpfPeriodicSchedulerDao>()
                .AddTransient<IMessagePublisher, SnsMessagePublisher>();
        }
    }
}

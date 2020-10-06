using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Scheduler.Config
{
    public interface ISpfSchedulerConfig
    {
        string PublisherConnectionString { get; }
    }

    public class SpfSchedulerConfig : ISpfSchedulerConfig
    {
        public SpfSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
        }

        public string PublisherConnectionString { get; }

        public long RefreshIntervalSeconds { get; }
    }
}

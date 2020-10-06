using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Scheduler.Config
{
    public interface ISpfPeriodicSchedulerConfig
    {
        int DomainBatchSize { get; }
        string PublisherConnectionString { get; }
        long RefreshIntervalSeconds { get; }
    }

    public class SpfPeriodicSchedulerConfig : ISpfPeriodicSchedulerConfig
    {
        public SpfPeriodicSchedulerConfig(IEnvironmentVariables environmentVariables)
        {
            DomainBatchSize = environmentVariables.GetAsInt("DomainBatchSize");
            PublisherConnectionString = environmentVariables.Get("SnsTopicArn");
            RefreshIntervalSeconds = environmentVariables.GetAsLong("RefreshIntervalSeconds");
        }

        public int DomainBatchSize { get; }

        public string PublisherConnectionString { get; }

        public long RefreshIntervalSeconds { get; }
    }
}

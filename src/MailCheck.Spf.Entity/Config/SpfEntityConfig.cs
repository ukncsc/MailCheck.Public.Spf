using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Entity.Config
{
    public interface ISpfEntityConfig
    {
        string SnsTopicArn { get; }
    }

    public class SpfEntityConfig : ISpfEntityConfig
    {
        public SpfEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}

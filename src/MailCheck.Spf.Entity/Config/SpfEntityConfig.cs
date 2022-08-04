using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Entity.Config
{
    public interface ISpfEntityConfig
    {
        string SnsTopicArn { get; }
        string WebUrl { get; }
    }

    public class SpfEntityConfig : ISpfEntityConfig
    {
        public SpfEntityConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            WebUrl = environmentVariables.Get("WebUrl");
        }

        public string SnsTopicArn { get; }
        public string WebUrl { get; }
    }
}

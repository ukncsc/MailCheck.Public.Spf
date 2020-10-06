using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Api.Config
{
    public interface ISpfApiConfig
    {
        string SnsTopicArn { get; }
        string MicroserviceOutputSnsTopicArn { get; }
    }

    public class SpfApiConfig : ISpfApiConfig
    {
        public SpfApiConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
            MicroserviceOutputSnsTopicArn = environmentVariables.Get("MicroserviceOutputSnsTopicArn");
        }

        public string SnsTopicArn { get; }
        public string MicroserviceOutputSnsTopicArn { get; }
    }
}

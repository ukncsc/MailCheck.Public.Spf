using MailCheck.Common.Environment.Abstractions;

namespace MailCheck.Spf.Evaluator.Config
{
    public interface ISpfEvaluatorConfig
    {
        string SnsTopicArn { get; }
    }

    public class SpfEvaluatorConfig : ISpfEvaluatorConfig
    {

        public SpfEvaluatorConfig(IEnvironmentVariables environmentVariables)
        {
            SnsTopicArn = environmentVariables.Get("SnsTopicArn");
        }

        public string SnsTopicArn { get; }
    }
}

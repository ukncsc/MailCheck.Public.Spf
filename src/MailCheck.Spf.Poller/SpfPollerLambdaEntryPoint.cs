using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.Sqs;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.Spf.Poller
{
    public class SpfPollerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public SpfPollerLambdaEntryPoint() : base(new StartUp.StartUp())
        {
        }
    }
}

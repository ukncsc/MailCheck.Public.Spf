using Amazon.Lambda.Core;
using MailCheck.Common.Messaging.Sqs;
using MailCheck.Spf.Scheduler.StartUp;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace MailCheck.Spf.Scheduler
{
    public class SpfSqsSchedulerLambdaEntryPoint : SqsTriggeredLambdaEntryPoint
    {
        public SpfSqsSchedulerLambdaEntryPoint()
            : base(new SpfSqsSchedulerLambdaStartUp()) { }
    }
}

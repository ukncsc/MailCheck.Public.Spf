using MailCheck.Common.Messaging.CloudWatch;
using MailCheck.Spf.Scheduler.StartUp;

namespace MailCheck.Spf.Scheduler
{
    public class SpfPeriodicSchedulerLambdaEntryPoint : CloudWatchTriggeredLambdaEntryPoint
    {
        public SpfPeriodicSchedulerLambdaEntryPoint()
            : base(new SpfPeriodicSchedulerLambdaStartUp()) { }
    }
}
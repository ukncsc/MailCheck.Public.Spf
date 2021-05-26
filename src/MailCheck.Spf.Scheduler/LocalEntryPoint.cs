using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Common.Messaging;
using Microsoft.Extensions.CommandLineUtils;
using static Amazon.Lambda.SQSEvents.SQSEvent;

namespace MailCheck.Spf.Scheduler
{
    public static class LocalEntryPoint
    {
        public static void Main(string[] args)
        {
            CommandLineApplication app = new CommandLineApplication(false)
            {
                Name = "SpfScheduler"
            };

            app.Command("lambda-cw", LambdaCloudWatch);
            app.Command("lambda-sqs", LambdaSqs);
            app.Execute(args);
        }

        private static readonly Action<CommandLineApplication> LambdaCloudWatch = command =>
        {
            command.Description = "Execute CloudWatch triggered SPF scheduler lambda code locally.";

            command.OnExecute(async () =>
            {
                SpfPeriodicSchedulerLambdaEntryPoint entryPoint = new SpfPeriodicSchedulerLambdaEntryPoint();

                await entryPoint.FunctionHandler(null, LambdaContext.NonExpiringLambda);

                return 0;
            });
        };

        private static readonly Action<CommandLineApplication> LambdaSqs = command =>
        {
            command.Description = "Execute SQS triggered SPF scheduler lambda code locally.";

            CommandOption queueUrl = command.Option("-q|--queue",
                "The input SQS URL.",
                CommandOptionType.SingleValue);

            command.OnExecute(async () =>
            {
                SpfSqsSchedulerLambdaEntryPoint entryPoint = new SpfSqsSchedulerLambdaEntryPoint();

                ReceiveMessageRequest receiveMessageRequest = new ReceiveMessageRequest(queueUrl.Value())
                {
                    MaxNumberOfMessages = 1,
                    WaitTimeSeconds = 20,
                    MessageAttributeNames = new List<string> { "All" },
                    AttributeNames = new List<string> { "All" },
                };

                while (true)
                {
                    await ProcessMessages(queueUrl, entryPoint, receiveMessageRequest);
                }
            });
        };

        private static async Task ProcessMessages(CommandOption queueUrl,
            SpfSqsSchedulerLambdaEntryPoint entryPoint,
            ReceiveMessageRequest receiveMessageRequest)
        {
            AmazonSQSClient client = new AmazonSQSClient(new EnvironmentVariablesAWSCredentials());

            Console.WriteLine($"Polling {queueUrl.Value()} for messages...");

            ReceiveMessageResponse receiveMessageResponse = await client.ReceiveMessageAsync(receiveMessageRequest);

            Console.WriteLine($"Received {receiveMessageResponse.Messages.Count} messages from {queueUrl.Value()}.");

            if (receiveMessageResponse.Messages.Any())
            {
                try
                {
                    Console.WriteLine($"Running Lambda...");

                    SQSEvent sqsEvent = receiveMessageResponse.Messages.ToSqsEvent();
                    await entryPoint.FunctionHandler(sqsEvent, LambdaContext.NonExpiringLambda);

                    Console.WriteLine($"Lambda completed.");

                    Console.WriteLine($"Deleting messages...");

                    await client.DeleteMessageBatchAsync(queueUrl.Value(),
                        sqsEvent.Records.Select(_ => new DeleteMessageBatchRequestEntry
                        {
                            Id = _.MessageId,
                            ReceiptHandle = _.ReceiptHandle
                        }).ToList());

                    Console.WriteLine($"Messages deleted.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"An error occured running lambda {e.Message} {Environment.NewLine} {e.StackTrace}");
                }
            }
        }

        private static SQSEvent ToSqsEvent(this List<Message> messages)
        {
            return new SQSEvent
            {
                Records = messages.Select(_ => new SQSMessage
                {
                    MessageAttributes = _.MessageAttributes.ToDictionary(a => a.Key,
                        a => new MessageAttribute { StringValue = a.Value.StringValue }),
                    Attributes = _.Attributes,
                    Body = _.Body,
                    MessageId = _.MessageId,
                    ReceiptHandle = _.ReceiptHandle
                }).ToList()
            };
        }
    }
}

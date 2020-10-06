using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Environment = System.Environment;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Poller.Test.ComponentTests
{
    [TestFixture(Category = "Component")]
    public class SpfPollerComponentTests
    {
        private const int ReadTimeoutSecond = 20;

        private const string DomainWithSpf = "ok.spf.testdomains.dev.mailcheck.service.ncsc.gov.uk";
        private const string ExpectedSpfRecord = "v=spf1 include:spf-a.outlook.com include:spf-b.outlook.com ip4:157.55.9.128/25 -all";
        private const int ExpectedRecordSize = 177;

        private const string DomainWithNxDomainResult = "norecord.spf.testdomains.dev.mailcheck.service.ncsc.gov.uk";

        private const string DomainWithoutSpf = "500.uk.com";
        private static readonly string OutputQueueUrl = Environment.GetEnvironmentVariable("TestPollerOutputSqsQueueUrl");
        private static readonly string FunctionName = "TF-test-spf-poller";

        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    TypeNameHandling = TypeNameHandling.Auto
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TermConverter());

                return serializerSetting;
            };
        }

        [Test]
        public async Task PollDomainWithSpfReturnsExpectedResult()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            SpfPollPending spfRecordExpired = new SpfPollPending(DomainWithSpf);

            InvokeLambda(spfRecordExpired);

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Is.TypeOf<SpfRecordsPolled>());

            SpfRecordsPolled spfRecordsPolled = (SpfRecordsPolled)messages[0];
            Assert.That(spfRecordsPolled.Id, Is.EqualTo(spfRecordsPolled.Id));

            Assert.That(spfRecordsPolled.DnsQueryCount, Is.EqualTo(2));
            Assert.That(spfRecordsPolled.ElapsedQueryTime, Is.Not.Null);
            Assert.That(spfRecordsPolled.Records.Records.Count, Is.EqualTo(1));
            Assert.That(spfRecordsPolled.Records.PayloadSizeBytes, Is.EqualTo(ExpectedRecordSize));
            Assert.That(spfRecordsPolled.Records.Records[0].Record, Is.EqualTo(ExpectedSpfRecord));
            Assert.That(spfRecordsPolled.Records.Messages, Is.Empty);
        }

        [Test]
        public async Task PollNxDomainReturnsExpectedResult()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            SpfPollPending spfRecordExpired = new SpfPollPending(DomainWithNxDomainResult);

            InvokeLambda(spfRecordExpired);

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Is.TypeOf<SpfRecordsPolled>());

            SpfRecordsPolled spfRecordsPolled = (SpfRecordsPolled)messages[0];
            Assert.That(spfRecordsPolled.Id, Is.EqualTo(spfRecordsPolled.Id));
            Assert.That(spfRecordsPolled.Records.Records, Is.Empty);
            Assert.That(spfRecordsPolled.DnsQueryCount, Is.Null);
            Assert.That(spfRecordsPolled.ElapsedQueryTime, Is.Null);
            Assert.That(spfRecordsPolled.Messages.Count, Is.EqualTo(1));
            StringAssert.StartsWith("Failed SPF record query for", spfRecordsPolled.Messages[0].Text);
        }

        [Test]
        public async Task PollDomainWithoutSpfReturnsExpectedResult()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            SpfPollPending spfRecordExpired = new SpfPollPending(DomainWithoutSpf);

            InvokeLambda(spfRecordExpired);

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Is.TypeOf<SpfRecordsPolled>());

            SpfRecordsPolled spfRecordsPolled = (SpfRecordsPolled)messages[0];
            Assert.That(spfRecordsPolled.Id, Is.EqualTo(spfRecordsPolled.Id));
            Assert.That(spfRecordsPolled.Records.Records, Is.Empty);
            Assert.That(spfRecordsPolled.DnsQueryCount, Is.EqualTo(0));
            Assert.That(spfRecordsPolled.ElapsedQueryTime, Is.Not.Null);
            Assert.That(spfRecordsPolled.Records.Messages.Count, Is.EqualTo(1));
            StringAssert.StartsWith("500.uk.com: Domain should have exactly 1 SPF record. This domain has 0.", spfRecordsPolled.Records.Messages[0].Text);
        }

        #region Test Support

        private static void InvokeLambda(Message message)
        {
            AmazonLambdaClient client = new AmazonLambdaClient();
            string serializedMessage = JsonConvert.SerializeObject(message);

            SQSEvent.MessageAttribute typeAttribute = new SQSEvent.MessageAttribute
            {
                DataType = "String",
                StringValue = message.GetType().Name
            };

            SQSEvent.SQSMessage fakeSqsMessage = new SQSEvent.SQSMessage
            {
                Body = serializedMessage,
                Attributes = new Dictionary<string, string>
                {
                    { "SentTimestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString() }
                },
                MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                {
                    { "Type", typeAttribute }
                }
            };

            SQSEvent fakeSqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage> { fakeSqsMessage }
            };

            string serializedFakeSqsEvent = JsonConvert.SerializeObject(fakeSqsEvent, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            InvokeRequest invokeRequest = new InvokeRequest
            {
                FunctionName = FunctionName,
                Payload = serializedFakeSqsEvent
            };
            InvokeResponse response = client.InvokeAsync(invokeRequest).GetAwaiter().GetResult();

            string responseString = Encoding.UTF8.GetString(response.Payload.GetBuffer());
            Console.WriteLine(responseString);
        }

        private static async Task PurgeAmazonSqsQueue(string queueUrl)
        {
            using (AmazonSQSClient sqsClient = new AmazonSQSClient())
            {
                ReceiveMessageResponse receiveMessageResponse;
                do
                {
                    receiveMessageResponse =
                        await sqsClient.ReceiveMessageAsync(
                            new ReceiveMessageRequest(queueUrl) { MaxNumberOfMessages = 10 });

                    if (receiveMessageResponse.Messages.Any())
                    {
                        DeleteMessageBatchResponse deleteMessageResponse =
                            await sqsClient.DeleteMessageBatchAsync(new DeleteMessageBatchRequest(queueUrl,
                                receiveMessageResponse.Messages.Select(_ =>
                                    new DeleteMessageBatchRequestEntry(_.MessageId, _.ReceiptHandle)).ToList()));

                    }
                } while (receiveMessageResponse.Messages.Any());
            }
        }

        private static async Task<List<Message>> ReadAmazonSqsEvent(string queueUrl, int waitTimeoutSeconds)
        {
            using (AmazonSQSClient sqsClient = new AmazonSQSClient())
            {
                ReceiveMessageResponse receiveMessageResponse =
                    await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest(queueUrl)
                    {
                        WaitTimeSeconds = waitTimeoutSeconds,
                        MessageAttributeNames = new List<string> { "All" }
                    });

                List<Message> messages = receiveMessageResponse.Messages.Select(GetMessage).ToList();

                if (receiveMessageResponse.Messages.Any())
                {
                    List<DeleteMessageBatchRequestEntry> deleteMessageBatchRequestEntries = receiveMessageResponse
                        .Messages
                        .Select(_ => new DeleteMessageBatchRequestEntry(_.MessageId, _.ReceiptHandle)).ToList();

                    DeleteMessageBatchRequest deleteMessageBatchRequest =
                        new DeleteMessageBatchRequest(queueUrl, deleteMessageBatchRequestEntries);

                    DeleteMessageBatchResponse
                        response = await sqsClient.DeleteMessageBatchAsync(deleteMessageBatchRequest);
                }

                return messages;
            }
        }

        private static Message GetMessage(Amazon.SQS.Model.Message sqsMessage)
        {
            MessageAttributeValue messageAttributeValue = sqsMessage.MessageAttributes["Type"];
            Type type = GetType(messageAttributeValue.StringValue);
            return (Message)JsonConvert.DeserializeObject(sqsMessage.Body, type);
        }

        private static Type GetType(string className)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type assemblyType in assembly.GetTypes())
                {
                    if (assemblyType.Name == className)
                    {
                        return assemblyType;
                    }
                }
            }

            return null;
        }
        #endregion
    }
}

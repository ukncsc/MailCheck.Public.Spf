using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Environment = System.Environment;
using Message = MailCheck.Common.Messaging.Abstractions.Message;

namespace MailCheck.Spf.Evaluator.Test.ComponentTests
{
    [TestFixture(Category = "Component")]
    public class SpfEvaluatorComponentTests
    {
        private const int ReadTimeoutSecond = 20;

        private const string Id = "abc.com";

        private static readonly string FunctionName = "TF-test-spf-evaluator";
        private static readonly string OutputQueueUrl = Environment.GetEnvironmentVariable("TestEvaluatorOutputSqsQueueUrl");

        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    TypeNameHandling = TypeNameHandling.Auto,
                    NullValueHandling = NullValueHandling.Ignore
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TermConverter());

                return serializerSetting;
            };
        }

        [Test]
        public async Task TermsExplained()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            List<Term> terms = new List<Term>
            {
                new All(Qualifier.Fail, "-All", true, false)
            };

            SpfRecords spfRecords = CreateSpfRecords(terms);

            SpfRecordsPolled spfRecordsPolled = new SpfRecordsPolled(Id, spfRecords, 1, TimeSpan.FromSeconds(0),
                new List<Contracts.SharedDomain.Message>());

            InvokeLambda(spfRecordsPolled);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.Not.Null);
            Assert.That(message, Is.TypeOf<SpfRecordsEvaluated>());

            SpfRecordsEvaluated spfRecordsEvaluated = (SpfRecordsEvaluated)message;

            Assert.That(spfRecordsEvaluated.Records.Records.Count, Is.EqualTo(1));
            Assert.That(spfRecordsEvaluated.Records.Records[0].Terms.Count, Is.EqualTo(1));
            StringAssert.StartsWith("Do not allow any (other) ip addresses.", spfRecordsEvaluated.Records.Records[0].Terms[0].Explanation);
        }

        [Test]
        public async Task NonFailAllTermEvaluated()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            List<Term> terms = new List<Term>
            {
                new All(Qualifier.Neutral, "?All", true, false)
            };

            SpfRecords spfRecords = CreateSpfRecords(terms, isRoot: true);

            SpfRecordsPolled spfRecordsPolled = new SpfRecordsPolled(Id, spfRecords, 1, TimeSpan.FromSeconds(0),
                new List<Contracts.SharedDomain.Message>());

            InvokeLambda(spfRecordsPolled);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.Not.Null);
            Assert.That(message, Is.TypeOf<SpfRecordsEvaluated>());

            SpfRecordsEvaluated spfRecordsEvaluated = (SpfRecordsEvaluated)message;

            Assert.That(spfRecordsEvaluated.Records.Records.Count, Is.EqualTo(1));
            Assert.That(spfRecordsEvaluated.Records.Records[0].Messages.Count, Is.EqualTo(1));

            StringAssert.StartsWith("Only \"-all\" (do not allow other ip addresses) or \"~all\" (allow but mark other ip addresses) protect", spfRecordsEvaluated.Records.Records[0].Messages[0].Text);
        }

        [Test]
        public async Task EvaluationOccursForNestedRecords()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);

            List<Term> innerTerms = new List<Term>
            {
                new All(Qualifier.Neutral, "?All", true, false)
            };

            List<Term> terms = new List<Term>
            {
                new Include(Qualifier.Pass, string.Empty, string.Empty, CreateSpfRecords(innerTerms), true)
            };

            SpfRecords spfRecords = CreateSpfRecords(terms);

            SpfRecordsPolled spfRecordsPolled = new SpfRecordsPolled(Id, spfRecords, 1, TimeSpan.FromSeconds(0),
                new List<Contracts.SharedDomain.Message>());
            
            InvokeLambda(spfRecordsPolled);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.Not.Null);
            Assert.That(message, Is.TypeOf<SpfRecordsEvaluated>());

            SpfRecordsEvaluated spfRecordsEvaluated = (SpfRecordsEvaluated)message;

            List<SpfRecord> records = spfRecordsEvaluated.Records.Records;
            Assert.That(records.Count, Is.EqualTo(1));
            Assert.That(records[0].Terms.Count, Is.EqualTo(1));
            Assert.That(records[0].Terms[0].Explanation, Is.Not.Null);

            Assert.That(records[0].Terms[0], Is.TypeOf<Include>());
            List<SpfRecord> innerRecords = ((Include)records[0].Terms[0]).Records.Records;

            Assert.That(innerRecords[0].Terms.Count, Is.EqualTo(1));
            Assert.That(innerRecords[0].Terms[0].Explanation, Is.Not.Null);
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

            string serializedFakeSqsEvent = JsonConvert.SerializeObject(fakeSqsEvent);

            InvokeRequest invokeRequest = new InvokeRequest
            {
                FunctionName = FunctionName,
                Payload = serializedFakeSqsEvent
            };
            InvokeResponse response = client.InvokeAsync(invokeRequest).GetAwaiter().GetResult();

            string responseString = Encoding.UTF8.GetString(response.Payload.GetBuffer());
            Console.WriteLine(responseString);
        }

        private static SpfRecords CreateSpfRecords(List<Term> terms, bool isRoot = false)
        {
            SpfRecords spfRecords = new SpfRecords(new List<SpfRecord>
            {
                new SpfRecord(new List<string> {"one"}, new Contracts.SharedDomain.Version(string.Empty, true), terms, new List<Contracts.SharedDomain.Message>(), isRoot)
            }, 0, new List<Contracts.SharedDomain.Message>());
            return spfRecords;
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

        private static async Task<Message> ReadAmazonSqsEvent(string queueUrl, int waitTimeoutSPerMessageSeconds)
        {
            using (AmazonSQSClient sqsClient = new AmazonSQSClient())
            {
                ReceiveMessageResponse receiveMessageResponse =
                    await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest(queueUrl)
                    {
                        WaitTimeSeconds = waitTimeoutSPerMessageSeconds,
                        MessageAttributeNames = new List<string> { "All" },
                        MaxNumberOfMessages = 1
                    });

                Message message = receiveMessageResponse.Messages.Count == 1
                    ? GetMessage(receiveMessageResponse.Messages.First())
                    : null;

                if (message != null)
                {
                    DeleteMessageResponse response =
                        await sqsClient.DeleteMessageAsync(queueUrl, receiveMessageResponse.Messages.First().ReceiptHandle);
                }

                return message;
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

        #endregion Test Support
    }
}

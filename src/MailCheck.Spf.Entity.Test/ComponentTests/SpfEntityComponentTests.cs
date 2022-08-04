using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Common.Contracts.Messaging;
using MailCheck.Common.Data.Util;
using MailCheck.Spf.Contracts;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.Poller;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Contracts.SharedDomain.Serialization;
using MailCheck.Spf.Entity.Entity;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Environment = System.Environment;
using Message = MailCheck.Common.Messaging.Abstractions.Message;
using MessageAttributeValue = Amazon.SQS.Model.MessageAttributeValue;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;
using Version = MailCheck.Spf.Contracts.SharedDomain.Version;

namespace MailCheck.Spf.Entity.Test.ComponentTests
{
    [TestFixture(Category = "Component")]
    public class SpfEntityComponentTests
    {
        private const int ReadTimeoutSecond = 20;

        private const string Id = "abc.com";

        private static readonly string FunctionName = "TF-test-spf-entity";

        private static readonly string OutputQueueUrl =
            Environment.GetEnvironmentVariable("TestEntityOutputSqsQueueUrl");

        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");

        [SetUp]
        public void SetUp()
        {
            JsonConvert.DefaultSettings = () =>
            {
                JsonSerializerSettings serializerSetting = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                };

                serializerSetting.Converters.Add(new StringEnumConverter());
                serializerSetting.Converters.Add(new TermConverter());

                return serializerSetting;
            };
        }

        [Test]
        public async Task DomainCreatedWithNoDomainCreatesEntityForDomain()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            DomainCreated domainCreated = new DomainCreated(Id, "test@abc.com", DateTime.UtcNow);

            InvokeLambda(domainCreated);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.TypeOf<SpfEntityCreated>());

            SpfEntityCreated spfEntityCreated = (SpfEntityCreated) message;
            Assert.That(spfEntityCreated.Id, Is.EqualTo(domainCreated.Id));
            Assert.That(spfEntityCreated.Version, Is.EqualTo(1));

            List<SpfEntityState> spfEntityStates = await GetStates(ConnectionString);

            Assert.That(spfEntityStates.Count, Is.EqualTo(1));
            Assert.That(spfEntityStates[0].Id, Is.EqualTo(domainCreated.Id));
            Assert.That(spfEntityStates[0].Version, Is.EqualTo(1));
        }

        [Test]
        public async Task DomainAlreadyCreatedNoChangesOccur()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            await SetState(ConnectionString, new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.Now));

            DomainCreated domainCreated = new DomainCreated(Id, "test@abc.com", DateTime.UtcNow);

            InvokeLambda(domainCreated);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.Null);

            List<SpfEntityState> spfEntityStates = await GetStates(ConnectionString);

            Assert.That(spfEntityStates.Count, Is.EqualTo(1));
            Assert.That(spfEntityStates[0].Id, Is.EqualTo(domainCreated.Id));
            Assert.That(spfEntityStates[0].Version, Is.EqualTo(1));
        }

        [Test]
        public async Task SpfRecordsPolledAndRecordsChangedEventsRaisedAndStateUpdated()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            await SetState(ConnectionString, new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.Now));

            SpfRecordsPolled spfRecordsPolled = new SpfRecordsPolled(Id,
                CreateSpfRecords(), 1, TimeSpan.FromSeconds(1), new List<Contracts.SharedDomain.Message>());

            InvokeLambda(spfRecordsPolled);

            Message message1 = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message1, Is.TypeOf<SpfRecordsPolled>());

            SpfRecordsEvaluated spfRecordsEvaluated = (SpfRecordsEvaluated) message1;
            Assert.That(spfRecordsEvaluated.Id, Is.EqualTo(Id));
            Assert.That(spfRecordsEvaluated.Records, Is.EqualTo(spfRecordsPolled.Records));
            Assert.That(spfRecordsEvaluated.DnsQueryCount, Is.EqualTo(spfRecordsPolled.DnsQueryCount));
            Assert.That(spfRecordsEvaluated.ElapsedQueryTime, Is.EqualTo(spfRecordsPolled.ElapsedQueryTime));
            CollectionAssert.AreEqual(spfRecordsEvaluated.Messages, spfRecordsPolled.Messages);

            List<SpfEntityState> spfEntityStates = await GetStates(ConnectionString);

            Assert.That(spfEntityStates.Count, Is.EqualTo(1));
            Assert.That(spfEntityStates[0].Id, Is.EqualTo(Id));
            Assert.That(spfEntityStates[0].Version, Is.EqualTo(2));
            Assert.That(spfEntityStates[0].SpfRecords, Is.EqualTo(spfRecordsPolled.Records));
            Assert.That(spfEntityStates[0].DnsQueryCount, Is.EqualTo(spfRecordsPolled.DnsQueryCount));
            Assert.That(spfEntityStates[0].ElapsedQueryTime, Is.EqualTo(spfRecordsPolled.ElapsedQueryTime));
            CollectionAssert.AreEqual(spfEntityStates[0].Messages, spfRecordsPolled.Messages);
        }

        [Test]
        public async Task SpfRecordsEvaluatedAndRecordsChangedEventsRaisedAndStateUpdated()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            SpfRecords spfRecords = CreateSpfRecords();

            SpfEntityState spfEntityState = new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.Now)
            {
                SpfRecords = spfRecords,
                DnsQueryCount = 1,
                ElapsedQueryTime = TimeSpan.FromSeconds(1),
                Messages = new List<Contracts.SharedDomain.Message>(),
            };

            await SetState(ConnectionString, spfEntityState);

            spfRecords.Records[0].Messages
                .Add(new Contracts.SharedDomain.Message(Guid.Empty, "mailcheck.spf.test", MessageSources.SpfEvaluator, MessageType.error,
                    "EvaluationError", "markdown"));
            spfRecords.Records[0].Terms[0].Explanation = "Explanation";

            SpfRecordsEvaluated spfRecordsEvaluated = new SpfRecordsEvaluated(Id, spfRecords, 1,
                TimeSpan.FromSeconds(0), new List<Contracts.SharedDomain.Message>(), DateTime.UtcNow);

            InvokeLambda(spfRecordsEvaluated);

            Message message = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(message, Is.TypeOf<SpfRecordEvaluationsChanged>());

            SpfRecordEvaluationsChanged spfRecordEvaluationsChanged = message as SpfRecordEvaluationsChanged;

            Assert.That(spfRecordEvaluationsChanged.Records.Records[0].Messages.Count, Is.EqualTo(1));

            Assert.That(spfRecordEvaluationsChanged.Records.Records[0].Messages[0].Text,
                Is.EqualTo(spfRecords.Records[0].Messages[0].Text));

            Assert.That(spfRecordEvaluationsChanged.Records.Records[0].Terms[0].Explanation,
                Is.EqualTo(spfRecords.Records[0].Terms[0].Explanation));

            List<SpfEntityState> spfEntityStates = await GetStates(ConnectionString);

            Assert.That(spfEntityStates.Count, Is.EqualTo(1));
            Assert.That(spfEntityStates[0].Id, Is.EqualTo(Id));
            Assert.That(spfEntityStates[0].Version, Is.EqualTo(2));

            Assert.That(spfEntityStates[0].SpfRecords.Records[0].Messages.Count, Is.EqualTo(1));

            Assert.That(spfEntityStates[0].SpfRecords.Records[0].Messages[0].Text,
                Is.EqualTo(spfRecordsEvaluated.Records.Records[0].Messages[0].Text));

            Assert.That(spfEntityStates[0].SpfRecords.Records[0].Terms[0].Explanation,
                Is.EqualTo(spfRecordsEvaluated.Records.Records[0].Terms[0].Explanation));
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
                    {"SentTimestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()}
                },
                MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                {
                    {"Type", typeAttribute}
                }
            };

            SQSEvent fakeSqsEvent = new SQSEvent
            {
                Records = new List<SQSEvent.SQSMessage> {fakeSqsMessage}
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
                            new ReceiveMessageRequest(queueUrl) {MaxNumberOfMessages = 10});

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
                        MessageAttributeNames = new List<string> {"All"},
                        MaxNumberOfMessages = 1
                    });

                Message message = receiveMessageResponse.Messages.Count == 1
                    ? GetMessage(receiveMessageResponse.Messages.First())
                    : null;

                if (message != null)
                {
                    DeleteMessageResponse response =
                        await sqsClient.DeleteMessageAsync(queueUrl,
                            receiveMessageResponse.Messages.First().ReceiptHandle);
                }

                return message;
            }
        }

        private static Message GetMessage(Amazon.SQS.Model.Message sqsMessage)
        {
            MessageAttributeValue messageAttributeValue = sqsMessage.MessageAttributes["Type"];
            Type type = GetType(messageAttributeValue.StringValue);
            return (Message) JsonConvert.DeserializeObject(sqsMessage.Body, type);
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

        private static async Task<List<SpfEntityState>> GetStates(string connectionString)
        {
            List<SpfEntityState> results = new List<SpfEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(connectionString,
                "SELECT state FROM spf_entity ORDER BY version DESC;"))
            {
                while (await reader.ReadAsync())
                {
                    results.Add(CreateSpfEntityState(reader));
                }
            }

            return results;
        }

        private static SpfEntityState CreateSpfEntityState(DbDataReader reader)
        {
            return JsonConvert.DeserializeObject<SpfEntityState>(reader.GetString("state"));
        }

        private static async Task SetState(string connectionString, SpfEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(connectionString,
                "INSERT INTO `spf_entity`(`id`,`version`,`state`) VALUES (@domain, @version, @state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private static async Task TruncateDatabase(string connectionString)
        {
            await MySqlHelper.ExecuteNonQueryAsync(connectionString, "DELETE FROM spf_entity;");
        }

        private static SpfRecords CreateSpfRecords()
        {
            return new SpfRecords(new List<SpfRecord>
                {
                    new SpfRecord(
                        new List<string>
                        {
                            "v=spf1......"
                        },
                        new Version("1", true),
                        new List<Term>
                        {
                            new All(Qualifier.Fail, "", true, false)
                        },
                        new List<Contracts.SharedDomain.Message>(),
                        false)
                }, 100,
                new List<Contracts.SharedDomain.Message>());
        }

        #endregion
    }
}
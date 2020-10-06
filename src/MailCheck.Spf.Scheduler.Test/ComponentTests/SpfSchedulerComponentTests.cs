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
using MailCheck.Common.Data.Util;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Contracts.Scheduler;
using MailCheck.Spf.Scheduler.Dao.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Environment = System.Environment;
using Message = MailCheck.Common.Messaging.Abstractions.Message;
using MessageAttributeValue = Amazon.SQS.Model.MessageAttributeValue;

namespace MailCheck.Spf.Scheduler.Test.ComponentTests
{
    [TestFixture(Category = "Component")]
    public class SpfSchedulerComponentTests
    {
        private const int ReadTimeoutSecond = 20;

        private static readonly string FunctionName = "TF-test-spf-scheduler-sqs";
        private static readonly string OutputQueueUrl = Environment.GetEnvironmentVariable("TestSchedulerOutputSqsQueueUrl");
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");

        [Test]
        public async Task ItShouldRaiseARecordMessageAndSaveStateForNewlyCreatedEntities()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            SpfEntityCreated entityCreated = new SpfEntityCreated("ncsc.gov.uk", 1);

            InvokeLambda(entityCreated);

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Is.TypeOf<SpfRecordExpired>());

            SpfRecordExpired expiredMessage = (SpfRecordExpired)messages[0];
            Assert.That(expiredMessage.Id, Is.EqualTo(entityCreated.Id));

            List<SpfSchedulerState> states = await GetStates(ConnectionString);

            Assert.That(states.Count, Is.EqualTo(1));
            Assert.That(states[0].Id, Is.EqualTo(expiredMessage.Id));
        }

        [Test]
        public async Task ItShouldNotRaiseAnEventForExistingState()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);
            SpfEntityCreated entityCreated = new SpfEntityCreated("ncsc.gov.uk", 1);
            
            InvokeLambda(entityCreated);

            List<Message> firstRead = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);
            Assert.AreEqual(firstRead.Count, 1);

            InvokeLambda(entityCreated);

            List<Message> secondRead = await ReadAmazonSqsEvent(OutputQueueUrl, 5);

            Assert.AreEqual(secondRead.Count, 0);

            List<SpfSchedulerState> states = await GetStates(ConnectionString);

            Assert.That(states.Count, Is.EqualTo(1));
            Assert.That(states[0].Id, Is.EqualTo(entityCreated.Id));
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

                    DeleteMessageBatchResponse response =
                        await sqsClient.DeleteMessageBatchAsync(deleteMessageBatchRequest);
                }

                return messages;
            }
        }

        private static Message GetMessage(Amazon.SQS.Model.Message sqsMessage)
        {
            MessageAttributeValue messageAttributeValue = sqsMessage.MessageAttributes["Type"];
            Type type = GetType(messageAttributeValue.StringValue);
            return (Message)JsonConvert.DeserializeObject(sqsMessage.Body, type,
                new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
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

        private static SpfSchedulerState CreateSpfSchedulerState(DbDataReader reader)
        {
            return new SpfSchedulerState(reader.GetString("id"));
        }

        private static async Task<List<SpfSchedulerState>> GetStates(string connectionString)
        {
            List<SpfSchedulerState> results = new List<SpfSchedulerState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString, "SELECT id FROM spf_scheduled_records"))
            {
                while (await reader.ReadAsync())
                {
                    results.Add(CreateSpfSchedulerState(reader));
                }
            }

            return results;
        }

        private static Task TruncateDatabase(string connectionString) =>
            MySqlHelper.ExecuteNonQueryAsync(connectionString, "DELETE FROM spf_scheduled_records;");

        #endregion
    }
}

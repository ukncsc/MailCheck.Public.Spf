using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.Lambda;
using Amazon.Lambda.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using MailCheck.Spf.Contracts.Scheduler;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NUnit.Framework;
using Environment = System.Environment;
using Message = MailCheck.Common.Messaging.Abstractions.Message;
using MessageAttributeValue = Amazon.SQS.Model.MessageAttributeValue;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Scheduler.Test.ComponentTests
{
    [TestFixture(Category = "Component")]
    public class SpfPeriodicSchedulerComponentTests
    {
        private const int ReadTimeoutSecond = 20;

        private static readonly string LambdaFunctionName = Environment.GetEnvironmentVariable("PeriodicSchedulerLambdaFunctionName");
        private static readonly string OutputQueueUrl = Environment.GetEnvironmentVariable("TestSchedulerOutputSqsQueueUrl");
        private static readonly string ConnectionString = Environment.GetEnvironmentVariable("ConnectionString");

        [Test]
        public async Task ItShouldRaiseEventsWhenThereAreExpiredRecords()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            DateTime expiredDate = DateTime.UtcNow.Subtract(TimeSpan.FromHours(25));
            await Insert("ncsc.gov.uk", expiredDate);

            await TriggerLambda(LambdaFunctionName);

            DateTime updatedLastChecked = await GetLastChecked("ncsc.gov.uk");
            Assert.That(updatedLastChecked, Is.GreaterThan(expiredDate));

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(1));
            Assert.That(messages[0], Is.TypeOf<SpfRecordExpired>());

            SpfRecordExpired expiredMessage = (SpfRecordExpired)messages[0];
            Assert.That(expiredMessage.Id, Is.EqualTo("ncsc.gov.uk"));
        }

        [Test]
        public async Task ItShouldNotRaiseAnEventForExisitingState()
        {
            await PurgeAmazonSqsQueue(OutputQueueUrl);
            await TruncateDatabase(ConnectionString);

            await TriggerLambda(LambdaFunctionName);

            List<Message> messages = await ReadAmazonSqsEvent(OutputQueueUrl, ReadTimeoutSecond);

            Assert.That(messages.Count, Is.EqualTo(0));
        }

        #region Test Support

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

        private Task TriggerLambda(string functionName)
        {
            using (AmazonLambdaClient client = new AmazonLambdaClient())
            {
                return client.InvokeAsync(new InvokeRequest { FunctionName = functionName });
            }
        }

        private Task Insert(string domain, DateTime lastChecked) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO spf_scheduled_records (id, last_checked) VALUES (@domain, @last_checked)",
                new MySqlParameter("domain", domain),
                new MySqlParameter("last_checked", lastChecked));

        private async Task<DateTime> GetLastChecked(string domain) =>
            (DateTime)await MySqlHelper.ExecuteScalarAsync(ConnectionString,
                $"SELECT last_checked FROM spf_scheduled_records WHERE id = @domain",
                new MySqlParameter("domain", domain));

        private static Task TruncateDatabase(string connectionString) =>
            MySqlHelper.ExecuteNonQueryAsync(connectionString, "DELETE FROM spf_scheduled_records;");

        #endregion
    }
}

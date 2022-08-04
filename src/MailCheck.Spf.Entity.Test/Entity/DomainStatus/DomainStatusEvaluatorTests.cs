using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using FakeItEasy;
using MailCheck.Common.Messaging.Abstractions;
using MailCheck.DomainStatus.Contracts;
using MailCheck.Spf.Contracts.Evaluator;
using MailCheck.Spf.Contracts.SharedDomain;
using MailCheck.Spf.Entity.Config;
using MailCheck.Spf.Entity.Entity.DomainStatus;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using A = FakeItEasy.A;
using Message = MailCheck.Spf.Contracts.SharedDomain.Message;

namespace MailCheck.Spf.Entity.Test.Entity.DomainStatus
{
    [TestFixture]
    public class DomainStatusEvaluatorTests
    {
        private DomainStatusEvaluator _domainStatusEvaluator;
        
        [SetUp]
        public void SetUp()
        {
            _domainStatusEvaluator = new DomainStatusEvaluator();
        }

        [TestCase(Status.Success, null)]
        [TestCase(Status.Success, new MessageType[]{})]
        [TestCase(Status.Info, new[] { MessageType.info, MessageType.info })]
        [TestCase(Status.Warning, new[] { MessageType.info, MessageType.warning })]
        [TestCase(Status.Warning, new[] { MessageType.warning, MessageType.warning })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.warning, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.error, MessageType.error })]
        [TestCase(Status.Error, new[] { MessageType.info, MessageType.warning, MessageType.error })]
        public void StatusIsDeterminedAndDispatched(Status expectedStatus, MessageType[] messageTypes)
        {
            List<Message> messages = messageTypes?.Select(CreateMessage).ToList();

            Status result = _domainStatusEvaluator.GetStatus(messages);

            Assert.AreEqual(result, expectedStatus);
        }

        private Message CreateMessage(MessageType messageType)
        {
            return new Message(Guid.Empty, "mailcheck.spf.test", null, messageType, "", "");
        }
    }
}

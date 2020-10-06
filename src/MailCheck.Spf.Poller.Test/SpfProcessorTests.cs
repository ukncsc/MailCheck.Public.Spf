using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Spf.Poller.Config;
using MailCheck.Spf.Poller.Dns;
using MailCheck.Spf.Poller.Domain;
using MailCheck.Spf.Poller.Exception;
using MailCheck.Spf.Poller.Expansion;
using MailCheck.Spf.Poller.Parsing;
using MailCheck.Spf.Poller.Rules;
using NUnit.Framework;
using A = FakeItEasy.A;

namespace MailCheck.Spf.Poller.Test
{
    [TestFixture]
    public class SpfProcessorTests
    {
        private IDnsClient _dnsClient;
        private ISpfRecordsParser _spfRecordsParser;
        private ISpfRecordExpander _spfRecordExpander;
        private ISpfProcessor _spfProcessor;
        private IEvaluator<SpfPollResult> _evaluator;
        private ISpfPollerConfig _config;

        [SetUp]
        public void SetUp()
        {
            _dnsClient = A.Fake<IDnsClient>();
            _spfRecordsParser = A.Fake<ISpfRecordsParser>();
            _spfRecordExpander = A.Fake<ISpfRecordExpander>();
            _evaluator = A.Fake<IEvaluator<SpfPollResult>>();
            _config = A.Fake<ISpfPollerConfig>();


            _spfProcessor = new SpfProcessor(_dnsClient, _spfRecordsParser, _spfRecordExpander, _evaluator, _config);
        }

        [Test]
        public async Task SpfExceptionThrownWhenAllowNullResultsNotSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(false);

            Assert.Throws<SpfPollerException>(() => _spfProcessor.Process(domain).GetAwaiter().GetResult());
        }

        [Test]
        public async Task SpfExceptionNotThrownWhenAllowNullResultsSetAndEmptyResult()
        {
            string domain = "abc.com";

            A.CallTo(() => _config.AllowNullResults).Returns(true);

            SpfPollResult result = await _spfProcessor.Process(domain);

            Assert.AreEqual(0, result.Records.Records.Count);
        }
    }
}

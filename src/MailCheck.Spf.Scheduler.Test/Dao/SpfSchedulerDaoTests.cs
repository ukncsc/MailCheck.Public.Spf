using System;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Spf.Migration;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class SpfSchedulerDaoTests : DatabaseTestBase
    {
        private SpfSchedulerDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();

            IConnectionInfoAsync connectionInfo = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfo.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new SpfSchedulerDao(connectionInfo);
        }

        [Test]
        public async Task ItShouldReturnNullIfTheStateDoesntExist()
        {
            SpfSchedulerState state = await _dao.Get("ncsc.gov.uk");
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task ItShouldGetTheStateIfItExists()
        {
            await Insert("ncsc.gov.uk");

            SpfSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldSaveTheStateIfItDoesNotExist()
        {
            await _dao.Save(new SpfSchedulerState("ncsc.gov.uk"));

            await _dao.Get("ncsc.gov.uk");

            SpfSchedulerState state = await _dao.Get("ncsc.gov.uk");

            Assert.AreEqual(state.Id, "ncsc.gov.uk");
        }

        [Test]
        public async Task ItShouldThrowAnExceptionIfTheStateAlreadyExists()
        {
            await Insert("ncsc.gov.uk");

            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(new SpfSchedulerState("ncsc.gov.uk")));
        }

        protected override string GetDatabaseName() => "spf";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO spf_scheduled_records (id, last_checked) VALUES (@domain, UTC_TIMESTAMP())",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM spf_scheduled_records;");
    }
}

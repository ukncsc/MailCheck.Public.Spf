using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Common.Util;
using MailCheck.Spf.Migration;
using MailCheck.Spf.Scheduler.Config;
using MailCheck.Spf.Scheduler.Dao;
using MailCheck.Spf.Scheduler.Dao.Model;
using MySql.Data.MySqlClient;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.Scheduler.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class SpfPeriodicSchedulerDaoTests : DatabaseTestBase
    {
        private SpfPeriodicSchedulerDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            TruncateDatabase().Wait();

            ISpfPeriodicSchedulerConfig config = A.Fake<ISpfPeriodicSchedulerConfig>();
            A.CallTo(() => config.RefreshIntervalSeconds).Returns(0);
            A.CallTo(() => config.DomainBatchSize).Returns(10);

            IDatabase database = A.Fake<IDatabase>();
            IClock clock = A.Fake<IClock>();

            _dao = new SpfPeriodicSchedulerDao(database, config, clock);
        }

        [Test]
        public async Task ItShouldReturnNothingIfThereAreNoExpiredRecords()
        {
            List<SpfSchedulerState> states = await _dao.GetExpiredSpfRecords();
            Assert.That(states.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task ItShouldReturnAllExpiredRecords()
        {
            await Insert("ncsc.gov.uk", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1)));

            List<SpfSchedulerState> state = await _dao.GetExpiredSpfRecords();

            Assert.That(state[0].Id, Is.EqualTo("ncsc.gov.uk"));
        }

        [Test]
        public async Task ItShouldUpdateTheLastCheckedTime()
        {
            await Insert("ncsc.gov.uk", DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(1)));

            DateTime current = await GetLastChecked("ncsc.gov.uk");

            await _dao.UpdateLastChecked(new List<SpfSchedulerState> { new SpfSchedulerState("ncsc.gov.uk") });

            DateTime updated = await GetLastChecked("ncsc.gov.uk");

            Assert.That(updated, Is.GreaterThan(current));
        }

        protected override string GetDatabaseName() => "spf";

        protected override Assembly GetSchemaAssembly() => Assembly.GetAssembly(typeof(Migrator));

        private Task Insert(string domain, DateTime lastChecked) =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO spf_scheduled_records (id, last_checked) VALUES (@domain, @last_checked)",
                new MySqlParameter("domain", domain),
                new MySqlParameter("last_checked", lastChecked));

        private async Task<DateTime> GetLastChecked(string domain) =>
            (DateTime)await MySqlHelper.ExecuteScalarAsync(ConnectionString,
                $"SELECT last_checked FROM spf_scheduled_records WHERE id = @domain",
                new MySqlParameter("domain", domain));

        private Task TruncateDatabase() =>
            MySqlHelper.ExecuteNonQueryAsync(ConnectionString, "DELETE FROM spf_scheduled_records;");

    }
}

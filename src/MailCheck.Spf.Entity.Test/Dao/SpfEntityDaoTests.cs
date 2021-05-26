using System;
using System.Data.Common;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.TestSupport;
using MailCheck.Spf.Contracts.Entity;
using MailCheck.Spf.Entity.Dao;
using MailCheck.Spf.Entity.Entity;
using MailCheck.Spf.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MailCheck.Common.Data.Util;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;
using Microsoft.Extensions.Logging;

namespace MailCheck.Spf.Entity.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class SpfEntityDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private SpfEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            ILogger<SpfEntityDao> logger = A.Fake<ILogger<SpfEntityDao>>();

            _dao = new SpfEntityDao(connectionInfoAsync, logger);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            SpfEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            SpfEntityState state = new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.UtcNow);

            await Insert(state);

            SpfEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.Version, Is.EqualTo(state.Version));
        }

        [Test]
        public async Task SaveDuplicateEntryThrows()
        {
            SpfEntityState state1 = new SpfEntityState(Id, 1, SpfState.PollPending, DateTime.UtcNow);

            await _dao.Save(state1);
            Assert.ThrowsAsync<InvalidOperationException>(() => _dao.Save(state1));
        }

        
        protected override string GetDatabaseName() => "spfentity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(SpfEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `spf_entity`(`id`,`version`,`state`)VALUES(@domain,@version,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("version", state.Version),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private SpfEntityState CreateSpfEntityState(DbDataReader reader)
        {
            return JsonConvert.DeserializeObject<SpfEntityState>(reader.GetString("state"));
        }

        #endregion
    }
}

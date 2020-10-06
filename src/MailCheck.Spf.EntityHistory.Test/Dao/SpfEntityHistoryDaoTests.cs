using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using FakeItEasy;
using MailCheck.Common.Data.Abstractions;
using MailCheck.Common.Data.Util;
using MailCheck.Common.TestSupport;
using MailCheck.Spf.EntityHistory.Dao;
using MailCheck.Spf.EntityHistory.Entity;
using MailCheck.Spf.Migration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using NUnit.Framework;
using MySqlHelper = MailCheck.Common.Data.Util.MySqlHelper;

namespace MailCheck.Spf.EntityHistory.Test.Dao
{
    [TestFixture(Category = "Integration")]
    public class SpfEntityHistoryDaoTests : DatabaseTestBase
    {
        private const string Id = "abc.com";

        private ISpfHistoryEntityDao _dao;

        [SetUp]
        protected override void SetUp()
        {
            base.SetUp();

            IConnectionInfoAsync connectionInfoAsync = A.Fake<IConnectionInfoAsync>();
            A.CallTo(() => connectionInfoAsync.GetConnectionStringAsync()).Returns(ConnectionString);

            _dao = new SpfHistoryEntityDao(connectionInfoAsync);
        }

        [Test]
        public async Task GetNoStateExistsReturnsNull()
        {
            SpfHistoryEntityState state = await _dao.Get(Id);
            Assert.That(state, Is.Null);
        }

        [Test]
        public async Task GetStateExistsReturnsState()
        {
            string spfRecord = "spfRecord1";

            SpfHistoryEntityState state = new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord> { new SpfHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { spfRecord }) });
            
            await Insert(state);

            SpfHistoryEntityState stateFromDatabase = await _dao.Get(Id);

            Assert.That(stateFromDatabase.Id, Is.EqualTo(state.Id));
            Assert.That(stateFromDatabase.SpfHistory.Count, Is.EqualTo(state.SpfHistory.Count));
            Assert.That(stateFromDatabase.SpfHistory[0].SpfRecords.Count, Is.EqualTo(state.SpfHistory[0].SpfRecords.Count));
            Assert.That(stateFromDatabase.SpfHistory[0].SpfRecords[0], Is.EqualTo(state.SpfHistory[0].SpfRecords[0]));
        }

        [Test]
        public async Task HistoryIsSavedForChanges()
        {
            string spfRecord1= "spfRecord1";
            string spfRecord2 = "spfRecord2";

            SpfHistoryEntityState state = new SpfHistoryEntityState(Id,
                new List<SpfHistoryRecord> { new SpfHistoryRecord(DateTime.UtcNow.AddDays(-1), null, new List<string> { spfRecord1 }) });

            await _dao.Save(state);

            SpfHistoryEntityState state2 = (await SelectAllHistory(Id)).First();
            state2.SpfHistory[0].EndDate = DateTime.UtcNow;
            state2.SpfHistory.Insert(0, new SpfHistoryRecord(DateTime.UtcNow, null, new List<string> { spfRecord2 }));

            await _dao.Save(state2);

            List<SpfHistoryEntityState> historyStates = await SelectAllHistory(Id);
            Assert.That(historyStates[0].SpfHistory.Count, Is.EqualTo(2));

            Assert.That(historyStates[0].SpfHistory[0].EndDate, Is.Null);
            Assert.That(historyStates[0].SpfHistory[0].SpfRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].SpfHistory[0].SpfRecords[0], Is.EqualTo(spfRecord2));

            Assert.That(historyStates[0].SpfHistory[1].EndDate, Is.Not.Null);
            Assert.That(historyStates[0].SpfHistory[1].SpfRecords.Count, Is.EqualTo(1));
            Assert.That(historyStates[0].SpfHistory[1].SpfRecords[0], Is.EqualTo(spfRecord1));
        }

        protected override string GetDatabaseName() => "spfHistoryEntity";

        protected override Assembly GetSchemaAssembly()
        {
            return Assembly.GetAssembly(typeof(Migrator));
        }

        #region TestSupport

        private async Task Insert(SpfHistoryEntityState state)
        {
            await MySqlHelper.ExecuteNonQueryAsync(ConnectionString,
                @"INSERT INTO `spf_entity_history`(`id`,`state`)VALUES(@domain,@state)",
                new MySqlParameter("domain", state.Id),
                new MySqlParameter("state", JsonConvert.SerializeObject(state)));
        }

        private async Task<List<SpfHistoryEntityState>> SelectAllHistory(string id)
        {
            List<SpfHistoryEntityState> list = new List<SpfHistoryEntityState>();

            using (DbDataReader reader = await MySqlHelper.ExecuteReaderAsync(ConnectionString,
                @"SELECT state FROM spf_entity_history WHERE id = @domain ORDER BY id;",
                new MySqlParameter("domain", id)))
            {
                while (reader.Read())
                {
                    string state = reader.GetString("state");

                    if (!string.IsNullOrWhiteSpace(state))
                    {
                        list.Add(JsonConvert.DeserializeObject<SpfHistoryEntityState>(state));
                    }
                }
            }

            return list;
        }

        #endregion
    }
}

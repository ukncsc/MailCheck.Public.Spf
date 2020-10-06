using MailCheck.Common.Data.Implementations;
using Microsoft.Extensions.DependencyInjection;

namespace MailCheck.Spf.Entity.Seeding.History
{
    public class HistoryMigratorFactory
    {
        public static IHistoryMigrator Create(string dmarcConnectionString, string spfConnectionString)
        {
            return new ServiceCollection()
                .AddTransient<IHistoryReaderDao>(_ => new HistoryReaderDao(new StringConnectionInfo(dmarcConnectionString)))
                .AddTransient<IHistoryWriterDao>(_ => new HistoryWriterDao(new StringConnectionInfo(spfConnectionString)))
                .AddTransient<IHistoryMigrator, HistoryMigrator>()
                .BuildServiceProvider()
                .GetRequiredService<IHistoryMigrator>();
        }
    }
}

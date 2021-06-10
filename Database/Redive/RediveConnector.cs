using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;


namespace PriconneBotConsoleApp.Database
{
    public class RediveConnector : DbContext
    {
        public DbSet<CampaignData> CampaignData { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            var connectionString =
                new SqliteConnectionStringBuilder { DataSource = @"./data/redive_jp.db" }.ToString();
            optionBuilder.UseSqlite(new SqliteConnection(connectionString));
        }
    }
}

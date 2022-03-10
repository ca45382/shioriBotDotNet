using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


namespace ShioriBot.Model
{
    public class RediveDBContext : DbContext
    {
        public DbSet<CampaignData> CampaignData { get; set; }
        public DbSet<ClanBattleSchedule> ClanBattleSchedule { get; set; }
        public DbSet<ClanBattleData> ClanBattleData { get; set; }
        public DbSet<WaveGroupData> WaveGroupData { get; set; }
        public DbSet<EnemyParameter> EnemyParameter { get; set; }
        public DbSet<ClanBattlePeriod> ClanBattlePeriod { get; set; }
        public DbSet<ClanBattleTrainingSchedule> ClanBattleTrainingSchedule { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionBuilder)
        {
            var connectionString =
                new SqliteConnectionStringBuilder { DataSource = @"./data/redive_jp.db" }.ToString();
            optionBuilder.UseSqlite(new SqliteConnection(connectionString));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.Script;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseConnector : DbContext
    {
        public DbSet<ServerData> ServerData { get; set; }
        public DbSet<ClanData> ClanData { get; set; }

        //クラン詳細情報
        public DbSet<ChannelData> ChannelData { get; set; }
        public DbSet<MessageData> MessageData { get; set; }
        public DbSet<RoleData> RoleData { get; set; }

        // 機能情報
        public DbSet<ChannelFeature> ChannelFeatures { get; set; }
        public DbSet<MessageFeature> MessageFeatures { get; set; }
        public DbSet<RoleFeature> RoleFeatures { get; set; }

        // プレイヤーデータ
        public DbSet<PlayerData> PlayerData { get; set; }
        public DbSet<ReservationData> ReservationData { get; set; }
        public DbSet<DeclarationData> DeclarationData { get; set; }
        
        public static readonly JsonDataManager JsonData = new JsonDataManager();

        // TODO: 動的に取得する
        public static readonly MariaDbServerVersion ServerVersion = new MariaDbServerVersion(new Version(10, 3, 27));

        public DatabaseConnector()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(JsonData.MySQLConnectionString, ServerVersion);

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClanData>()
                .HasOne(b => b.ServerData)
                .WithMany(i => i.ClanData)
                .HasForeignKey(b => b.ServerID);

            // クラン情報に関するデータへのリレーション
            modelBuilder.Entity<ChannelData>()
                .HasOne(b => b.ClanData)
                .WithMany(i => i.ChannelData)
                .HasForeignKey(b => b.ClanID);

            modelBuilder.Entity<MessageData>()
                .HasOne(b => b.ClanData)
                .WithMany(i => i.MessageData)
                .HasForeignKey(b => b.ClanID);

            modelBuilder.Entity<RoleData>()
                .HasOne(b => b.ClanData)
                .WithMany(i => i.roleData)
                .HasForeignKey(b => b.ClanID);

            // 機能情報とのリレーション
            modelBuilder.Entity<ChannelData>()
                .HasOne(x => x.ChannelFeature)
                .WithMany(x => x.ChannelData)
                .HasForeignKey(x => x.FeatureID);

            modelBuilder.Entity<MessageData>()
                .HasOne(x => x.MessageFeature)
                .WithMany(x => x.MessageData)
                .HasForeignKey(x => x.FeatureID);

            modelBuilder.Entity<RoleData>()
                .HasOne(x => x.RoleFeature)
                .WithMany(x => x.RoleData)
                .HasForeignKey(x => x.FeatureID);

            // プレイヤーデータへのリレーション
            modelBuilder.Entity<PlayerData>()
                .HasOne(b => b.ClanData)
                .WithMany(i => i.PlayerData)
                .HasForeignKey(b => b.ClanID);

            // データとプレイヤーデータへのリレーション
            modelBuilder.Entity<ReservationData>()
                .HasOne(b => b.PlayerData)
                .WithMany(i => i.ReservationData)
                .HasForeignKey(b => b.PlayerID);

            modelBuilder.Entity<DeclarationData>()
                .HasOne(b => b.PlayerData)
                .WithMany(i => i.DeclarationData)
                .HasForeignKey(b => b.PlayerID);
        }
    }
}

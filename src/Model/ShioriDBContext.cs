using System;
using Microsoft.EntityFrameworkCore;
using ShioriBot.Net.Script;

namespace ShioriBot.Net.Model
{
    public class ShioriDBContext : DbContext
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

        // クラバト機能保存
        public DbSet<ReservationData> ReservationData { get; set; }
        public DbSet<DeclarationData> DeclarationData { get; set; }
        public DbSet<ReportData> ReportData { get; set; }
        public DbSet<ProgressData> ProgressData { get; set; }
        public DbSet<CarryOverData> CarryOverData { get; set; }
        public DbSet<TaskKillData> TaskKillData { get; set; }

        // TODO: 動的に取得する
        public static readonly MariaDbServerVersion ServerVersion = new MariaDbServerVersion(new Version(10, 3, 27));

        public ShioriDBContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseMySql(BotConfigManager.SQLConnectionString, ServerVersion);

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
                .WithMany(i => i.RoleData)
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

            modelBuilder.Entity<ReportData>()
                .HasOne(x => x.PlayerData)
                .WithMany(x => x.ReportData)
                .HasForeignKey(x => x.PlayerID);

            modelBuilder.Entity<ProgressData>()
                .HasOne(x => x.PlayerData)
                .WithMany(x => x.ProgressData)
                .HasForeignKey(x => x.PlayerID);

            modelBuilder.Entity<CarryOverData>()
                .HasOne(x => x.PlayerData)
                .WithMany(x => x.CarryOverData)
                .HasForeignKey(x => x.PlayerID);

            modelBuilder.Entity<TaskKillData>()
                .HasOne(x => x.PlayerData)
                .WithMany(x => x.TaskKillData)
                .HasForeignKey(x => x.PlayerID);
        }
    }
}

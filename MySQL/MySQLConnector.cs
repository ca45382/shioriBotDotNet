﻿using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataTypes;
using PriconneBotConsoleApp.Script.Initialize;
using System;

namespace PriconneBotConsoleApp.MySQL
{
    class MySQLConnector : DbContext
    {
        public DbSet<ServerData> ServerData { get; set; }
        public DbSet<ClanData> ClanData { get; set; }

        public DbSet<MessageIDs> MessageIDs { get; set; }
        public DbSet<PlayerData> PlayerData { get; set; }
        public DbSet<ReservationData> ReservationData { get; set; }
        public DbSet<DeclarationData> DeclarationData { get; set; }
        
        public static readonly JsonDataManager JsonData = new JsonDataManager();

        // TODO: 動的に取得する
        public static readonly MariaDbServerVersion ServerVersion = new MariaDbServerVersion(new Version(10, 3, 27));

        public MySQLConnector()
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

            modelBuilder.Entity<ClanData>()
                .HasOne(b => b.ChannelIDs)
                .WithOne(i => i.ClanData)
                .HasForeignKey<ChannelIDs>(b => b.ClanID);

            modelBuilder.Entity<ClanData>()
                .HasOne(b => b.MessageIDs)
                .WithOne(i => i.ClanData)
                .HasForeignKey<MessageIDs>(b => b.ClanID);

            modelBuilder.Entity<ClanData>()
                .HasOne(b => b.RoleIDs)
                .WithOne(i => i.ClanData)
                .HasForeignKey<RoleIDs>(b => b.ClanID);

            modelBuilder.Entity<PlayerData>()
                .HasOne(b => b.ClanData)
                .WithMany(i => i.PlayerData)
                .HasForeignKey(b => b.ClanID);

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

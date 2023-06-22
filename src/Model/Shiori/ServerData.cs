using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace ShioriBot.Model
{
    [Table("server_data")]
    public class ServerData
    {
        [Column("server_id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT"), Key, Required]
        public ulong ServerID { get; set; }

        /// <summary>
        /// DiscordのサーバーID
        /// </summary>
        [Column("discord_guild_id", TypeName = "BIGINT UNSIGNED"), Required]
        public ulong DiscordGuildID { get; set; }

        [Column("server_name", TypeName = "VARCHAR(100)")]
        public string ServerName { get; set; }

        [Column("server_icon_hash", TypeName = "VARCHAR(100)")]
        public string ServerIconHash { get; set; }

        [Column("server_owner_id", TypeName = "BIGINT UNSIGNED")]
        public ulong ServerOwnerID { get; set; }

        [Column("admin_role_id", TypeName = "BIGINT UNSIGNED")]
        public ulong AdminRoleID { get; set; }

        [Column("system_msg_ch_id", TypeName = "BIGINT UNSIGNED")]
        public ulong SystemMessageChannelID { get; set; }

        [Column("system_ch_id", TypeName = "BIGINT UNSIGNED")]
        public ulong SystemCommandChannelID { get; set; }

        [Column("create_date_time")]
        [Timestamp]
        public DateTime CreateDateTime { get; set; }

        [Column("delete_flag", TypeName = "TINYINT UNSIGNED"), DefaultValue(false)]
        public bool DeleteFlag { get; set; }

        public List<ClanData> ClanData { get; set; }

        public static void OnModelCreate(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ServerData>()
                .HasIndex(m => m.DiscordGuildID)
                .IsUnique();
        }
    }
}

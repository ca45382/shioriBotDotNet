using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShioriBot.Net.Model
{
    [Table("server_data")]
    public class ServerData
    {
        [Column("server_id", TypeName = "BIGINT UNSIGNED"), Key]
        public ulong ServerID { get; set; }

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

        public List<ClanData> ClanData { get; set; }
    }
}

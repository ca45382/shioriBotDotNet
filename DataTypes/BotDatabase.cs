using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

using Microsoft.EntityFrameworkCore;

namespace PriconneBotConsoleApp.DataTypes
{
    
    [Table("server_data")]
    public class BotDatabase
    {
        [Column("server_id", TypeName = "varchar(21)"), Key]
        public string ServerID { get; set; }

        [Column("server_name", TypeName = "varchar(100)")]
        public string ServerName { get; set; }

        [Column("server_owner_id", TypeName = "varchar(21)")]
        public string ServerOwnerID { get; set; }

        [Column("admin_role_id", TypeName = "varchar(21)")]
        public string AdminRoleID { get; set; }

        [Column("system_msg_ch_id", TypeName = "varchar(21)")]
        public string SystemMessageChannelID { get; set; }

        [Column("system_ch_id", TypeName = "varchar(21)")]
        public string SystemCommandChannelID { get; set; }


        public List<ClanData> ClanData { get; set; }
    }

}

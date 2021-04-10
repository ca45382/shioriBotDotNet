using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PriconneBotConsoleApp.DataTypes
{
    
    [Table("server_data")]
    public class BotDatabase
    {
        [Column("server_id", TypeName = "varchar(21)"), Required]
        public string ServerID { get; set; }

        [Column("server_id", TypeName = "varchar(100)")]
        public string ServerName { get; set; }


        public List<ClanData> ClanData { get; set; }
    }

}

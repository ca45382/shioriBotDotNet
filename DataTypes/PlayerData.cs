using System;
using System.Collections.Generic;
using System.Text;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataTypes
{
    [Table("player_data")]
    public class PlayerData
    {
        [Column("player_id", TypeName = "BIGINT UNSIGNED AUTO_INCREMENT")]
        public int PlayerID { get; set; }

        [Column("user_id", TypeName = "varchar(21)"), Required]
        public string UserID { get; set; }

        [Column("name", TypeName = "VARCHAR(100)")]
        public string GuildUserName { get; set; }


        //外部キー
        [Column("clan_id", TypeName = "BIGINT UNSIGNED"), Required]
        public int ClanID { get; set; }

        public ClanData ClanData { get; set; }


        public List<ReservationData> ReservationData { get; set; }
    }
}

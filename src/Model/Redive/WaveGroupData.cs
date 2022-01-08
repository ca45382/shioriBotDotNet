using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShioriBot.Net.Model
{
    [Table("wave_group_data")]
    public class WaveGroupData
    {
        [Column("id", TypeName = "INTEGER"), Key]
        public int ID { get; set; }

        [Column("wave_group_id", TypeName = "INTEGER")]
        public int WaveGroupID { get; set; }

        [Column("enemy_id_1", TypeName = "INTEGER")]
        public int EnemyID1 { get; set; }

        [Column("enemy_id_2", TypeName = "INTEGER")]
        public int EnemyID2 { get; set; }

        [Column("enemy_id_3", TypeName = "INTEGER")]
        public int EnemyID3 { get; set; }

        [Column("enemy_id_4", TypeName = "INTEGER")]
        public int EnemyID4 { get; set; }

        [Column("enemy_id_5", TypeName = "INTEGER")]
        public int EnemyID5 { get; set; }
    }
}

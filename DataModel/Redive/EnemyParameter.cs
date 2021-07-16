using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PriconneBotConsoleApp.DataModel
{
    [Table("enemy_parameter")]
    public class EnemyParameter
    {
        [Column("enemy_id", TypeName = "INTEGER"), Key]
        public int EnemyID { get; set; }

        [Column("name", TypeName = "TEXT")]
        public string Name { get; set; }

        [Column("level", TypeName = "INTEGER")]
        public int Level { get; set; }

        [Column("hp", TypeName = "INTEGER")]
        public int HP { get; set; }
    }
}

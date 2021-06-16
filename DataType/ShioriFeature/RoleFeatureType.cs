using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    public enum RoleFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("TaskKillRoleID")]
        TaskKillRoleID = 3001,

        [Description("Boss1RoleID")]
        Boss1RoleID = 3011,

        [Description("Boss1RoleID")]
        Boss2RoleID = 3012,

        [Description("Boss1RoleID")]
        Boss3RoleID = 3013,

        [Description("Boss1RoleID")]
        Boss4RoleID = 3014,

        [Description("Boss1RoleID")]
        Boss5RoleID = 3015,
    }
}

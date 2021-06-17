using System.ComponentModel;

namespace PriconneBotConsoleApp.DataType
{
    //ロール系 3xxx
    public enum RoleFeatureType
    {
        [Description("")]
        Unknown = 0,

        [Description("TaskKillRoleID")]
        TaskKillRoleID = 3001,

        [Description("Boss1RoleID")]
        Boss1RoleID = 3101,

        [Description("Boss1RoleID")]
        Boss2RoleID = 3102,

        [Description("Boss1RoleID")]
        Boss3RoleID = 3103,

        [Description("Boss1RoleID")]
        Boss4RoleID = 3104,

        [Description("Boss1RoleID")]
        Boss5RoleID = 3105,
    }
}

using System.Threading.Tasks;
using PriconneBotConsoleApp.Attribute;
using PriconneBotConsoleApp.DataModel;
using PriconneBotConsoleApp.DataType;

namespace PriconneBotConsoleApp.Script
{
    public static class Commands
    {
        // 持ち越し関連
        [Command(AttackType.CarryOver, 2, compatibleChannels: ChannelFeatureType.CarryOverID)]
        public static Task UpdateCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).UpdateCarryOverData();
            return Task.CompletedTask;
        }

        [Command("消化", 0, 1, compatibleChannels: ChannelFeatureType.CarryOverID)]
        public static Task DeleteCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).DeleteCarryOverData();
            return Task.CompletedTask;
        }

        [Command("!rm", 0, 2, compatibleChannels: ChannelFeatureType.CarryOverID)]
        public static Task DeleteOtherPlayerCarryOverData(CommandEventArgs commandEventArgs)
        {
            if (commandEventArgs.Arguments.Count == 2)
            {
                new BattleCarryOver(commandEventArgs).DeleteOtherPlayerData();
            }
            else
            {
                new BattleCarryOver(commandEventArgs).DeleteCarryOverData();
            }

            return Task.CompletedTask;
        }

        [Command("!list", 0, 0, compatibleChannels: ChannelFeatureType.CarryOverID)]
        public static async Task DisplayCarryOverList(CommandEventArgs commandEventArgs)
        {
            await new BattleCarryOver(commandEventArgs).SendClanCarryOverList();
        }

        [Command("!init", 0, 0, compatibleChannels: ChannelFeatureType.CarryOverID)]
        public static Task InitAllCarryOverData(CommandEventArgs commandEventArgs)
        {
            new BattleCarryOver(commandEventArgs).InitAllData();
            return Task.CompletedTask;
        }

        // 進行関連
        [Command(
            AttackType.Physics,
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task AttackPhysics(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateAttackData(AttackType.Physics);

        [Command(
            AttackType.Magic,
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task AttackMagic(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateAttackData(AttackType.Magic);

        [Command(
            AttackType.NewYearKaryl,
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task AttackNewYearKaryl(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateAttackData(AttackType.NewYearKaryl);

        [Command(
            AttackType.CarryOver,
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task AttackCarryOver(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateAttackData(AttackType.CarryOver);

        [Command(
            new[] { "kari", "仮確定" },
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task UpdateProgressStatusReady(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateStatusData(ProgressStatus.AttackReady);

        [Command(
            new[] { "atk", "確定" },
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task UpdateProgressStatusDone(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateStatusData(ProgressStatus.AttackDone);

        [Command(
            new[] { "sos", "jiko", "ziko", "事故" },
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task UpdateProgressSOS(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateStatusData(ProgressStatus.SOS);

        [Command(
            new[] { "〆確定", "fin" },
            0,
            0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task UpdateProgressFin(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).UpdateStatusData(ProgressStatus.Fin);

        [Command(
            "!init", 0, 0,
            new[]
            {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
            }
        )]
        public static async Task InitProgressData(CommandEventArgs commandEventArgs)
            => await new BattleProgress(commandEventArgs).InitCommand();

        [Command(
             "!call", 1, 1,
             new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task CallProgress(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).Start();

        [Command(
             "!list", 0, 0,
             new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task DisplayList(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).SendList();

        [Command(
             "!rm", 1, 1,
             new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task RemoveProgressData(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).RemoveOrRevertUserData(true);

        [Command(
             "!rv", 1, 1,
             new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task RevertProgressData(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).RemoveOrRevertUserData();

        [Command(
             "!next", 0, 0,
             new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task NextProgressBoss(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).NextBoss();

        [Command(
             compatibleChannels: new[]
             {
                ChannelFeatureType.ProgressBoss1ID,
                ChannelFeatureType.ProgressBoss2ID,
                ChannelFeatureType.ProgressBoss3ID,
                ChannelFeatureType.ProgressBoss4ID,
                ChannelFeatureType.ProgressBoss5ID,
             }
         )]
        public static async Task UpdateProgressDamage(CommandEventArgs commandEventArgs)
             => await new BattleProgress(commandEventArgs).UpdateDamageData();

        [Command("!list", 0, 0, ChannelFeatureType.ReportID)]
        public static async Task ListReport(CommandEventArgs commandEventArgs)
            => await new BattleReport(commandEventArgs).SendClanAttackList();

        [Command("!rm", 0, 1, ChannelFeatureType.ReportID)]
        public static Task RemovePlayerReportData(CommandEventArgs commandEventArgs)
        {
            new BattleReport(commandEventArgs).DeleteReportData();
            return Task.CompletedTask;
        }

        [Command("!add", 3, 3, ChannelFeatureType.ReportID)]
        public static Task RegisterOtherPlayerReportData(CommandEventArgs commandEventArgs)
        {
            new BattleReport(commandEventArgs).RegisterOtherUserReportData();
            return Task.CompletedTask;
        }

        [Command("!init", 0, 0, ChannelFeatureType.ReportID)]
        public static Task InitReportData(CommandEventArgs commandEventArgs)
        {
            new BattleReport(commandEventArgs).DeleteAllClanReport();
            return Task.CompletedTask;
        }

        [Command(minArgumentLength: 0, maxArgumentLength: 0, compatibleChannels: ChannelFeatureType.ReportID)]
        public static Task RegisterReportData(CommandEventArgs commandEventArgs)
        {
            new BattleReport(commandEventArgs).RegisterReportData();
            return Task.CompletedTask;
        }

        [Command("予約", 0, compatibleChannels: ChannelFeatureType.ReserveID)]
        public static Task ReservationCommand(CommandEventArgs commandEventArgs)
        {
            BattleReservation battleReservation = new(commandEventArgs);

            if (commandEventArgs.Arguments.Count == 0)
            {
                battleReservation.PlayerReserveList();
            }
            else
            {
                battleReservation.RegisterReserveData();
            }

            return Task.CompletedTask;
        }

        [Command(new[] { "予約一覧, 予約確認, 予約状況, !list" }, 0, 0, ChannelFeatureType.ReserveID)]
        public static Task ListReservation(CommandEventArgs commandEventArgs)
        {
            new BattleReservation(commandEventArgs).PlayerReserveList();
            return Task.CompletedTask;
        }

        [Command(new[] { "削除", "!rm" }, 2, 2, ChannelFeatureType.ReserveID)]
        public static Task DeleteReserveData(CommandEventArgs commandEventArgs)
        {
            new BattleReservation(commandEventArgs).DeleteReserveData();
            return Task.CompletedTask;
        }

        [Command("!start", 0, 0, ChannelFeatureType.ReserveResultID)]
        public static async Task SendReserveSummary(CommandEventArgs commandEventArgs)
            => await new BattleReservationSummary(commandEventArgs.Role, commandEventArgs.ClanData).SendMessage();

        [Command(compatibleChannels: new[]
             {
                ChannelFeatureType.DeclareBoss1ID,
                ChannelFeatureType.DeclareBoss2ID,
                ChannelFeatureType.DeclareBoss3ID,
                ChannelFeatureType.DeclareBoss4ID,
                ChannelFeatureType.DeclareBoss5ID,
             })]
        public static async Task StartDeclare(CommandEventArgs commandEventArgs)
        {
            var BossNumber = commandEventArgs.ChannelFeatureType switch
            {
                ChannelFeatureType.DeclareBoss1ID => BossNumberType.Boss1Number,
                ChannelFeatureType.DeclareBoss2ID => BossNumberType.Boss2Number,
                ChannelFeatureType.DeclareBoss3ID => BossNumberType.Boss3Number,
                ChannelFeatureType.DeclareBoss4ID => BossNumberType.Boss4Number,
                ChannelFeatureType.DeclareBoss5ID => BossNumberType.Boss5Number,
                _ => BossNumberType.Unknown,
            };

            if (BossNumber == BossNumberType.Unknown)
            {
                return;
            }

            await new BattleDeclaration(commandEventArgs.ClanData, commandEventArgs.SocketUserMessage, BossNumber).RunDeclarationCommandByMessage();
        }
    }
}

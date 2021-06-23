using Microsoft.EntityFrameworkCore;
using PriconneBotConsoleApp.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PriconneBotConsoleApp.Database
{
    class DatabaseTaskKillController
    {
        /// <summary>
        /// クラン全体のタスキル情報を取得
        /// </summary>
        /// <param name="clanData"></param>
        /// <returns></returns>
        public IEnumerable<TaskKillData> LoadTaskKillData(ClanData clanData)
        {
            using var databaseConnector = new DatabaseConnector();

            return databaseConnector.TaskKillData
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.DeleteFlag)
                .ToList();
        }

        /// <summary>
        /// 個人のタスキル情報を取得
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns></returns>
        public TaskKillData LoadTaskKillData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (playerData.PlayerID == 0)
            {
                playerData = databaseConnector.PlayerData
                    .FirstOrDefault(x => x.ClanID == playerData.ClanID && x.UserID == playerData.UserID);
            }

            if (playerData == null)
            {
                return null;
            }

            return databaseConnector.TaskKillData
                .FirstOrDefault(x => x.PlayerID == playerData.PlayerID && !x.DeleteFlag);
        }

        /// <summary>
        /// タスキルデータをデータベースに登録。
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns>書き込み成功True</returns>
        public bool CreateTaskKillData(PlayerData playerData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (playerData == null)
            {
                return false;
            }

            var databaseUserTaskKill = databaseConnector.TaskKillData.AsQueryable()
                .FirstOrDefault(x => x.PlayerID == playerData.PlayerID && !x.DeleteFlag);

            if (databaseUserTaskKill != null)
            {
                return false;
            }

            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseConnector.TaskKillData.Add(new TaskKillData()
                {
                    PlayerID = playerData.PlayerID,
                });
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        /// <summary>
        /// タスキルデータのDeleteFlagを付与。
        /// </summary>
        /// <param name="playerData"></param>
        /// <returns>付与成功True</returns>
        public bool DeleteTaskKillData(TaskKillData taskKillData)
        {
            using var databaseConnector = new DatabaseConnector();

            if (taskKillData == null)
            {
                return false;
            }

            var databaseData = databaseConnector.TaskKillData
                .FirstOrDefault(x => x.TaskKillID == taskKillData.TaskKillID && !x.DeleteFlag);

            if (databaseData == null)
            {
                return false;
            }

            var transaction = databaseConnector.Database.BeginTransaction();

            try
            {
                databaseData.DeleteFlag = true;
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }

        }

        public bool DeleteTaskKillData(ClanData clanData)
        {
            if (clanData == null)
            {
                return false;
            }

            using var databaseConnector = new DatabaseConnector();

            var deleteDataList = databaseConnector.TaskKillData
                .Include(x => x.PlayerData)
                .Where(x => x.PlayerData.ClanID == clanData.ClanID && !x.DeleteFlag)
                .ToList();

            if (!deleteDataList.Any())
            {
                return false;
            }

            var transaction = databaseConnector.Database.BeginTransaction();
            try
            {
                deleteDataList.ForEach(x => x.DeleteFlag = true);
                databaseConnector.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
            
        }
    }
}

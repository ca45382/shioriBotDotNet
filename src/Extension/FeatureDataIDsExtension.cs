
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ShioriBot.Net.Model;
using ShioriBot.Net.DataType;

namespace ShioriBot.Net.Extension
{

    public static class FeatureDataIDsExtension
    {
        /// <summary>
        /// 指定された機能のチャンネルIDを返します。
        /// 存在しない場合は0を返します。
        /// </summary>
        /// <param name="channelDataList"></param>
        /// <param name="clanID">ClanData.ClanIDを使います。</param>
        /// <param name="type"></param>
        /// <returns>IDか0</returns>
        public static ulong GetChannelID(this IEnumerable<ChannelData> channelDataList,
            ulong clanID, ChannelFeatureType type)
        {
            return channelDataList.FirstOrDefault(x => x.ClanID == clanID && x.FeatureID == (uint)type)
                ?.ChannelID ?? 0;
        }

        /// <summary>
        /// 指定された機能のメッセージIDを返します。
        /// 存在しない場合は0を返します。
        /// </summary>
        /// <param name="messageDataList"></param>
        /// <param name="clanID">ClanData.ClanIDを使います。</param>
        /// <param name="type"></param>
        /// <returns>IDか0</returns>
        public static ulong GetMessageID(this IEnumerable<MessageData> messageDataList,
            ulong clanID, MessageFeatureType type)
        {
            return messageDataList.FirstOrDefault(x => x.ClanID == clanID && x.FeatureID == (uint)type)
                ?.MessageID ?? 0;
        }

        /// <summary>
        /// 指定された機能のロールIDを返します。
        /// 存在しない場合は0を返します。
        /// </summary>
        /// <param name="roleDataList"></param>
        /// <param name="clanID">ClanData.ClanIDを使います。</param>
        /// <param name="type"></param>
        /// <returns>IDか0</returns>
        public static ulong GetRoleID(this IEnumerable<RoleData> roleDataList,
            ulong clanID, RoleFeatureType type)
        {
            return roleDataList.FirstOrDefault(x => x.ClanID == clanID && x.FeatureID == (uint)type)
                ?.RoleID ?? 0;
        }
    }
    
}

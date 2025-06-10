using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Generates from a <see cref="SptProfile"/>
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>A new <see cref="FriendData"/></returns>
        public static FriendData ToFriendData(this SptProfile profile)
        {
            return new()
            {
                Aid = profile.ProfileInfo.Aid,
                Id = profile.ProfileInfo.ProfileId,
                Info = new()
                {
                    Level = profile.CharacterData.PmcData.Info.Level,
                    MemberCategory = profile.CharacterData.PmcData.Info.MemberCategory,
                    SelectedMemberCategory = profile.CharacterData.PmcData.Info.SelectedMemberCategory,
                    Nickname = profile.CharacterData.PmcData.Info.Nickname,
                    Side = profile.CharacterData.PmcData.Info.Side
                }
            };
        }

        /// <summary>
        /// Generates from a <see cref="PmcData"/>
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>A new <see cref="FriendData"/></returns>
        public static FriendData ToFriendData(this PmcData pmcData)
        {
            return new()
            {
                Aid = pmcData.Aid,
                Id = pmcData.Id,
                Info = new()
                {
                    Level = pmcData.Info.Level,
                    MemberCategory = pmcData.Info.MemberCategory,
                    SelectedMemberCategory = pmcData.Info.SelectedMemberCategory,
                    Nickname = pmcData.Info.Nickname,
                    Side = pmcData.Info.Side
                }
            };
        }
    }
}

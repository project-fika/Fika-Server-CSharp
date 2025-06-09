using FikaServer.Models.Fika.Dialog;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer
{
    public static class ExtensionMethods
    {
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
    }
}

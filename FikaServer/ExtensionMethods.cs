using FikaServer.Models.Enums;
using FikaServer.Models.Fika.Dialog;
using FikaServer.Models.Fika.Presence;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Profile;
using System.Diagnostics.CodeAnalysis;
using static FikaShared.Enums;

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

        /// <summary>
        /// Checks if the profile has valid data to get the <see cref="Info.ProfileId"/>
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static bool HasProfileData(this SptProfile? profile)
        {
            return profile != null && profile.ProfileInfo?.ProfileId != null && profile.CharacterData?.PmcData?.Info?.Nickname != null;
        }

        public static EFikaLocation ToFikaLocation(this string location)
        {
            return location switch
            {
                "bigmap" => EFikaLocation.Customs,
                "factory4_day" or "factory4_night" => EFikaLocation.Factory,
                "interchange" => EFikaLocation.Interchange,
                "laboratory" => EFikaLocation.Laboratory,
                "labyrinth" => EFikaLocation.Labyrinth,
                "lighthouse" => EFikaLocation.Lighthouse,
                "rezervbase" => EFikaLocation.Reserve,
                "sandbox" or "sandbox_high" => EFikaLocation.GroundZero,
                "shoreline" => EFikaLocation.Shoreline,
                "tarkovstreets" => EFikaLocation.Streets,
                "woods" => EFikaLocation.Streets,
                _ => EFikaLocation.None,
            };
        }

        public static EFikaLocation ToFikaLocation(this FikaPlayerPresence presence)
        {
            if (presence.Activity is EFikaPlayerPresences.IN_HIDEOUT)
            {
                return EFikaLocation.Hideout;
            }

            if (presence.Activity is not EFikaPlayerPresences.IN_RAID)
            {
                return EFikaLocation.None;
            }

            return presence.RaidInformation?.Location switch
            {
                "bigmap" => EFikaLocation.Customs,
                "factory4_day" or "factory4_night" => EFikaLocation.Factory,
                "interchange" => EFikaLocation.Interchange,
                "laboratory" => EFikaLocation.Laboratory,
                "labyrinth" => EFikaLocation.Labyrinth,
                "lighthouse" => EFikaLocation.Lighthouse,
                "rezervbase" => EFikaLocation.Reserve,
                "sandbox" or "sandbox_high" => EFikaLocation.GroundZero,
                "shoreline" => EFikaLocation.Shoreline,
                "tarkovstreets" => EFikaLocation.Streets,
                "woods" => EFikaLocation.Streets,
                _ => EFikaLocation.None,
            };
        }
    }
}

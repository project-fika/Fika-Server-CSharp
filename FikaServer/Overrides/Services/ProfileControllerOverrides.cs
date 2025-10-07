using FikaServer.Models.Fika.Config;
using FikaServer.Services;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using System.Reflection;

namespace FikaServer.Overrides.Services;

public class GetMiniProfilesOverride : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProfileController).GetMethod(nameof(ProfileController.GetMiniProfiles))!;
    }

    [PatchPrefix]
    public static bool Prefix(ref List<MiniProfile> __result)
    {
        FikaConfig fikaConfig = ServiceLocator.ServiceProvider.GetService<ConfigService>()?.Config ?? throw new NullReferenceException("FikaConfig is null!");

        if (!fikaConfig.Server.LauncherListAllProfiles)
        {
            __result = [];

            return false;
        }

        return true;
    }
}


public class GetFriendsOverride : AbstractPatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProfileController).GetMethod(nameof(ProfileController.SearchProfiles))!;
    }

    [PatchPrefix]
    public static bool Prefix(SearchProfilesRequestData request, MongoId sessionID, ref List<SearchFriendResponse> __result)
    {
        string searchNickname = request.Nickname.ToLower();

        ProfileHelper profileHelper = ServiceLocator.ServiceProvider.GetService<ProfileHelper>() ?? throw new NullReferenceException("ProfileHelper is null!");

        Dictionary<MongoId, SptProfile> profiles = profileHelper.GetProfiles();
        List<SearchFriendResponse> friends = [];

        foreach (SptProfile profile in profiles.Values)
        {
            if (profile.IsHeadlessProfile())
            {
                continue;
            }

            if (profile.CharacterData?.PmcData?.Info != null)
            {
                if (profile.CharacterData.PmcData.Info.Nickname.StartsWith(searchNickname, StringComparison.CurrentCultureIgnoreCase))
                {
                    friends.Add(new SearchFriendResponse
                    {
                        Id = profile.CharacterData.PmcData.Id.Value,
                        Aid = profile.CharacterData.PmcData.Aid,
                        Info = new UserDialogDetails
                        {
                            Nickname = profile.CharacterData.PmcData.Info.Nickname,
                            Side = profile.CharacterData.PmcData.Info.Side,
                            Level = profile.CharacterData.PmcData.Info.Level,
                            MemberCategory = profile.CharacterData.PmcData.Info.MemberCategory,
                            SelectedMemberCategory = profile.CharacterData.PmcData.Info.SelectedMemberCategory
                        }
                    });
                }
            }
        }

        __result = friends;
        return false;
    }
}

using FikaServer.Helpers;
using FikaServer.Models.Fika.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Reflection.Patching;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers.Profile;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Launcher;
using SPTarkov.Server.Core.Models.Eft.Profile;
using System.Reflection;

namespace FikaServer.Overrides.Services;

[Injectable]
public sealed class GetMiniProfilesOverride : AbstractPatch
{
    private static HeadlessHelper _headlessHelper = default!;

    public GetMiniProfilesOverride(HeadlessHelper headlessHelper)
    {
        _headlessHelper = headlessHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProfileController).GetMethod(nameof(ProfileController.GetMiniProfiles))!;
    }

    [PatchPostfix]
    public static void Postfix(ref List<MiniProfile> __result)
    {
        __result.RemoveAll(x => _headlessHelper.IsHeadlessClient(x.ProfileId));
    }
}


[Injectable]
public sealed class GetFriendsOverride : AbstractPatch
{
    private static ProfileHelper _profileHelper = default!;

    public GetFriendsOverride(ProfileHelper profileHelper)
    {
        _profileHelper = profileHelper;
    }

    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProfileController).GetMethod(nameof(ProfileController.SearchProfiles))!;
    }

    [PatchPrefix]
    public static bool Prefix(SearchProfilesRequestData request, MongoId sessionID, ref List<SearchFriendResponse> __result)
    {
        var searchNickname = request.Nickname.ToLower();

        var profiles = _profileHelper.GetProfiles();
        List<SearchFriendResponse> friends = [];

        foreach (var profile in profiles.Values)
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

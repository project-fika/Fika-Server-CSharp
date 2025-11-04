using FikaServer.Models.Fika;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using Path = System.IO.Path;

namespace FikaServer.Services.Headless;

[Injectable(InjectionType.Singleton)]
public class HeadlessProfileService(ISptLogger<HeadlessProfileService> logger, SaveServer saveServer, ConfigService configService,
    ConfigServer configServer, HashUtil hashUtil, ProfileController profileController, InventoryHelper inventoryHelper,
    JsonUtil jsonUtil)
{
    private readonly CoreConfig _sptCoreConfig = configServer.GetConfig<CoreConfig>();
    public List<SptProfile> HeadlessProfiles { get; set; } = [];

    private static readonly MongoId HEAD_USEC_4 = new("5fdb4139e4ed5b5ea251e4ed"); // _parent: 5cc085e214c02e000c6bea67
    private static readonly MongoId VOICE_USEC_4 = new("6284d6a28e4092597733b7a6"); // _parent: 5fc100cf95572123ae738483

    public async Task OnPostLoadAsync()
    {
        LoadHeadlessProfiles();
        logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Found {HeadlessProfiles.Count} headless profiles");

        var profileAmount = configService.Config.Headless.Profiles.Amount;

        if (HeadlessProfiles.Count < profileAmount)
        {
            List<SptProfile> createdProfiles = await CreateHeadlessProfiles(profileAmount);
            logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Created {createdProfiles.Count} headless client profiles!");
        }

        // Stop headless from adding up to the percentage of achievements unlocked
        foreach (SptProfile headlessProfile in HeadlessProfiles)
        {
            _sptCoreConfig.Features.AchievementProfileIdBlacklist.Add(headlessProfile.ProfileInfo.ProfileId);
        }
    }

    internal async Task<List<SptProfile>> CreateHeadlessProfiles(int amount, bool ignoreCurrent = false)
    {
        var profileCount = HeadlessProfiles.Count;
        var profileAmountToCreate = ignoreCurrent ? amount : amount - profileCount;
        List<SptProfile> createdProfiles = [];
        for (var i = 0; i < profileAmountToCreate; i++)
        {
            SptProfile profile = await CreateHeadlessProfile();
            createdProfiles.Add(profile);
            HeadlessProfiles.Add(profile);
            GenerateLaunchScript(profile.ProfileInfo.ProfileId.Value);
        }

        return createdProfiles;
    }

    private void LoadHeadlessProfiles()
    {
        HeadlessProfiles = [.. saveServer.GetProfiles().Values
            .Where(x => x.IsHeadlessProfile())];
    }

    private async Task<SptProfile> CreateHeadlessProfile()
    {
        // Generate a unique username
        var username = $"headless_{new MongoId()}";
        // Random edition. Doesn't matter
        const string edition = "Standard";

        // Create mini profile
        string profileId = await CreateMiniProfile(username, edition);

        // Random character configs. Doesn't matter.
        ProfileCreateRequestData newProfileData = new()
        {
            Side = "usec",
            Nickname = username, // Use the username as the nickname to ensure it is unique.
            HeadId = HEAD_USEC_4,
            VoiceId = VOICE_USEC_4
        };

        return await CreateFullProfile(newProfileData, profileId);
    }

    private void GenerateLaunchScript(MongoId profileId)
    {
        var modPath = configService.ModPath;
        var scriptsPath = Path.Combine(modPath, "assets/scripts/");
        var newFolderPath = Path.Combine(scriptsPath, profileId);

        try
        {
            Directory.CreateDirectory(newFolderPath);
        }
        catch (Exception ex)
        {
            logger.Error($"Failed to create headless launch settings for {profileId}", ex);
            return;
        }

        var backendUrl = configService.Config.Headless.Scripts.ForceIp;
        backendUrl = string.IsNullOrEmpty(backendUrl) ? "https://127.0.0.1:6969" : backendUrl;

        if (!Uri.TryCreate(backendUrl, UriKind.Absolute, out Uri uri))
        {
            logger.Error($"Could not parse {backendUrl} as a valid URL, please delete the headless profile and try again.");
            return;
        }

        var launchSettings = new HeadlessLaunchSettings()
        {
            ProfileId = profileId,
            BackendUrl = uri,
            StartMinimized = false
        };

        var serialized = jsonUtil.Serialize(launchSettings, true);
        var newFile = Path.Combine(newFolderPath, "HeadlessConfig.json");
        File.WriteAllText(newFile, serialized);
        logger.Info($"Created new launch settings in {newFile} for headless profile {profileId}");
    }

    private async Task<MongoId> CreateMiniProfile(string username, string edition)
    {
        MongoId profileId = new();
        MongoId scavId = new();

        SPTarkov.Server.Core.Models.Eft.Profile.Info newProfile = new()
        {
            ProfileId = profileId,
            ScavengerId = scavId,
            Aid = hashUtil.GenerateAccountId(),
            Username = username,
            IsWiped = true,
            Edition = edition
        };

        saveServer.CreateProfile(newProfile);

        await saveServer.LoadProfileAsync(profileId);
        await saveServer.SaveProfileAsync(profileId);

        return profileId;
    }

    private async Task<SptProfile> CreateFullProfile(ProfileCreateRequestData profileData, MongoId profileId)
    {
        await profileController.CreateProfile(profileData, profileId);

        SptProfile profile = saveServer.GetProfile(profileId)
            ?? throw new NullReferenceException("CreateFullProfile:: Could not find profile");

        ClearUnecessaryHeadlessItems(profile.CharacterData.PmcData, profileId);

        profile.CharacterData.PmcData.Info.MemberCategory = MemberCategory.UnitTest;

        await saveServer.SaveProfileAsync(profileId);

        return profile;
    }

    private void ClearUnecessaryHeadlessItems(PmcData pmcProfile, MongoId sessionId)
    {
        if (pmcProfile == null)
        {
            throw new NullReferenceException("ClearUnecessaryHeadlessItems:: PmcProfile was null");
        }

        foreach (var item in GetAllHeadlessItems(pmcProfile))
        {
            inventoryHelper.RemoveItem(pmcProfile, item, sessionId);
        }

        pmcProfile.Inventory.FastPanel = [];
    }

    private List<string?> GetAllHeadlessItems(PmcData pmcProfile)
    {
        List<Item> inventoryItems = pmcProfile.Inventory?.Items ?? [];
        string? equipmentRootId = pmcProfile.Inventory?.Equipment;
        string? stashRootId = pmcProfile.Inventory?.Stash;

        return [.. inventoryItems
            .Where(x => x.ParentId == equipmentRootId || x.ParentId == stashRootId)
            .Select(x => x.Id)];
    }
}

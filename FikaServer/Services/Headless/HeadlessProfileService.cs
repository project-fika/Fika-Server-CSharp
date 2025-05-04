using FikaServer.Models.Fika.Config;
using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Services.Headless
{
    [Injectable(InjectionType.Singleton)]
    public class HeadlessProfileService(ISptLogger<HeadlessProfileService> logger, SaveServer saveServer, ConfigService configService, HashUtil hashUtil,
        ProfileController profileController, InventoryHelper inventoryHelper)
    {
        public List<SptProfile> HeadlessProfiles { get; set; } = [];

        private readonly ISptLogger<HeadlessProfileService> _logger = logger;
        private readonly SaveServer _saveServer = saveServer;
        private readonly FikaConfig _fikaConfig = configService.Config;
        private readonly HashUtil _hashUtil = hashUtil;
        private readonly ProfileController _profileController = profileController;
        private readonly InventoryHelper _inventoryHelper = inventoryHelper;

        private const string HEAD_USEC_4 = "5fdb4139e4ed5b5ea251e4ed";
        private const string VOICE_USEC_4 = "6284d6a28e4092597733b7a6";

        public void PostSptLoad()
        {
            LoadHeadlessProfiles();
            _logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Found {HeadlessProfiles.Count} headless profiles");

            int profileAmount = _fikaConfig.Headless.Profiles.Amount;

            if (HeadlessProfiles.Count < profileAmount)
            {
                List<SptProfile> createdProfiles = CreateHeadlessProfiles(profileAmount);
                _logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Created {createdProfiles.Count} headless client profiles!");
            }
        }

        private void LoadHeadlessProfiles()
        {
            HeadlessProfiles = [.. _saveServer.GetProfiles().Values
                .Where(x => x.ProfileInfo?.Password == "fika-headless")];
        }

        private List<SptProfile> CreateHeadlessProfiles(int amount)
        {
            int profileCount = HeadlessProfiles.Count;
            int profileAmountToCreate = amount - profileCount;
            List<SptProfile> createdProfiles = [];
            for (int i = 0; i < profileAmountToCreate; i++)
            {
                SptProfile profile = CreateHeadlessProfile();
                createdProfiles.Add(profile);
                HeadlessProfiles.Add(profile);
            }

            return createdProfiles;
        }

        private SptProfile CreateHeadlessProfile()
        {
            string username = $"headless_{_hashUtil.Generate()}";
            string password = "fika-headless";
            string edition = "Standard";

            string profileId = CreateMiniProfile(username, password, edition);

            ProfileCreateRequestData newProfileData = new()
            {
                Side = "usec",
                Nickname = username,
                HeadId = HEAD_USEC_4,
                VoiceId = VOICE_USEC_4
            };

            return CreateFullProfile(newProfileData, profileId);
        }

        private string CreateMiniProfile(string username, string password, string edition)
        {
            var profileId = _hashUtil.Generate();
            var scavId = _hashUtil.Generate();

            SPTarkov.Server.Core.Models.Eft.Profile.Info newProfile = new()
            {
                ProfileId = profileId,
                ScavengerId = scavId,
                Aid = _hashUtil.GenerateAccountId(),
                Username = username,
                Password = password,
                IsWiped = true,
                Edition = edition
            };

            _saveServer.CreateProfile(newProfile);

            _saveServer.LoadProfile(profileId);
            _saveServer.SaveProfile(profileId);

            return profileId;
        }

        private SptProfile CreateFullProfile(ProfileCreateRequestData profileData, string profileId)
        {
            _profileController.CreateProfile(profileData, profileId);

            SptProfile profile = _saveServer.GetProfile(profileId)
                ?? throw new NullReferenceException("CreateFullProfile:: Could not find profile");

            ClearUnecessaryHeadlessItems(profile.CharacterData.PmcData, profileId);

            return profile;
        }

        private void GenerateLaunchScript(string profileId, string backendUrl, string scriptsFolderPath)
        {
            //Todo: Stub for now, implement method.
            // This will become a generator for a json that will be used in the new headless launcher
        }

        private void ClearUnecessaryHeadlessItems(PmcData pmcProfile, string sessionId)
        {
            if (pmcProfile == null)
            {
                throw new NullReferenceException("ClearUnecessaryHeadlessItems:: PmcProfile was null");
            }

            List<string?> itemsToDelete = GetAllHeadlessItems(pmcProfile);
            foreach (string? item in itemsToDelete)
            {
                _inventoryHelper.RemoveItem(pmcProfile, item, sessionId);
            }

            pmcProfile.Inventory.FastPanel = [];
        }

        private List<string?> GetAllHeadlessItems(PmcData pmcProfile)
        {
            var inventoryItems = pmcProfile.Inventory?.Items ?? [];
            var equipmentRootId = pmcProfile.Inventory?.Equipment;
            var stashRootId = pmcProfile.Inventory?.Stash;

            return [.. inventoryItems
                .Where(x => x.ParentId == equipmentRootId || x.ParentId == stashRootId)
                .Select(x => x.Id)];
        }
    }
}

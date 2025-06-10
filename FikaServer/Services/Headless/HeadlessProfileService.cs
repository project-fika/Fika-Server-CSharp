using SPTarkov.DI.Annotations;
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

        private const string HEAD_USEC_4 = "5fdb4139e4ed5b5ea251e4ed"; // _parent: 5cc085e214c02e000c6bea67
        private const string VOICE_USEC_4 = "6284d6a28e4092597733b7a6"; // _parent: 5fc100cf95572123ae738483

        public async Task OnPostLoadAsync()
        {
            LoadHeadlessProfiles();
            logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Found {HeadlessProfiles.Count} headless profiles");

            int profileAmount = configService.Config.Headless.Profiles.Amount;

            if (HeadlessProfiles.Count < profileAmount)
            {
                List<SptProfile> createdProfiles = await CreateHeadlessProfiles(profileAmount);
                logger.Log(SPTarkov.Server.Core.Models.Spt.Logging.LogLevel.Info, $"Created {createdProfiles.Count} headless client profiles!");
            }
        }

        private void LoadHeadlessProfiles()
        {
            HeadlessProfiles = [.. saveServer.GetProfiles().Values
                .Where(x => x.ProfileInfo?.Password == "fika-headless")];
        }

        private async Task<List<SptProfile>> CreateHeadlessProfiles(int amount)
        {
            int profileCount = HeadlessProfiles.Count;
            int profileAmountToCreate = amount - profileCount;
            List<SptProfile> createdProfiles = [];
            for (int i = 0; i < profileAmountToCreate; i++)
            {
                SptProfile profile = await CreateHeadlessProfile();
                createdProfiles.Add(profile);
                HeadlessProfiles.Add(profile);
            }

            return createdProfiles;
        }

        private async Task<SptProfile> CreateHeadlessProfile()
        {
            // Generate a unique username
            string username = $"headless_{hashUtil.Generate()}";
            // Using a password allows us to know which profiles are headless client profiles.
            string password = "fika-headless";
            // Random edition. Doesn't matter
            string edition = "Standard";

            // Create mini profile
            string profileId = await CreateMiniProfile(username, password, edition);

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

        private async Task<string> CreateMiniProfile(string username, string password, string edition)
        {
            string profileId = hashUtil.Generate();
            string scavId = hashUtil.Generate();

            SPTarkov.Server.Core.Models.Eft.Profile.Info newProfile = new()
            {
                ProfileId = profileId,
                ScavengerId = scavId,
                Aid = hashUtil.GenerateAccountId(),
                Username = username,
                Password = password,
                IsWiped = true,
                Edition = edition
            };

            saveServer.CreateProfile(newProfile);

            await saveServer.LoadProfileAsync(profileId);
            await saveServer.SaveProfileAsync(profileId);

            return profileId;
        }

        private async Task<SptProfile> CreateFullProfile(ProfileCreateRequestData profileData, string profileId)
        {
            await profileController.CreateProfile(profileData, profileId);

            SptProfile profile = saveServer.GetProfile(profileId)
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
}

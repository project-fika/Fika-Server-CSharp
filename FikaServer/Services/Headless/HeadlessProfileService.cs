using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Eft.Profile;

namespace FikaServer.Services.Headless
{
    [Injectable(InjectionType.Singleton)]
    public class HeadlessProfileService
    {
        public SptProfile[] GetHeadlessProfiles()
        {
            //Todo: Stub for now, implement method.
            return [];
        }

        public void PostSptLoad()
        {
            //Todo: Stub for now, implement method.
        }

        private void LoadHeadlessProfiles()
        {
            //Todo: Stub for now, implement method.
        }

        private SptProfile[] CreateHeadlessProfiles(int Amount)
        {
            //Todo: Stub for now, implement method.
            return [];
        }

        private SptProfile CreateHeadlessProfile()
        {
            //Todo: Stub for now, implement method.
            return new SptProfile();
        }

        private string CreateMiniProfile(string username, string password, string edition)
        {
            //Todo: Stub for now, implement method.
            return "";
        }

        private SptProfile CreateFullProfile(ProfileCreateRequestData profileData, string profileId)
        {
            //Todo: Stub for now, implement method.
            return new();
        }

        private void GenerateLaunchScript(string profileId, string backendUrl, string scriptsFolderPath)
        {
            //Todo: Stub for now, implement method.
        }

        private void ClearUnecessaryHeadlessItems(PmcData pmcProfile, string sessionId)
        {
            //Todo: Stub for now, implement method.
        }

        private Item[] GetAllHeadlessItems(PmcData pmcProfile)
        {
            //Todo: Stub for now, implement method.
            return [];
        }
    }
}

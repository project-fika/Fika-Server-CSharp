using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Models.Fika.Insurance
{
    public record FikaInsurancePlayer
    {
        public string SessionID { get; set; } = string.Empty;
        public bool EndedRaid { get; set; } = false;
        public List<MongoId> LostItems { get; set; } = [];
        public List<MongoId> FoundItems { get; set; } = [];
        public List<MongoId> Inventory { get; set; } = [];
    }
}

using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Models.Fika.Insurance;

public sealed record FikaInsurancePlayer
{
    public MongoId SessionID { get; set; }
    public bool EndedRaid { get; set; }
    public List<MongoId> LostItems { get; set; } = [];
    public List<MongoId> FoundItems { get; set; } = [];
    public List<MongoId> Inventory { get; set; } = [];
}

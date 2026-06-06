using SPTarkov.Server.Core.Models.Common;

namespace FikaServer.Models.Fika.Insurance;

public sealed record FikaInsurancePlayer
{
    public required MongoId SessionID { get; init; }
    public bool EndedRaid { get; set; }
    public MongoId[]? InsuredItemsBroughtToRaid { get; set; }
    public MongoId[]? InventoryAfterRaid { get; set; }
}

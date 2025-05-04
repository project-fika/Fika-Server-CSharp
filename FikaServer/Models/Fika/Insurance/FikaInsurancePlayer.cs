namespace FikaServer.Models.Fika.Insurance
{
    public record FikaInsurancePlayer
    {
        public string SessionID { get; set; } = string.Empty;
        public bool EndedRaid { get; set; } = false;
        public List<string?> LostItems { get; set; } = [];
        public List<string?> FoundItems { get; set; } = [];
        public List<string?> Inventory { get; set; } = [];
    }
}

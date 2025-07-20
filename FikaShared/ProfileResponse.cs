namespace FikaShared
{
    public record ProfileResponse
    {
        public string Nickname { get; set; } = string.Empty;
        public string ProfileId { get; set; } = string.Empty;
        public bool HasFleaBan { get; set; }
        public int Level { get; set; }
    }
}

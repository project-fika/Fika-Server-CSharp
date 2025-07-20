namespace FikaShared.Requests
{
    public record SendItemRequest : ProfileIdRequest
    {
        public required string ItemTemplate { get; set; }
        public required int Amount { get; set; }
    }
}

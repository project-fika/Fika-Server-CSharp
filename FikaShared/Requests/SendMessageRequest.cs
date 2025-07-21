namespace FikaShared.Requests
{
    public record SendMessageRequest : ProfileIdRequest
    {
        public required string Message { get; set; }
    }
}

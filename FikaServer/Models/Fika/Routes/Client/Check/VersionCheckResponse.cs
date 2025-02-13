namespace FikaServer.Models.Fika.Routes.Client.Check
{
    public record VersionCheckResponse
    {
        public string Version { get; set; } = string.Empty;
    }
}

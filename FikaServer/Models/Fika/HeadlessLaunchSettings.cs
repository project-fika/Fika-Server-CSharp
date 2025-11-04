namespace FikaServer.Models.Fika;

public class HeadlessLaunchSettings
{
    public string? ProfileId { get; set; }
    public Uri? BackendUrl { get; set; }
    public bool StartMinimized { get; set; }
}

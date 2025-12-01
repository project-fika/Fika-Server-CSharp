namespace FikaWebApp.Services;

public class WebAppConfig
{
    public static string DataPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data");
    public static string DatabasePath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data/database");
    public static string ProtectedFilesPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data/protectedfiles");
    public static string StoredDataPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data/storeddata");
    public static string LogsPath { get; set; } = Path.Combine(AppContext.BaseDirectory, "data/logs");


    public string? APIKey { get; set; }
    public Uri? BaseUrl { get; set; }
    public int HeartbeatInterval { get; set; } = 5;
}

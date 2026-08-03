namespace FikaServer.Models.Fika.Config;

public sealed record FikaPaths(string ModPath)
{
    public string ConfigsPath
    {
        get
        {
            return Path.Join(ModPath, "assets", "configs");
        }
    }

    public string ConfigFilePath
    {
        get
        {
            return Path.Join(ConfigsPath, "fika.jsonc");
        }
    }

    public string DatabasePath
    {
        get
        {
            return Path.Join(ModPath, "database");
        }
    }

    public string ScriptsPath
    {
        get
        {
            return Path.Join(ModPath, "assets", "scripts");
        }
    }

    public string GlobalLocalesPath
    {
        get
        {
            return Path.Join(ModPath, "assets", "database", "locales", "global");
        }
    }
}

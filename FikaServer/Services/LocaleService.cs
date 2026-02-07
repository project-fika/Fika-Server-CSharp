using System.Text.Json;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(FileUtil fileUtil, ConfigService fikaConfig, DatabaseServer databaseServer)
{
    private readonly string _globalLocaleDir = Path.Join(fikaConfig.ModPath, "assets", "database", "locales", "global");
    //private readonly string serverLocaleDir = Path.Join(fikaConfig.GetModPath(), "assets", "database", "locales", "server");

    Dictionary<string, Dictionary<string, string>> _globalLocales = [];

    public async Task OnPostLoadAsync()
    {
        await LoadGlobalLocales();
        LoadServerLocales();
    }

    private async Task LoadGlobalLocales()
    {
        _globalLocales = await RecursiveLoadFiles(_globalLocaleDir);

        foreach ((var locale, var lazyLoadedValue) in databaseServer.GetTables().Locales.Global)
        {
            lazyLoadedValue.AddTransformer(localeData =>
            {
                if (localeData is null)
                {
                    return localeData;
                }

                var fikaLocales = _globalLocales[locale];

                foreach (var fikaLocale in fikaLocales)
                {
                    localeData[fikaLocale.Key] = fikaLocale.Value;
                }

                return localeData;
            });
        }
    }

    private void LoadServerLocales()
    {
        // This is not necessary.. For now..
    }

    private async Task<Dictionary<string, Dictionary<string, string>>> RecursiveLoadFiles(string path)
    {
        var files = fileUtil.GetFiles(path);
        Dictionary<string, Dictionary<string, string>> locales = [];

        foreach (var file in files)
        {
            await using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
            {
                var localeFile = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);

                locales.Add(Path.GetFileNameWithoutExtension(file), localeFile);
            }
        }

        return locales;
    }
}

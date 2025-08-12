using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;
using System.Text.Json;

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

        foreach ((string locale, var lazyLoadedValue) in databaseServer.GetTables().Locales.Global)
        {
            lazyLoadedValue.AddTransformer(localeData =>
            {
                var fikaLocales = _globalLocales[locale];

                foreach (var fikaLocale in fikaLocales)
                {
                    if (localeData.ContainsKey(fikaLocale.Key))
                    {
                        localeData[fikaLocale.Key] = fikaLocale.Value;
                    }
                    else
                    {
                        localeData.Add(fikaLocale.Key, fikaLocale.Value);
                    }
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
        List<string> files = fileUtil.GetFiles(path);
        Dictionary<string, Dictionary<string, string>> locales = [];

        foreach (string file in files)
        {
            await using (FileStream fs = new(file, FileMode.Open, FileAccess.Read))
            {
                Dictionary<string, string>? localeFile = await JsonSerializer.DeserializeAsync<Dictionary<string, string>>(fs);

                locales.Add(Path.GetFileNameWithoutExtension(file), localeFile);
            }
        }

        return locales;
    }
}

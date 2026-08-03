using FikaServer.Models.Fika.Config;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Spt.Tables;
using SPTarkov.Server.Core.Utils;
using System.Text.Json;

namespace FikaServer.Services;

[Injectable(InjectionType.Singleton)]
public class LocaleService(FileUtil fileUtil, FikaPaths fikaPaths, LocaleTable localeTable)
{
    private readonly string _globalLocaleDir = fikaPaths.GlobalLocalesPath;
    //private readonly string serverLocaleDir = Path.Join(fikaConfig.GetModPath(), "assets", "database", "locales", "server");

    Dictionary<string, Dictionary<string, string>> _globalLocales = [];

    public async Task OnPreLoad()
    {
        await LoadGlobalLocales();
        LoadServerLocales();
    }

    private async Task LoadGlobalLocales()
    {
        _globalLocales = await RecursiveLoadFiles(_globalLocaleDir);

        foreach ((var locale, var lazyLoadedValue) in localeTable.Global)
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

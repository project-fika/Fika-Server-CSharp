using SPTarkov.Common.Annotations;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Text.Json;

namespace FikaServer.Services
{
    [Injectable(InjectionType.Singleton)]
    public class LocaleService(ISptLogger<LocaleService> logger, SPTarkov.Server.Core.Services.LocaleService localeService,
        JsonUtil jsonUtil, FileUtil fileUtil, ConfigService fikaConfig)
    {
        private readonly string globalLocaleDir = Path.Join(fikaConfig.GetModPath(), "assets", "database", "locales", "global");
        //private readonly string serverLocaleDir = Path.Join(fikaConfig.GetModPath(), "assets", "database", "locales", "server");

        public void PostSptLoad()
        {
            LoadGlobalLocales();
            LoadServerLocales();
        }

        private void LoadGlobalLocales()
        {
            Dictionary<string, Dictionary<string, string>> locales = RecursiveLoadFiles(globalLocaleDir);

            foreach (var language in locales)
            {
                var languageLocales = language.Value;

                foreach (var locale in languageLocales)
                {
                    localeService.AddCustomClientLocale(language.Key, locale.Key, locale.Value);
                }
            }
        }

        private void LoadServerLocales()
        {
            // This is not necessary.. For now..
        }

        private Dictionary<string, Dictionary<string, string>> RecursiveLoadFiles(string path)
        {
            List<string> files = fileUtil.GetFiles(path);

            Dictionary<string, Dictionary<string, string>> locales = [];

            foreach (string file in files)
            {
                using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    var localeFile = JsonSerializer.Deserialize<Dictionary<string, string>>(fs);

                    locales.Add(Path.GetFileNameWithoutExtension(file), localeFile);
                }
            }

            return locales;
        }
    }
}

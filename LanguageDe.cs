namespace AudioDictionary
{
    internal class LanguageDe : ILanguage
    {
        public string[] WikiBaseUrls => new[]
        {
            "https://de.wiktionary.org/wiki",
            "https://en.wiktionary.org/wiki"
        };

        public string GetLanguageSpecificDcitionaryUrl(string word) => default;
    }
}
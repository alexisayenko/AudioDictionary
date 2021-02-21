namespace AudioDictionary
{
    class LanguageRu : ILanguage
    {
        public string[] WikiBaseUrls => new[]
        {
            "https://ru.wiktionary.org/wiki",
            "https://en.wiktionary.org/wiki"
        };

        public string GetLanguageSpecificDcitionaryUrl(string word) => default;
    }
}

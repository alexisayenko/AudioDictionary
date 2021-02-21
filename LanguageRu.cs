using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string TryToSeparateArticle(string word, out string article)
        {
            article = null;
            return word;
        }
    }
}

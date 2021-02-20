using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDictionary
{
    class DeRuVocabulary : Vocabulary
    {
        protected override string[] Word1WikiBaseUrls => new[] 
        {
            "https://en.wiktionary.org/wiki",
            "https://de.wiktionary.org/wiki"
        };

        protected override string[] Word2WikiBaseUrls => new[]
        {
            "https://ru.wiktionary.org/wiki",
            "https://en.wiktionary.org/wiki"
        };

        protected override string GetWord1SpecificDcitionaryUrl(string word) => null;

        protected override string GetWord2SpecificDcitionaryUrl(string word) => null;
    }
}

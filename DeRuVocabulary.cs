using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDictionary
{
    class DeRuVocabulary : Vocabulary
    {
        protected override string GetWord1SpecificDcitionaryUrl(string word) => null;

        protected override string GetWord2SpecificDcitionaryUrl(string word) => "https://ru.wiktionary.org/wiki";
    }
}

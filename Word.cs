using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDictionary
{
    class Word
    {
        private ILanguage language;

        public Word(ILanguage language, string word)
        {
            this.language = language;

        }
        public string Article { get; }

        public string Value { get; set; }

        public override string ToString()
        {
            return Value;
        }
    }
}

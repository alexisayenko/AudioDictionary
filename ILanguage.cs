using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioDictionary
{
    interface ILanguage
    {
        string[] WikiBaseUrls { get; }
        string GetLanguageSpecificDcitionaryUrl(string word);
    }
}

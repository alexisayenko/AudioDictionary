using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    //class EnRuVocabulary : Vocabulary
    //{
    //    //private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
    //    //private const string UrlEnPostfix = "__us_1.ogg";

    //    protected override string[] Word1WikiBaseUrls => new[]
    //    {
    //        "https://en.wiktionary.org/wiki"
    //    };

    //    protected override string[] Word2WikiBaseUrls => new[]
    //    {
    //        "https://ru.wiktionary.org/wiki",
    //        "https://en.wiktionary.org/wiki"
    //    };

    //    //protected override string GetWord1SpecificDcitionaryUrl(string word) => GetOxfordUrl(word);
    //    protected override string GetWord2SpecificDcitionaryUrl(string word) => null;

    //    //private string GetOxfordUrl(string word)
    //    //{
    //    //    // todo: remove workaround
    //    //    if (word == "condition" || word == "contain")
    //    //        word = $"x{word}";

    //    //    var paddedWord = word.PadRight(5, '_');
    //    //    var part1 = paddedWord.Substring(0, 1);
    //    //    var part2 = paddedWord.Substring(0, 3);
    //    //    var part3 = paddedWord.Substring(0, 5);

    //    //    var result = $"{UlrEnBase}/{part1}/{part2}/{part3}/{word}{UrlEnPostfix}";

    //    //    // todo: remove workaround
    //    //    if (word == "act" || word == "cap")
    //    //        result = result.Replace("us_1.ogg", "us_2.ogg");

    //    //    return result;
    //    //}
    //}
}

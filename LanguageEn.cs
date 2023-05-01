using System.Net.Http;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    class LanguageEn : ILanguage
    {
        private const string UrlBaseOxford = "https://www.oxfordlearnersdictionaries.com/definition/english";
        private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
        private const string UrlEnPostfix = "__us_1.ogg";

        public string[] WikiBaseUrls => new[]
        {
            "https://en.wiktionary.org/wiki"
        };

        public string GetLanguageSpecificDcitionaryUrl(string word) => GetOxfordUrl(word);


        private static string GetOxfordUrl(string word)
        {
            HandleSpecificWordsExceptions(ref word);


            var html = new HttpClient().GetStringAsync($"{UrlBaseOxford}/{word}").Result;

            var matches = Regex.Match(html, "class=\"sound audio_play_button pron-us icon-audio\".+\\.ogg");

            html = matches.Value;

            var i = html.IndexOf("data-src-ogg=\"https://www.oxfordlearnersdictionaries.com/media/english/us_pron");
            html = html.Substring(i + 14);

            return html; 
        }

        private static string GetOxfordUrlObsolete(string word)
        {
            // todo: remove workaround
            if (word == "condition" || word == "contain")
                word = $"x{word}";

            var paddedWord = word.PadRight(5, '_');
            var part1 = paddedWord.Substring(0, 1);
            var part2 = paddedWord.Substring(0, 3);
            var part3 = paddedWord.Substring(0, 5);

            var result = $"{UlrEnBase}/{part1}/{part2}/{part3}/{word}{UrlEnPostfix}";

            // todo: remove workaround
            if (word == "act" || word == "cap")
                result = result.Replace("us_1.ogg", "us_2.ogg");

            return result;
        }

        private static void HandleSpecificWordsExceptions(ref string word)
        {
            if (word == "anytime")
                word = "any-time";
        }
    }
}

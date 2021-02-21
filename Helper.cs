using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    internal static class Helper
    {
        private static readonly WebClient webClient = new WebClient();

        [Obsolete]
        private static void DownloadFiles(Vocabulary vocabulary, string outputFolder, Func<string, string> formUrlFunctor)
        {
            WebClient webClient = new WebClient();

            foreach (var pair in vocabulary)
            {
                var outputFile = $"{outputFolder}/{pair.Lexeme1.Word}.ogg";

                if (File.Exists(outputFile))
                    continue;

                var downloadUrl = formUrlFunctor(pair.Lexeme1.Word);

                webClient.DownloadFile(downloadUrl, outputFile);
            }
        }

        private static void DownloadAudio(Vocabulary vocabulary)
        {
            var counter = 0;
            var total = vocabulary.Count;

            foreach (var pair in vocabulary)
            {
                counter++;

                Console.WriteLine($"{counter}/{total} Downloading words pair '{pair.Lexeme1.Word}'='{pair.Lexeme2.Word}'");

                // Try to separate article and download word1 and word2

                TryDownloadAudio(pair.Lexeme1);
                TryDownloadAudio(pair.Lexeme2);

                pair.Lexeme1.HasWordAudio =
                    Environment.IsOggExist(pair.Lexeme1.Word);

                pair.Lexeme2.HasWordAudio =
                     Environment.IsOggExist(pair.Lexeme2.Word);
            }
        }

        private static void TryDownloadAudio(Lexeme lexeme)
        {
            if (!Environment.IsOggExist(lexeme.Article))
            {
                var urlOggArticle = lexeme.Language.GetArticleUrl(lexeme.Article);
                lexeme.HasArticleAudio = TryDownloadAudio(lexeme.Article, urlOggArticle, webClient);
            }
            else
            {
                lexeme.HasArticleAudio = true;
            }

            if (Environment.IsOggExist(lexeme.Word))
                return;

            var urlOggWord = GetOggUrl(lexeme.Language, lexeme.Word);
            lexeme.HasWordAudio = TryDownloadAudio(lexeme.Word, urlOggWord, webClient);
        }

        private static bool TryDownloadAudio(string word, string url, WebClient webClient)
        {
            if (string.IsNullOrEmpty(word))
                return false;

            if (string.IsNullOrEmpty(url))
            {
                // Console.WriteLine($"[!] Skipping {word} due to missing audio file.");
                return false;
            }

            Console.WriteLine($"  Downloading '{word}'");
            webClient.DownloadFile(url, Environment.GetWorkingPathToOgg(word));
            return true;
        }

        /// <summary>
        /// First, tries to download from language specific dicitionary (E.g. Oxford), then from wikitionary.
        /// </summary>
        /// <param name="wordSpecificDictionaryFunctor"></param>
        /// <param name="wikiBaseUrls"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        private static string GetOggUrl(ILanguage language, string word)
        {
            if (string.IsNullOrEmpty(word))
                return default;

            var urlOgg = language.GetLanguageSpecificDcitionaryUrl(word);
            if (!string.IsNullOrEmpty(urlOgg))
                return urlOgg;

            foreach (var wikiBaseUrl in language.WikiBaseUrls)
            {
                urlOgg = GetWikiUrl(wikiBaseUrl, word);
                if (!string.IsNullOrEmpty(urlOgg))
                    return urlOgg;
            }

            Console.WriteLine($"[!] Url for '{word}' was not found.");
            return default;
        }

        private static string GetWikiUrl(string baseUrl, string word)
        {
            string urlOgg = null;

            try
            {
                var html = webClient.DownloadString($"{baseUrl}/{word}");
                var matches = Regex.Match(html, "<source src=\"//upload.wikimedia.org/wikipedia/commons.+\\.ogg\" type");

                //if (!matches.Success)
                //    matches = Regex.Match(html, "//upload.wikimedia.org/wikipedia/commons.+\\.ogg");

                html = matches.Value;

                if (html.Contains("type=\"audio/mpeg\""))
                {
                    var r = Regex.Match(html, "<source src=.+<source src=");
                    html = html.Remove(0, r.Length - 12);
                }

                urlOgg = html.Replace("<source src=\"", "https:").Replace("\" type", string.Empty);
            }
            catch (Exception)
            {
                //// Ignore exceptions
            }

            return urlOgg;
        }

        private static Vocabulary ReadFilesListToDownload(string fileName, string audioPattern)
        {
            var languagesCodes = ParseVocabularyType(audioPattern.Substring(0, 4));

            var result = new Vocabulary();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    var pair = new DictionaryPair(languagesCodes.Item1, keyvalue[0], languagesCodes.Item2, keyvalue[1]);
                    result.Add(pair);
                }
            }

            return result;
        }

        /// <summary>
        /// Languages codes entries according to ISO 3166-2.
        /// </summary>
        /// <param name="languages"></param>
        /// <returns></returns>
        private static Tuple<ILanguage, ILanguage> ParseVocabularyType(string languagesCodes)
        {
            if (string.IsNullOrEmpty(languagesCodes) ||
                string.IsNullOrWhiteSpace(languagesCodes) ||
                languagesCodes.Length != 4)
                throw new ApplicationException("Length of language ");

            var code1 = languagesCodes.Substring(0, 2).ToUpper();
            var code2 = languagesCodes.Substring(2, 2).ToUpper();

            var stringToVocabularyTypeDictionary = new Dictionary<string, Func<ILanguage>>
            {
                { "DE", () => new LanguageDe() },
                { "EN", () => new LanguageEn() },
                { "RU", () => new LanguageRu() }
            };

            if (stringToVocabularyTypeDictionary.TryGetValue(code1, out Func<ILanguage> functor1) == false)
                throw new ApplicationException("Language not found. Specify language according to ISO 3166-2 standard.");

            if (stringToVocabularyTypeDictionary.TryGetValue(code2, out Func<ILanguage> functor2) == false)
                throw new ApplicationException("Language not found. Specify language according to ISO 3166-2 standard.");

            return new Tuple<ILanguage, ILanguage>(functor1(), functor2());
        }

        internal static void GenerateOutputMp3(string inputWordsFile, string outputMp3File, string audioPattern)
        {
            // 1. Get list of files to download
            var pairsList = ReadFilesListToDownload(inputWordsFile, audioPattern);

            // 2. Download files
            DownloadAudio(pairsList);

            // 3. Convert .ogg to .mp3 files
            Console.WriteLine();
            Console.WriteLine("Converting donwloaded OGG files to MP3");
            AudioTool.ConvertDownloadedAudio(pairsList);

            // 4. Normalize .mp3 files
            AudioTool.NormalizeAudio(pairsList);

            // 5. Merge all files into one result mp3
            Console.WriteLine();
            Console.WriteLine("Merging all files into one result MP3");
            AudioTool.MergeFiles(pairsList, outputMp3File, audioPattern);

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Done. {pairsList.Count(w => w.HasAudio)} words has been merged.");
        }
    }
}

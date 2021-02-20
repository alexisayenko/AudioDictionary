using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    class EnRuVocabulary : Vocabulary
    {
        private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
        private const string UrlEnPostfix = "__us_1.ogg";
        private const string UrlWikiBaseOgg = "https://upload.wikimedia.org/wikipedia/commons";
        private const string UrlRuWikiBaseHtml = "https://ru.wiktionary.org/wiki";
        private const string UrlEnWikiBaseHtml = "https://en.wiktionary.org/wiki";
        
        public EnRuVocabulary(string silence05sec) 
        {
            Silence05sec = silence05sec;
        }

        public EnRuWordsList WordsList { get; set; }

        [Obsolete]
        private static void DownloadFiles(EnRuWordsList wordsList, string outputFolder, Func<string, string> formUrlFunctor)
        {
            WebClient webClient = new WebClient();

            foreach (var word in wordsList)
            {
                var outputFile = $"{outputFolder}/{word.English}.ogg";

                if (File.Exists(outputFile))
                    continue;

                var downloadUrl = formUrlFunctor(word.English);

                webClient.DownloadFile(downloadUrl, outputFile);
            }
        }

        public void DownloadAudio(EnRuWordsList wordsList)
        {
            WebClient webClient = new WebClient();

            var counter = 0;
            var total = wordsList.Count;

            foreach (var word in wordsList)
            {
                counter++;

                // Check if file exists
                var wordEn = word.English;
                var wordRu = word.Russian;

                if (File.Exists(Path.Combine(WorkingDirectory, $"{wordEn}.ogg")) &&
                    File.Exists(Path.Combine(WorkingDirectory, $"{wordRu}.ogg")))
                {
                    Console.WriteLine($"{counter}/{total} Skipping words pair '{wordEn}={wordRu}' as files are already downloaded");
                    word.HasAudio = true;
                    continue;
                }

                // First, download En word from En wiki and Oxford
                var urlOggEn = GetOxfordUrl(wordEn);
                if (string.IsNullOrEmpty(urlOggEn))
                    urlOggEn = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordEn);

                if (string.IsNullOrEmpty(urlOggEn))
                    Console.WriteLine("---> Not Found");

                // Then download Ru word from En and Ru wiki
                var urlOggRu = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordRu);
                if (string.IsNullOrEmpty(urlOggRu))
                    urlOggRu = GetWikiUrl(webClient, UrlRuWikiBaseHtml, wordRu);

                if (string.IsNullOrEmpty(urlOggRu) || string.IsNullOrEmpty(urlOggEn))
                {
                    Console.WriteLine($"{counter}/{total} [!] Skipping words pair '{wordEn}'='{wordRu}' due to missing audio files");
                    continue;
                }

                Console.WriteLine($"{counter}/{total} Downloading words pair '{wordEn}'='{wordRu}'");

                webClient.DownloadFile(urlOggEn, Path.Combine(WorkingDirectory, $"{wordEn}.ogg"));
                webClient.DownloadFile(urlOggRu, Path.Combine(WorkingDirectory, $"{wordRu}.ogg"));
                word.HasAudio = true;
            }
        }

        private static string GetWikiUrl(WebClient webClient, string baseUrl, string word)
        {
            string urlOgg = null;

            try
            {
                var html = webClient.DownloadString($"{baseUrl}/{word}");
                var matches = Regex.Match(html, "<source src=\"//upload.wikimedia.org/wikipedia/commons.+\\.ogg\" type");

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
                // Ignore exceptions
            }

            return urlOgg;
        }

        private static string GetOxfordUrl(string word)
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

        private static EnRuWordsList ReadFilesListToDownload(string fileName)
        {
            var result = new EnRuWordsList();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    var word = new EnRuWord();
                    word.English = keyvalue[0].Trim();
                    word.Russian = keyvalue[1].Trim();

                    result.Add(word);
                }
            }

            return result;
        }

        internal override void GenerateAudioFile()
        {
            var audioTool = new AudioTool(WorkingDirectory, Silence05sec);

            // 1. Get list of files to download
            var wordsList = ReadFilesListToDownload(Environment.WordsFile);

            // 2. Download files
            DownloadAudio(wordsList);

            // 3. Convert .ogg to .mp3 files
            Console.WriteLine();
            Console.WriteLine("Converting donwloaded OGG files to MP3");
            audioTool.ConvertDownloadedAudio(wordsList);

            // 4. Normalize .mp3 files
            audioTool.NormalizeAudio(wordsList);

            // 5. Merge all files into one result mp3
            Console.WriteLine();
            Console.WriteLine("Merging all files into one result MP3");
            audioTool.MergeFiles(wordsList, Path.Combine(WorkingDirectory, Environment.OutputResultMp3));

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Done. {wordsList.Count(w => w.HasAudio)} words has been merged.");
        }
    }
}

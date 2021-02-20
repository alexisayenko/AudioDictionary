using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    abstract class Vocabulary
    {
        private readonly WebClient webClient = new WebClient();

        protected abstract string[] Word1WikiBaseUrls { get; }
        protected abstract string[] Word2WikiBaseUrls { get; }

        // public VocabularyType Type { get; private set; }

        //public Vocabulary(VocabularyType type)
        //{
        //    this.Type = type;
        //}

        public string WorkingDirectory => Environment.WorkingDirectory;
        public string Silence05sec => Environment.SilenceFile;
        public WordPairList WordsList { get; set; }

        public static Vocabulary Create(VocabularyType type)
        {
            switch (type)
            {
                case VocabularyType.EnRuTranslation:
                    return new EnRuVocabulary();
                case VocabularyType.DeRuTranslation:
                    return new DeRuVocabulary();
                case VocabularyType.DeSingularPlural:
                    break;
                default:
                    break;
            }

            return null;
        }

        [Obsolete]
        private static void DownloadFiles(WordPairList wordsList, string outputFolder, Func<string, string> formUrlFunctor)
        {
            WebClient webClient = new WebClient();

            foreach (var word in wordsList)
            {
                var outputFile = $"{outputFolder}/{word.Word1}.ogg";

                if (File.Exists(outputFile))
                    continue;

                var downloadUrl = formUrlFunctor(word.Word1);

                webClient.DownloadFile(downloadUrl, outputFile);
            }
        }

        public void DownloadAudio(WordPairList wordPairList)
        {
            var counter = 0;
            var total = wordPairList.Count;

            foreach (var pair in wordPairList)
            {
                counter++;

                // Check if file exists
                if (File.Exists(Path.Combine(WorkingDirectory, $"{pair.Word1}.ogg")) &&
                    File.Exists(Path.Combine(WorkingDirectory, $"{pair.Word2}.ogg")))
                {
                    Console.WriteLine($"{counter}/{total} Skipping words pair '{pair.Word1}={pair.Word2}' as files are already downloaded");
                    pair.HasAudio = true;
                    continue;
                }

                //var urlOggEn = GetWord1SpecificDcitionaryUrl(pair.Word1);
                //if (string.IsNullOrEmpty(urlOggEn))
                //    urlOggEn = GetWikiUrl(Word1WikiBaseUrl, pair.Word1);

                //if (string.IsNullOrEmpty(urlOggEn))
                //    Console.WriteLine("---> Not Found");

                var urlOggEn = GetOggUrl(GetWord1SpecificDcitionaryUrl, Word1WikiBaseUrls, pair.Word1);

                // Then download Ru word from En and Ru wiki
                //var urlOggRu = GetWikiUrl(Word1WikiBaseUrl, pair.Word2);
                //if (string.IsNullOrEmpty(urlOggRu))
                //    urlOggRu = GetWikiUrl(Word2WikiBaseUrl, pair.Word2);
                var urlOggRu = GetOggUrl(GetWord2SpecificDcitionaryUrl, Word2WikiBaseUrls, pair.Word2);

                if (string.IsNullOrEmpty(urlOggRu) || string.IsNullOrEmpty(urlOggEn))
                {
                    Console.WriteLine($"{counter}/{total} [!] Skipping words pair '{pair.Word1}'='{pair.Word2}' due to missing audio files");
                    continue;
                }

                Console.WriteLine($"{counter}/{total} Downloading words pair '{pair.Word1}'='{pair.Word2}'");

                webClient.DownloadFile(urlOggEn, Path.Combine(WorkingDirectory, $"{pair.Word1}.ogg"));
                webClient.DownloadFile(urlOggRu, Path.Combine(WorkingDirectory, $"{pair.Word2}.ogg"));
                pair.HasAudio = true;
            }
        }

        /// <summary>
        /// First, tries to download from language specific dicitionary (E.g. Oxford), then from wikitionary.
        /// </summary>
        /// <param name="wordSpecificDictionaryFunctor"></param>
        /// <param name="wikiBaseUrls"></param>
        /// <param name="word"></param>
        /// <returns></returns>
        private string GetOggUrl(Func<string,string> wordSpecificDictionaryFunctor, string[] wikiBaseUrls, string word)
        {
            var urlOgg = wordSpecificDictionaryFunctor(word);
            if (!string.IsNullOrEmpty(urlOgg))
                return urlOgg;

            foreach(var wikiBaseUrl in wikiBaseUrls)
            {
                urlOgg = GetWikiUrl(wikiBaseUrl, word);
                if (!string.IsNullOrEmpty(urlOgg))
                    return urlOgg;
            }

            Console.WriteLine("---> Not Found");
            return default;
        }

        private string GetWikiUrl(string baseUrl, string word)
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

        protected static WordPairList ReadFilesListToDownload(string fileName)
        {
            var result = new WordPairList();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    var word = new WordPair();
                    word.Word1 = keyvalue[0].Trim();
                    word.Word2 = keyvalue[1].Trim();

                    result.Add(word);
                }
            }

            return result;
        }

        /// <summary>
        /// Use this method to get URL of online dictionary specifc to Word1.
        /// You should use the dictionary that suits the language of the word better than wikitionary. 
        /// For example, Oxford dictionary for English words.
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        protected abstract string GetWord1SpecificDcitionaryUrl(string word);

        /// <summary>
        /// Use this method to get URL of online dictionary specifc to Word2.
        /// You should use the dictionary that suits the language of the word better than wikitionary. 
        /// For example, Oxford dictionary for English words.  
        /// </summary>
        /// <param name="word"></param>
        /// <returns></returns>
        protected abstract string GetWord2SpecificDcitionaryUrl(string word);

        internal virtual void GenerateAudioFile()
        {
            var audioTool = new AudioTool(WorkingDirectory, Environment.SilenceFile);

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

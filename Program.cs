using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Text;
using Utility.CommandLine;

namespace AudioDictionary
{
    class Program
    {
        [Argument('w', "words-file", "Full path to logs file")]
        private static string WordsFile { get; set; }

        [Argument('o', "output-file", "Output MP3 file")]
        private static string OutputMp3File { get; set; }

        [Argument('l', "languages", "Languages codes according to ISO 3166-2")]
        private static string Languages { get; set; }

        private static void InitializeVariables(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (Environment.IsLinux == false)
                MediaFoundationInterop.MFStartup(0);

            WordsFile = WordsFile ?? @"/tmp/words-list.txt";
            OutputMp3File = OutputMp3File ?? "!result.mp3";
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

        static void Main(string[] args)
        {
            try
            {
                main(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }

        private static void main(string[] args)
        {
            Arguments.Populate();

            InitializeVariables(args);

            Console.WriteLine($"Reading words list from {WordsFile}");

            var languagesCodes = ParseVocabularyType(Languages ?? "EnRu");

            var vocabulary =
                new Vocabulary(languagesCodes.Item1, languagesCodes.Item2);

            vocabulary.GenerateAudioFile(WordsFile, OutputMp3File);
        }
    }
}

using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AudioDictionary
{
    class Program
    {
        private static void InitializeVariables(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (Environment.IsLinux == false)
                MediaFoundationInterop.MFStartup(0);

            Environment.SetSilenceFile(Path.Combine("Files", "silence-0.5s.mp3"));
            Environment.WordsFile = args.Length > 0 ? args[0] : @"/tmp/words-list.txt";
            Environment.SetOutputResultMp3(args.Length > 1 ? args[1] : "!result.mp3");
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
                InitializeVariables(args);

                Console.WriteLine($"Reading words list from {Environment.WordsFile}");

                var languagesCodes = args.Length > 2 ? ParseVocabularyType(args[2]) : new Tuple<ILanguage, ILanguage>(new LanguageEn(), new LanguageRu());

                var vocabulary =
                    new Vocabulary(languagesCodes.Item1, languagesCodes.Item2);

                vocabulary.GenerateAudioFile();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}

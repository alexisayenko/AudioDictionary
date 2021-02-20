using NAudio.MediaFoundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace AudioDictionary
{
    class Program
    {
        private static VocabularyType vocabularyType;

        private static void InitializeVariables(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (Environment.IsLinux == false)
                MediaFoundationInterop.MFStartup(0);

            Environment.SilenceFile = "silence-0.5s.mp3";
            Environment.WordsFile = args.Length > 0 ? args[0] : @"/tmp/words-list.txt";
            Environment.OutputResultMp3 = args.Length > 1 ? args[1] : "!result.mp3";
            vocabularyType = args.Length > 2 ? ParseVocabularyType(args[2]) : default;
        }

        private static VocabularyType ParseVocabularyType(string vocabulartyType)
        {
            var stringToVocabularyTypeDictionary = new Dictionary<string, VocabularyType>
            {
                { "EnRu", VocabularyType.EnRuTranslation },
                { "DeRu", VocabularyType.DeRuTranslation }
            };

            if (stringToVocabularyTypeDictionary.TryGetValue(vocabulartyType, out VocabularyType result))
                return result;

            return default;
        }

        static void Main(string[] args)
        {
            InitializeVariables(args);

            Console.WriteLine($"Reading words list from {Environment.WordsFile}");

            var vocabulary = 
                Vocabulary.Create(vocabularyType);

            vocabulary.GenerateAudioFile();
        }
    }
}

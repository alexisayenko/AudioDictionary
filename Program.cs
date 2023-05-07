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

        [Argument('p', "audio-pattern", "Audio pattern for generating")]
        private static string AudioPattern { get; set; }

        private static void InitializeVariables(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (Environment.IsLinux == false)
                MediaFoundationInterop.MFStartup(0);

            WordsFile ??= @"C:\Alex\AudioDictionary\Files\words-list-en-ru.txt";
                // @"/tmp/words-list.txt";

            OutputMp3File ??= "!result.mp3";
            AudioPattern ??=
                "EnRu 2..1.1.1.1.1...";
            //"RuDe 1..2.2.2.2.2...";
            //"DeDe 1..2...";
            //"EnRu 1..2.1.2.1.2...";
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

            Helper.GenerateOutputMp3(WordsFile, OutputMp3File, AudioPattern);
        }
    }
}

using NAudio.MediaFoundation;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Diagnostics;
using System.Threading;

namespace AudioDictionary
{
    class Program
    {

        private static readonly string WorkingDirectory;
        private const string Silence05sec = "silence-0.5s.mp3";

        static Program()
        {
            WorkingDirectory = EnvironmentTool.IsLinux ? @"/srv/audio-dictionary" : @"C:\Temp\AudioDictionary";

        }

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            if (EnvironmentTool.IsLinux == false)
                MediaFoundationInterop.MFStartup(0);

            var wordsFile = args.Length > 0 ? args[0] : @"/tmp/words-list.txt";
            var outputResultMp3 = args.Length > 1 ? args[1] : "!result.mp3";

            Console.WriteLine($"Reading words list from {wordsFile}");

            // 1. Get list of files to download
            var wordsList = ReadFilesListToDownload(wordsFile);

            var enRuVocabulary = new EnRuVocabulary(wordsList, WorkingDirectory, Silence05sec);

            // 2. Download files
            enRuVocabulary.DownloadAudio(wordsList);

            var audioTool = new AudioTool(WorkingDirectory, Silence05sec);

            // 3. Convert .ogg to .mp3 files
            Console.WriteLine();
            Console.WriteLine("Converting donwloaded OGG files to MP3");
            audioTool.ConvertDownloadedAudio(wordsList);

            // 4. Normalize .mp3 files
            audioTool.NormalizeAudio(wordsList);

            // 5. Merge all files into one result mp3
            Console.WriteLine();
            Console.WriteLine("Merging all files into one result MP3");
            audioTool.MergeFiles(wordsList, Path.Combine(WorkingDirectory, outputResultMp3));

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Done. {wordsList.Count(w => w.HasAudio)} words has been merged.");
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

    }
}

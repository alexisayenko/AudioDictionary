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

namespace AudioDictionary
{
    class Program
    {
        private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
        private const string UrlEnPostfix = "__us_1.ogg";
        private const string UrlWikiBaseOgg = "https://upload.wikimedia.org/wikipedia/commons";
        private const string UrlRuWikiBaseHtml = "https://ru.wiktionary.org/wiki";
        private const string UrlEnWikiBaseHtml = "https://en.wiktionary.org/wiki";
        private const string WorkingDirectory = @"C:\Temp\AudioPlaying";
        private const string Silence05sec = "silence-0.5s.mp3";

        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            MediaFoundationInterop.MFStartup(0);

            // 1. Get list of files to download
            var wordsList = ReadFilesListToDownload(@"C:\Temp\AudioPlaying\words-list.txt");

            // 2. Download files
            DownloadAudio(wordsList);

            // 3. Convert .ogg to .mp3 files
            ConvertDownloadedAudio(wordsList);

            // 4. Normalize .mp3 files
            NormalizeAudio(wordsList);

            // 5. Merge all files into one result mp3
            MergeFiles(wordsList, "result.mp3");

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Done. {wordsList.Count(w => w.HasAudio)} words has been merged.");

            Console.ReadKey();
        }

        private static void NormalizeAudio(List<Word> wordsList)
        {
            // Trim empy sound
            // Normalize Volume
            // Speed/Tempo?
        }

        private static void MergeFiles(List<Word> wordsList, string outputFile)
        {
            var outputStream = new FileStream($@"{WorkingDirectory}\{outputFile}", FileMode.Create);

            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                var fileName = $@"{WorkingDirectory}\{word.Russian}.mp3";
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);
                MergeFile(Silence05sec, outputStream);

                fileName = $@"{WorkingDirectory}\{word.English}.mp3";
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);
                MergeFile(fileName, outputStream);
                MergeFile(Silence05sec, outputStream);

                MergeFile(Silence05sec, outputStream);
                MergeFile(Silence05sec, outputStream);
            }
        }


        private static void MergeFile(string fileName, FileStream outputStream)
        {
            var reader = new Mp3FileReader(fileName);

            if ((outputStream.Position == 0) && (reader.Id3v2Tag != null))
            {
                outputStream.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
            }
            Mp3Frame frame;
            while ((frame = reader.ReadNextFrame()) != null)
            {
                outputStream.Write(frame.RawData, 0, frame.RawData.Length);
            }
        }

        private static void ConvertDownloadedAudio(List<Word> wordsList)
        {
            foreach (var word in wordsList)
            {
                if (!word.HasAudio)
                    continue;

                word.HasAudio = ConvertOggToMp3($"{WorkingDirectory}\\{word.English}.ogg");
                word.HasAudio &= ConvertOggToMp3($"{WorkingDirectory}\\{word.Russian}.ogg");
            }
        }

        private static bool ConvertOggToMp3(string fileName)
        {
            try
            {
                var oggReader = new VorbisWaveReader(fileName);

                MediaFoundationEncoder.EncodeToMp3(oggReader, fileName.Replace(".ogg", ".mp3"), 128000);
            }
            catch (Exception)
            {
                Console.WriteLine($"(!) No Suitable Encoder for {Path.GetFileNameWithoutExtension(fileName)}");
                return false;
            }

            return true;
        }

        private static void DownloadFiles(List<Word> wordsList, string outputFolder, Func<string, string> formUrlFunctor)
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

        private static void DownloadAudio(List<Word> wordsList)
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

                if (File.Exists($"{WorkingDirectory}\\{wordEn}.ogg") && File.Exists($"{WorkingDirectory}\\{wordRu}.ogg"))
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

                webClient.DownloadFile(urlOggEn, $"{WorkingDirectory}\\{wordEn}.ogg");
                webClient.DownloadFile(urlOggRu, $"{WorkingDirectory}\\{wordRu}.ogg");
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

        private static List<Word> ReadFilesListToDownload(string fileName)
        {
            var result = new List<Word>();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    var word = new Word();
                    word.English = keyvalue[0].Trim();
                    word.Russian = keyvalue[1].Trim();

                    result.Add(word);
                }
            }

            return result;
        }

        static void MergeOgg(string[] inputFiles, string outputFile)
        {
            // MediaFoundationInterop.MFStartup();

            var vorbisReader = new VorbisWaveReader("bias__us_1.ogg");

            MediaFoundationEncoder.EncodeToMp3(vorbisReader, outputFile);

            var outputStream = new FileStream("output.mp3", FileMode.Create);


            byte[] array = new byte[255];
            var span = new Span<byte>(array);
            vorbisReader.Read(span);

            outputStream.Write(span);
            //Mp3Frame frame;
            //while ((frame = reader.ReadNextFrame()) != null)
            //{
            //    outputStream.Write(frame.RawData, 0, frame.RawData.Length);
            //}
        }

        public static void Combine(string[] inputFiles, string outputFile)
        {
            var outputStream = new FileStream(outputFile, FileMode.Create);

            foreach (string file in inputFiles)
            {
                // new WaveProvider
                // MediaFoundationEncoder.EncodeToMp3(file,);



                Mp3FileReader reader = new Mp3FileReader(file);
                if ((outputStream.Position == 0) && (reader.Id3v2Tag != null))
                {
                    outputStream.Write(reader.Id3v2Tag.RawData, 0, reader.Id3v2Tag.RawData.Length);
                }
                Mp3Frame frame;
                while ((frame = reader.ReadNextFrame()) != null)
                {
                    outputStream.Write(frame.RawData, 0, frame.RawData.Length);
                }
            }
        }
    }
}

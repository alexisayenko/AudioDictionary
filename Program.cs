using NAudio.MediaFoundation;
using NAudio.Vorbis;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace AudioDictionary
{
    class Program
    {
        private const string UlrEnBase = "https://www.oxfordlearnersdictionaries.com/us/media/english/us_pron_ogg";
        private const string UrlEnPostfix = "__us_1.ogg";
        private const string UrlWikiBaseOgg = "https://upload.wikimedia.org/wikipedia/commons";
        private const string UrlRuWikiBaseHtml = "https://ru.wiktionary.org/wiki";
        private const string UrlEnWikiBaseHtml = "https://en.wiktionary.org/wiki";


        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            MediaFoundationInterop.MFStartup(0);

            // 1. Get list of files to download
            var filesListToDownload = ReadFilesListToDownload(@"C:\Temp\AudioPlaying\words-list.txt");

            // 2. Download files
            /// DownloadFiles(filesListToDownload, @"C:\Temp\AudioPlaying", FormUrlOxford);
            DownloadAudioFromWiki(filesListToDownload, @"C:\Temp\AudioPlaying");

            // 3. Convert .ogg to .mp3 files
            ConvertDownloadedOggToMp3(@"C:\Temp\AudioPlaying");

            // 4. Merge all files into one result mp3
            MergeFiles(@"C:\Temp\AudioPlaying", @"C:\Temp\AudioPlaying\result.mp3");

            Console.ReadKey();
        }

        private static void MergeFiles(string folderPath, string outputFile)
        {
            var inputFiles = Directory.GetFiles(folderPath, "*.mp3");

            var outputStream = new FileStream(outputFile, FileMode.Create);

            foreach (string file in inputFiles)
            {
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

        private static void ConvertDownloadedOggToMp3(string folderPath)
        {
            var oggFiles = Directory.GetFiles(folderPath, "*.ogg");
            ConvertOggToMp3(oggFiles);
        }

        private static void DownloadFiles(Dictionary<string, string> filesListToDownload, string outputFolder, Func<string, string> formUrlFunctor)
        {
            WebClient webClient = new WebClient();

            foreach (var word in filesListToDownload.Keys)
            {
                var outputFile = $"{outputFolder}/{word}.ogg";

                if (File.Exists(outputFile))
                    continue;

                var downloadUrl = formUrlFunctor(word);

                webClient.DownloadFile(downloadUrl, outputFile);
            }
        }

        private static void DownloadAudioFromWiki(Dictionary<string, string> filesListToDownload, string outputFolder)
        {
            WebClient webClient = new WebClient();

            foreach (var key in filesListToDownload.Keys)
            {
                // First, download En word from En wiki
                var wordEn = key;

                var urlOggEn = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordEn);
                if (string.IsNullOrEmpty(urlOggEn))
                    urlOggEn = GetOxfordUrl(wordEn);

                if (string.IsNullOrEmpty(urlOggEn))
                    Console.WriteLine("---> Not Found");


                // Then download Ru word from En wiki
                var wordRu = filesListToDownload[key];
                var urlOggRu = GetWikiUrl(webClient, UrlEnWikiBaseHtml, wordRu);
                if (string.IsNullOrEmpty(urlOggRu))
                    urlOggRu = GetWikiUrl(webClient, UrlRuWikiBaseHtml, wordRu);

                if (string.IsNullOrEmpty(urlOggRu) || string.IsNullOrEmpty(urlOggEn))
                {
                    Console.WriteLine($"Skipping words pair '{wordEn}'='{wordRu}' due to missing audio files");
                    continue;
                }

                Console.WriteLine($"Downloading words pair '{wordEn}'='{wordRu}'");

                webClient.DownloadFile(urlOggEn, $"{outputFolder}\\{wordEn}.ogg");
                webClient.DownloadFile(urlOggRu, $"{outputFolder}\\{wordRu}.ogg");
            }
        }

        private static string GetWikiUrl(WebClient webClient, string baseUrl, string word)
        {
            string urlOgg = null;

            try
            {
                var html = webClient.DownloadString($"{baseUrl}/{word}");
                var matches = Regex.Match(html, "<source src=\"//upload.wikimedia.org/wikipedia/commons.+\\.ogg\" type");
                urlOgg = matches.Value.Replace("<source src=\"", "https:").Replace("\" type", string.Empty);
            }
            catch (Exception)
            {
                // Ignore exceptions
            }

            return urlOgg;
        }

        private static string GetOxfordUrl(string word)
        {
            var paddedWord = word.PadRight(5, '_');
            var part1 = paddedWord.Substring(0, 1);
            var part2 = paddedWord.Substring(0, 3);
            var part3 = paddedWord.Substring(0, 5);

            var result = $"{UlrEnBase}/{part1}/{part2}/{part3}/{word}{UrlEnPostfix}";

            return result;
        }

        private static Dictionary<string, string> ReadFilesListToDownload(string fileName)
        {
            var result = new Dictionary<string, string>();

            var content = File.ReadAllLines(fileName);

            foreach (string line in content)
            {
                string[] keyvalue = line.Split('=');
                if (keyvalue.Length == 2)
                {
                    result.Add(keyvalue[0].Trim(), keyvalue[1].Trim());
                }
            }

            return result;
        }

        static void ConvertOggToMp3(string[] fileNames)
        {
            foreach (var fileName in fileNames)
            {
                var oggReader = new VorbisWaveReader(fileName);
                MediaFoundationEncoder.EncodeToMp3(oggReader, fileName.Replace(".ogg", ".mp3"));
            }
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
